using interpreter.Core;

namespace interpreter.Parsing
{
    class Parser
    {
        internal static HashSet<int> SubroutineAddresses { get; private set; } = new HashSet<int>();
    
        public static (List<Instruction>, List<ParserSettings>) GetInstructionsWithSettings(string pathToFile)
        {
            var settings = new ParserSettings();
            string[] lines = File.ReadAllLines(pathToFile);

            ParserContext context = Preprocessor.ProcessFile(lines, settings);

            SubroutineAddresses = context.SubroutineAddresses;

            List<Instruction> instructions = BuildInstructions(context);
    
            return (instructions, context.InstructionSettings);
        }

        private static List<Instruction> BuildInstructions(ParserContext context)
        {
            List<Instruction> instructions = new List<Instruction>();

            for (int i = 0; i < context.ProcesssedLines.Count; i++)
            {
                int originalLineNum = context.OriginalLineNumbers[i];
                string[] parts = context.ProcesssedLines[i].Split(' ');
                var instructionSettings = context.InstructionSettings[i];
                
                if (parts.Count() != 4)
                {
                    throw new Exception($"Invalid instruction at line {originalLineNum}");
                }

                List<byte> bParts = ProcessInstructionParts(parts, context, originalLineNum, instructionSettings);

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