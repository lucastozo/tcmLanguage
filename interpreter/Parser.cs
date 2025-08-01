class Parser
{
    internal static HashSet<int> SubroutineAddresses { get; private set; } = new HashSet<int>();

    public static List<Instruction> GetInstructions(string pathToFile)
    {
        Log.PrintMessage("[PARSER] initiating");

        Dictionary<string, byte> constants = new Dictionary<string, byte>();
        Dictionary<string, int> labels = new Dictionary<string, int>();
        Dictionary<string, int> subroutines = new Dictionary<string, int>();
        List<string> rawInstructionLines = new List<string>();
        List<int> originalLineNumbers = new List<int>(); // Original line numbers (og file used as reference)
        
        // Pragma settings
        bool allowOverflow = false;

        string[] lines = File.ReadAllLines(pathToFile);
        int instructionIndex = 0;

        SubroutineAddresses.Clear(); // Because its static, could cause a bug if run multiple programs

        // Preprocess
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            int commentIdx = line.IndexOf('#');
            if (commentIdx >= 0) line = line[..commentIdx];
            line = string.Join(" ", line.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] parts = line.Split(' ');

            // Pragma directives
            if (parts[0].Equals("@pragma", StringComparison.OrdinalIgnoreCase))
            {
                if (parts.Length != 2) throw new Exception($"Invalid pragma directive at line {i + 1}. Expected: @pragma <option>");
                
                string pragmaOption = parts[1].ToLowerInvariant();
                switch (pragmaOption)
                {
                    case "allow_overflow":
                        allowOverflow = true;
                        Log.PrintMessage($"[PARSER] Pragma: overflow behavior enabled");
                        break;
                    case "disallow_overflow":
                        allowOverflow = false;
                        Log.PrintMessage($"[PARSER] Pragma: overflow behavior disabled");
                        break;
                    default:
                        throw new Exception($"Unknown pragma option '{parts[1]}' at line {i + 1}.");
                }
                continue;
            }

            if (parts[0].Equals("const", StringComparison.OrdinalIgnoreCase))
            {
                if (parts.Length != 3) throw new Exception($"Invalid const declaration at line {i + 1}");
                if (!byte.TryParse(parts[2], out byte constVal))
                {
                    throw new Exception($"Invalid const value at line {i + 1}");
                }

                constants[parts[1]] = constVal;
                continue;
            }

            if (parts[0].Equals("label", StringComparison.OrdinalIgnoreCase))
            {
                if (parts.Length != 2) throw new Exception($"Invalid label at line {i + 1}");

                /* 
                -- ABOUT LABEL AND INSTRUCTION INDEX * 4 --
                    Turing Complete game treat label address as a single byte,
                    instead of a index of instruction.
                    So if subroutine is the 5th instruction, adress will be 4*4 = 16,
                    because the 16th byte is the first byte of the 5th instruction.
                    Kinda counterintuitive, but that's how the game works.
                    This is why we multiply instruction index by 4.
                */
                if (instructionIndex * 4 > byte.MaxValue)
                {
                    throw new Exception("Program exceeds maximum addressable memory of 256 bytes.");
                }
                labels.Add(parts[1], instructionIndex * 4);

                continue;
            }

            if (parts[0].Equals("subroutine", StringComparison.OrdinalIgnoreCase))
            {
                if (parts.Length != 2) throw new Exception($"Invalid subroutine declaration at line {i + 1}");

                if (instructionIndex * 4 > byte.MaxValue)
                {
                    throw new Exception("Program exceeds maximum addressable memory of 256 bytes.");
                }

                int address = instructionIndex * 4;
                subroutines.Add(parts[1], address);
                SubroutineAddresses.Add(address); // Store for interpreter access
                Log.PrintMessage($"[PARSER] Subroutine '{parts[1]}' registered at address {address}");

                continue;
            }

            rawInstructionLines.Add(line);
            originalLineNumbers.Add(i + 1);
            Log.PrintMessage($"Line {line} preprocessed sucessfully");
            instructionIndex++;
        }

        // Build instructions
        List<Instruction> instructions = new List<Instruction>();

        for (int i = 0; i < rawInstructionLines.Count; i++)
        {
            int originalLineNum = originalLineNumbers[i];
            string[] parts = rawInstructionLines[i].Split(' ');
            List<byte> bParts = new();

            foreach (var part in parts)
            {
                byte value = EvaluateExpression(part, constants, labels, subroutines, originalLineNum, allowOverflow);
                bParts.Add(value);
            }

            if (bParts.Count == 1)
            {
                byte opcode = bParts[0];

                if (opcode >= Opcodes.SYSTEM_INSTRUCTION_START)
                {
                    bParts.AddRange([0, 0, 0]); // Dummy values
                }
                else
                {
                    throw new Exception($"Invalid instruction at line {originalLineNum}: not a valid zero-operand instruction");
                }
            }
            else if (bParts.Count != 4)
            {
                throw new Exception($"Invalid instruction at line {originalLineNum}: must have exactly 4 operands or be a valid zero-operand instruction");
            }

            instructions.Add(new Instruction(bParts[0], bParts[1], bParts[2], bParts[3]));
        }

        if (Log.ShowLogs)
        {
            string programMsg = "Final program:\n";
            foreach (Instruction instr in instructions)
            {
                programMsg += $"{instr.Opcode} {instr.Arg1} {instr.Arg2} {instr.Destination}\n";
            }
            Log.PrintMessage(programMsg);
        }

        return instructions;
    }

    private static byte EvaluateExpression(string expression, Dictionary<string, byte> constants,
                                         Dictionary<string, int> labels, Dictionary<string, int> subroutines,
                                         int lineNumber, bool allowOverflow = false)
    {
        try
        {
            if (ContainsOperators(expression))
            {
                return (byte)ParseExpression(expression, constants, labels, subroutines, allowOverflow);
            }

            if (expression.StartsWith("0b", StringComparison.OrdinalIgnoreCase))
            {
                string binaryPart = expression.Substring(2);
                if (binaryPart.Length != 8)
                {
                    throw new Exception($"Binary value must have exactly 8 bits, got {binaryPart.Length} at line {lineNumber}");
                }
                if (!binaryPart.All(c => c == '0' || c == '1'))
                {
                    throw new Exception($"Binary value can only contain 0 and 1 at line {lineNumber}");
                }
                return Convert.ToByte(binaryPart, 2);
            }

            if (expression.All(char.IsDigit))
            {
                int v = int.Parse(expression);
                
                if (allowOverflow)
                {
                    if (v < 0)
                    {
                        while (v < 0) v += 256;
                    }
                    return (byte)(v % 256);
                }
                else
                {
                    if (v < 0 || v > 255) throw new Exception($"Value {v} out of byte range at line {lineNumber}");
                    return (byte)v;
                }
            }

            if (constants.TryGetValue(expression, out byte constVal))
            {
                return constVal;
            }

            if (labels.TryGetValue(expression, out int labelAddr))
            {
                return (byte)labelAddr;
            }

            if (subroutines.TryGetValue(expression, out int subroutineAddr))
            {
                return (byte)subroutineAddr;
            }

            if (Keywords.list.TryGetValue(expression, out byte kwVal))
            {
                return kwVal;
            }

            throw new Exception($"Unknown token '{expression}' at line {lineNumber}");
        }
        catch (Exception ex) when (!(ex.Message.StartsWith("Unknown token") || ex.Message.StartsWith("Value")))
        {
            throw new Exception($"Error evaluating expression '{expression}' at line {lineNumber}: {ex.Message}");
        }
    }

    private static bool ContainsOperators(string expression)
    {
        return expression.Contains('|') || expression.Contains('+') || expression.Contains('-') ||
               expression.Contains('*') || expression.Contains('/') || expression.Contains('%') ||
               expression.Contains('^') || expression.Contains('&');
    }

    private static int ParseExpression(string expression, Dictionary<string, byte> constants,
                                     Dictionary<string, int> labels, Dictionary<string, int> subroutines,
                                     bool allowOverflow = false)
    {
        // Handle operators
        // 1. Multiplication (*), Division (/), Modulo (%)
        // 2. Addition (+), Subtraction (-)
        // 3. Bitwise operations (|, ^, &)

        if (expression.Contains('|'))
        {
            return ParseBinaryOperation(expression, '|', constants, labels, subroutines, (a, b) => a | b, allowOverflow);
        }
        if (expression.Contains('^'))
        {
            return ParseBinaryOperation(expression, '^', constants, labels, subroutines, (a, b) => a ^ b, allowOverflow);
        }
        if (expression.Contains('&'))
        {
            return ParseBinaryOperation(expression, '&', constants, labels, subroutines, (a, b) => a & b, allowOverflow);
        }

        if (expression.Contains('+'))
        {
            return ParseBinaryOperation(expression, '+', constants, labels, subroutines, (a, b) => a + b, allowOverflow);
        }
        if (expression.Contains('-'))
        {
            return ParseBinaryOperation(expression, '-', constants, labels, subroutines, (a, b) => a - b, allowOverflow);
        }

        if (expression.Contains('*'))
        {
            return ParseBinaryOperation(expression, '*', constants, labels, subroutines, (a, b) => a * b, allowOverflow);
        }
        if (expression.Contains('/'))
        {
            return ParseBinaryOperation(expression, '/', constants, labels, subroutines, (a, b) =>
            {
                if (b == 0) throw new Exception("Division by zero");
                return a / b;
            }, allowOverflow);
        }
        if (expression.Contains('%'))
        {
            return ParseBinaryOperation(expression, '%', constants, labels, subroutines, (a, b) =>
            {
                if (b == 0) throw new Exception("Modulo by zero");
                return a % b;
            }, allowOverflow);
        }

        throw new Exception($"Invalid expression '{expression}'");
    }

    private static int ParseBinaryOperation(string expression, char op, Dictionary<string, byte> constants,
                                          Dictionary<string, int> labels, Dictionary<string, int> subroutines,
                                          Func<int, int, int> operation, bool allowOverflow = false)
    {
        // Find the rightmost occurrence of the operator to handle left-to-right evaluation
        int opIndex = expression.LastIndexOf(op);
        if (opIndex == -1) return ParseExpression(expression, constants, labels, subroutines, allowOverflow);

        string left = expression.Substring(0, opIndex).Trim();
        string right = expression.Substring(opIndex + 1).Trim();

        int leftVal = EvaluateExpression(left, constants, labels, subroutines, 0, allowOverflow);
        int rightVal = EvaluateExpression(right, constants, labels, subroutines, 0, allowOverflow);

        int result = operation(leftVal, rightVal);

        if (allowOverflow)
        {
            if (result < 0)
            {
                while (result < 0) result += 256;
            }
            return result % 256;
        }
        else
        {
            if (result < 0 || result > 255)
            {
                throw new Exception($"Expression result {result} out of byte range (0-255)");
            }
            return result;
        }
    }
}