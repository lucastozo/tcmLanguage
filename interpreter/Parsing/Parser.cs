using interpreter.Core;
using interpreter.Utils;

namespace interpreter.Parsing
{
    class Parser
    {
        // Keep for backward compatibility with Interpreter
        internal static HashSet<int> SubroutineAddresses { get; private set; } = new HashSet<int>();
    
        public static List<Instruction> GetInstructions(string pathToFile)
        {
            Log.PrintMessage("[PARSER] initiating");
    
            var settings = new ParserSettings();
            string[] lines = File.ReadAllLines(pathToFile);
            
            // Preprocess the file
            ParserContext context = Preprocessor.ProcessFile(lines, settings);
            
            // Update static property for backward compatibility with Interpreter
            SubroutineAddresses = context.SubroutineAddresses;
    
            // Build instructions
            List<Instruction> instructions = BuildInstructions(context, settings);
    
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
    
        private static List<Instruction> BuildInstructions(ParserContext context, ParserSettings settings)
        {
            List<Instruction> instructions = new List<Instruction>();
    
            for (int i = 0; i < context.RawInstructionLines.Count; i++)
            {
                int originalLineNum = context.OriginalLineNumbers[i];
                string[] parts = context.RawInstructionLines[i].Split(' ');
                List<byte> bParts = new();
                
                var instructionSettings = context.InstructionSettings[i];
    
                foreach (var part in parts)
                {
                    byte value = ExpressionEvaluator.EvaluateExpression(part, context, originalLineNum, instructionSettings.Overflow);
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
    
            return instructions;
        }
    }
}