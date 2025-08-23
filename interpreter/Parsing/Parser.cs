using interpreter.Core;
using interpreter.Utils;

namespace interpreter.Parsing
{
    class Parser
    {
        internal static HashSet<int> SubroutineAddresses { get; private set; } = new HashSet<int>();
    
        public static (List<Instruction>, List<ParserSettings>) GetInstructionsWithSettings(string pathToFile)
        {
            Log.PrintMessage("[PARSER] initiating");
    
            var settings = new ParserSettings();
            string[] lines = File.ReadAllLines(pathToFile);

            if (Log.ShowLogs)
            {
                Log.PrintMessage("Original program:\n");
                foreach (string line in lines)
                {
                    Log.PrintMessage(line);
                }
            }

            ParserContext context = Preprocessor.ProcessFile(lines, settings);

            SubroutineAddresses = context.SubroutineAddresses;

            List<Instruction> instructions = BuildInstructions(context);
    
            if (Log.ShowLogs)
            {
                Log.PrintMessage("Final program:");
                foreach (Instruction instr in instructions)
                {
                    Log.PrintMessage($"{instr.Opcode} {instr.Arg1} {instr.Arg2} {instr.Destination}");
                }
            }
    
            return (instructions, context.InstructionSettings);
        }

        private static List<Instruction> BuildInstructions(ParserContext context)
        {
            List<Instruction> instructions = new List<Instruction>();

            for (int i = 0; i < context.RawInstructionLines.Count; i++)
            {
                int originalLineNum = context.OriginalLineNumbers[i];
                string[] parts = context.RawInstructionLines[i].Split(' ');
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

            // Replace constants in instruction
            for (int i = 0; i < parts.Length; i++)
            {
                if (context.Constants.TryGetValue(parts[i], out string? value))
                {
                    parts[i] = value;
                }
            }

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