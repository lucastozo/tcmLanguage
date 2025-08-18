using interpreter.Utils;
using System.Text.RegularExpressions;

namespace interpreter.Parsing
{
    public static class Preprocessor
    {
        public static ParserContext ProcessFile(string[] lines, ParserSettings settings)
        {
            var context = new ParserContext();
            int instructionIndex = 0;

            context.SubroutineAddresses.Clear(); // Clear previous subroutine addresses

            var currentSettings = new ParserSettings
            {
                Overflow = settings.Overflow,
                CharOutput = settings.CharOutput
            };

            // Preprocess
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                int commentIdx = line.IndexOf('#');
                if (commentIdx >= 0) line = line[..commentIdx];

                var matches = Regex.Matches(line.Trim(), @"('.')|(\S+)");
                line = string.Join(" ", matches.Select(m => ProcessCharacterLiteral(m.Value)));

                if (string.IsNullOrWhiteSpace(line)) continue;
                line = line.ToUpper();

                string completedLine = InstructionCompleter.CompleteInstruction(line, i + 1);
                if (completedLine != line)
                {
                    Log.PrintMessage($"Line completed: '{line}' -> '{completedLine}'");
                    line = completedLine;
                }

                string[] parts = line.Split(' ');

                // Handle pragma directives
                if (PragmaProcessor.ProcessPragma(parts, i + 1, currentSettings))
                {
                    continue;
                }

                if (instructionIndex > byte.MaxValue)
                {
                    throw new Exception($"Program exceeds maximum of {byte.MaxValue} instructions at line {i + 1}");
                }

                if (parts[0].Equals("const", StringComparison.OrdinalIgnoreCase))
                {
                    ProcessConstant(parts, i + 1, context);
                    continue;
                }

                if (parts[0].Equals("label", StringComparison.OrdinalIgnoreCase))
                {
                    ProcessLabel(parts, i + 1, context, instructionIndex);
                    continue;
                }

                if (parts[0].Equals("subroutine", StringComparison.OrdinalIgnoreCase))
                {
                    ProcessSubroutine(parts, i + 1, context, instructionIndex);
                    continue;
                }

                var settingsSnapshot = new ParserSettings
                {
                    Overflow = currentSettings.Overflow,
                    CharOutput = currentSettings.CharOutput,
                    SignedMode = currentSettings.SignedMode
                };
                context.InstructionSettings.Add(settingsSnapshot);

                context.RawInstructionLines.Add(line);
                context.OriginalLineNumbers.Add(i + 1);
                Log.PrintMessage($"Line {line} preprocessed sucessfully");
                instructionIndex++;
            }

            return context;
        }

        private static void ProcessConstant(string[] parts, int lineNumber, ParserContext context)
        {
            if (parts.Length != 3)
                throw new Exception($"Invalid const declaration at line {lineNumber}");

            if (!byte.TryParse(parts[2], out byte constVal))
            {
                throw new Exception($"Invalid const value at line {lineNumber}");
            }

            context.Constants[parts[1]] = constVal;
        }

        private static void ProcessLabel(string[] parts, int lineNumber, ParserContext context, int instructionIndex)
        {
            if (parts.Length != 2)
                throw new Exception($"Invalid label at line {lineNumber}");

            context.Labels.Add(parts[1], instructionIndex);
        }

        private static void ProcessSubroutine(string[] parts, int lineNumber, ParserContext context, int instructionIndex)
        {
            if (parts.Length != 2)
                throw new Exception($"Invalid subroutine declaration at line {lineNumber}");

            int address = instructionIndex;
            context.Subroutines.Add(parts[1], address);
            context.SubroutineAddresses.Add(address); // Store for interpreter access
            Log.PrintMessage($"[PARSER] Subroutine '{parts[1]}' registered at address {address}");
        }

        private static string ProcessCharacterLiteral(string token)
        {
            if (token.Length == 3 && token[0] == '\'' && token[2] == '\'')
            {
                char c = token[1];
                return ((int)c).ToString(); // ascii
            }
            return token;
        }
    }
}