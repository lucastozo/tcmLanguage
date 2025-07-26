class Parser
{
    public static List<Instruction> GetInstructions(string pathToFile)
    {
        Log.PrintMessage("[PARSER] initiating");

        Dictionary<string, byte> constants = new Dictionary<string, byte>();
        Dictionary<string, int> labels = new Dictionary<string, int>();
        List<string> rawInstructionLines = new List<string>();

        string[] lines = File.ReadAllLines(pathToFile);
        int instructionIndex = 0;

        // Preprocess
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            int commentIdx = line.IndexOf('#');
            if (commentIdx >= 0) line = line[..commentIdx];
            line = string.Join(" ", line.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] parts = line.Split(' ');

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

            rawInstructionLines.Add(line);
            Log.PrintMessage($"Line {line} preprocessed sucessfully");
            instructionIndex++;
        }

        // Build instructions
        List<Instruction> instructions = new List<Instruction>();

        for (int i = 0; i < rawInstructionLines.Count; i++)
        {
            string[] parts = rawInstructionLines[i].Split(' ');
            List<byte> bParts = new();

            foreach (var part in parts)
            {
                byte value = EvaluateExpression(part, constants, labels, i + 1);
                bParts.Add(value);
            }

            if (bParts.Count != 4)
            {
                throw new Exception($"Invalid instruction at line {i + 1}: must have exactly 4 operands");
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
                                         Dictionary<string, int> labels, int lineNumber)
    {
        try
        {
            // Check if binary value
            if (ContainsOperators(expression))
            {
                return (byte)ParseExpression(expression, constants, labels);
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
                if (v < 0 || v > 255) throw new Exception($"Value {v} out of byte range at line {lineNumber}");
                return (byte)v;
            }

            if (constants.TryGetValue(expression, out byte constVal))
            {
                return constVal;
            }

            if (labels.TryGetValue(expression, out int labelAddr))
            {
                return (byte)labelAddr;
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
                                     Dictionary<string, int> labels)
    {
        // Handle operators
        // 1. Multiplication (*), Division (/), Modulo (%)
        // 2. Addition (+), Subtraction (-)
        // 3. Bitwise operations (|, ^, &)
        System.Console.WriteLine(expression);

        if (expression.Contains('|'))
        {
            return ParseBinaryOperation(expression, '|', constants, labels, (a, b) => a | b);
        }
        if (expression.Contains('^'))
        {
            return ParseBinaryOperation(expression, '^', constants, labels, (a, b) => a ^ b);
        }
        if (expression.Contains('&'))
        {
            return ParseBinaryOperation(expression, '&', constants, labels, (a, b) => a & b);
        }

        if (expression.Contains('+'))
        {
            return ParseBinaryOperation(expression, '+', constants, labels, (a, b) => a + b);
        }
        if (expression.Contains('-'))
        {
            return ParseBinaryOperation(expression, '-', constants, labels, (a, b) => a - b);
        }

        if (expression.Contains('*'))
        {
            return ParseBinaryOperation(expression, '*', constants, labels, (a, b) => a * b);
        }
        if (expression.Contains('/'))
        {
            return ParseBinaryOperation(expression, '/', constants, labels, (a, b) => {
                if (b == 0) throw new Exception("Division by zero");
                return a / b;
            });
        }
        if (expression.Contains('%'))
        {
            return ParseBinaryOperation(expression, '%', constants, labels, (a, b) => {
                if (b == 0) throw new Exception("Modulo by zero");
                return a % b;
            });
        }

        throw new Exception($"Invalid expression '{expression}'");
    }

    private static int ParseBinaryOperation(string expression, char op, Dictionary<string, byte> constants, 
                                          Dictionary<string, int> labels, Func<int, int, int> operation)
    {
        // Find the rightmost occurrence of the operator to handle left-to-right evaluation
        int opIndex = expression.LastIndexOf(op);
        if (opIndex == -1) return ParseExpression(expression, constants, labels);

        string left = expression.Substring(0, opIndex).Trim();
        string right = expression.Substring(opIndex + 1).Trim();

        int leftVal = EvaluateExpression(left, constants, labels, 0);
        int rightVal = EvaluateExpression(right, constants, labels, 0);

        int result = operation(leftVal, rightVal);
        
        // Ensure result fits in byte range
        if (result < 0 || result > 255)
        {
            throw new Exception($"Expression result {result} out of byte range (0-255)");
        }

        return result;
    }
}