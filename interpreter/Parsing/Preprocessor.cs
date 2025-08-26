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

            var currentSettings = new ParserSettings
            {
                Overflow = settings.Overflow,
                CharOutput = settings.CharOutput,
                SignedMode = settings.SignedMode
            };

            // Preprocess
            for (int i = 0; i < lines.Length; i++)
            {
                string rawLine = lines[i];
                int commentIdx = rawLine.IndexOf("//");
                if (commentIdx >= 0) rawLine = rawLine[..commentIdx];

                if (string.IsNullOrWhiteSpace(rawLine)) continue;
                string processedLine = rawLine.Trim();

                string completedLine = InstructionCompleter.CompleteInstruction(processedLine, i + 1);
                if (completedLine != processedLine)
                {
                    Log.PrintMessage($"Line completed: '{processedLine}' -> '{completedLine}'");
                    processedLine = completedLine;
                }

                string[] parts = SplitterWithException(processedLine, splitChar: ' ', exceptionChar: '\"');

                if (PragmaProcessor.ProcessPragma(parts, i + 1, currentSettings))
                {
                    continue;
                }

                if (instructionIndex > int.MaxValue)
                {
                    throw new Exception($"Program exceeds maximum of {int.MaxValue} instructions");
                }

                if (parts[0].Equals("#define", StringComparison.OrdinalIgnoreCase))
                {
                    ProcessMacro(parts, i + 1, context);
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

                List<string> stringExpansions = StringPrintProcessor.ProcessStringPrint(parts, i + 1, currentSettings);
                if (stringExpansions.Count > 0)
                {
                    for (int j = 0; j < stringExpansions.Count; j++)
                    {
                        string expansion = stringExpansions[j];

                        if (PragmaProcessor.ProcessPragma(expansion.Split(' '), i + 1, currentSettings))
                            continue;
                        
                        var expandedSettings = new ParserSettings
                        {
                            Overflow = currentSettings.Overflow,
                            CharOutput = currentSettings.CharOutput,
                            SignedMode = currentSettings.SignedMode
                        };
                        context.InstructionSettings.Add(expandedSettings);

                        context.ProcesssedLines.Add(expansion);
                        context.OriginalLineNumbers.Add(i + 1);
                        instructionIndex++;
                    }
                    
                    continue;
                }

                var settingsSnapshot = new ParserSettings
                {
                    Overflow = currentSettings.Overflow,
                    CharOutput = currentSettings.CharOutput,
                    SignedMode = currentSettings.SignedMode
                };
                context.InstructionSettings.Add(settingsSnapshot);
                
                processedLine = string.Join(" ", parts).ToUpper();
                context.ProcesssedLines.Add(processedLine);
                
                context.OriginalLineNumbers.Add(i + 1);
                Log.PrintMessage($"Line {rawLine} preprocessed sucessfully to {processedLine}");
                instructionIndex++;
            }

            return context;
        }

        private static void ProcessMacro(string[] parts, int lineNumber, ParserContext context)
        {
            if (parts.Length != 3)
                throw new Exception($"Invalid macro definition at line {lineNumber}");

            string name = parts[1];
            string value = parts[2];

            if (int.TryParse(name, out _))
                throw new Exception($"Macro name must be a identifier at line {lineNumber}");

            if (Utils.Keywords.list.ContainsKey(name))
                throw new Exception($"Macro name is a reserved keyword at line {lineNumber}");

            context.Macros[name] = value;
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
            context.SubroutineAddresses.Add(address); // for interpreter access
            Log.PrintMessage($"[PARSER] Subroutine '{parts[1]}' registered at address {address}");
        }

        internal static string[] SplitterWithException(string str, char splitChar, char exceptionChar)
        {
            List<string> words = new List<string>();

            string word = "";
            bool exceptionIsOpen = false;
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] == exceptionChar) exceptionIsOpen = !exceptionIsOpen;
                if (str[i] == splitChar && !exceptionIsOpen)
                {
                    words.Add(word);
                    word = "";
                    continue;
                }
                word += str[i];
            }

            if (word.Length > 0) words.Add(word);

            return words.ToArray();
        }
    }
}