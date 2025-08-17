using interpreter.Core;
using interpreter.Utils;

namespace interpreter.Parsing
{
    class Parser
    {
        // Keep for backward compatibility with Interpreter
        internal static HashSet<int> SubroutineAddresses { get; private set; } = new HashSet<int>();
    
        public static (List<Instruction>, List<ParserSettings>) GetInstructionsWithSettings(string pathToFile)
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
    
            return (instructions, context.InstructionSettings);
        }
        
        public static List<Instruction> GetInstructions(string pathToFile)
        {
            return GetInstructionsWithSettings(pathToFile).Item1;
        }

        private static List<Instruction> BuildInstructions(ParserContext context, ParserSettings settings)
        {
            List<Instruction> instructions = new List<Instruction>();

            for (int i = 0; i < context.RawInstructionLines.Count; i++)
            {
                int originalLineNum = context.OriginalLineNumbers[i];
                string[] parts = context.RawInstructionLines[i].Split(' ');
                var instructionSettings = context.InstructionSettings[i];

                List<byte> bParts = ProcessInstructionParts(parts, context, originalLineNum, instructionSettings);

                if (bParts.Count != 4)
                {
                    throw new Exception($"Invalid instruction at line {originalLineNum}");
                }

                instructions.Add(new Instruction(bParts[0], bParts[1], bParts[2], bParts[3]));
            }

            return instructions;
        }

        private static List<byte> ProcessInstructionParts(string[] parts, ParserContext context, int lineNumber, ParserSettings instructionSettings)
        {
            List<byte> bParts = new();

            byte baseOpcode = ExpressionEvaluator.EvaluateExpression(parts[0], context, lineNumber, instructionSettings.Overflow);
            byte finalOpcode = OpcodeManager.BuildOpcode(baseOpcode, parts[1], parts[2], context, lineNumber);
            bParts.Add(finalOpcode);
            
            for (int i = 1; i < parts.Length; i++)
            {
                byte value = ExpressionEvaluator.EvaluateExpression(parts[i], context, lineNumber, instructionSettings.Overflow);
                bParts.Add(value);
            }

            return bParts;
        }
    }
}