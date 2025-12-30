using interpreter.Utils;

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
                SignedMode = settings.SignedMode,
                StringInput = settings.StringInput
            };

            // Preprocess
            for (int i = 0; i < lines.Length; i++)
            {
                string rawLine = lines[i];
                int commentIndex = rawLine.IndexOf("//");
                if (commentIndex >= 0)
                    rawLine = rawLine.Substring(0, commentIndex);

                if (string.IsNullOrWhiteSpace(rawLine)) continue;
                string processedLine = rawLine.Trim();

                // Split by ':' to support multiple instructions per line
                string[] subInstructions = SplitterWithException(processedLine, splitChar: ':', exceptionChar: '\"');

                for (int subIdx = 0; subIdx < subInstructions.Length; subIdx++)
                {
                    string subInstruction = subInstructions[subIdx].Trim();
                    if (string.IsNullOrWhiteSpace(subInstruction)) continue;

                    // Replace constants in instruction
                    string[] parts = SplitterWithException(subInstruction, splitChar: ' ', exceptionChar: '\"');
                    for (int j = 0; j < parts.Length; j++)
                    {
                        if (context.Macros.TryGetValue(parts[j], out string? value))
                        {
                            parts[j] = value;
                        }
                    }
                    subInstruction = string.Join(' ', parts);

                    subInstruction = InstructionCompleter.CompleteInstruction(subInstruction, i + 1);
                    
                    parts = SplitterWithException(subInstruction, splitChar: ' ', exceptionChar: '\"');

                    // Convert keywords to Upper
                    for (int w = 0; w < parts.Length; w++)
                    {
                        if (Keywords.list.ContainsKey(parts[w].ToUpper()))
                        {
                            parts[w] = parts[w].ToUpper();
                        }
                    }

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
                                SignedMode = currentSettings.SignedMode,
                                StringInput = currentSettings.StringInput
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
                        SignedMode = currentSettings.SignedMode,
                        StringInput = currentSettings.StringInput
                    };
                    context.InstructionSettings.Add(settingsSnapshot);
                    
                    subInstruction = string.Join(" ", parts);
                    context.ProcesssedLines.Add(subInstruction);
                    
                    context.OriginalLineNumbers.Add(i + 1);
                    instructionIndex++;
                }
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
                throw new Exception($"Macro identifier can not be a reserved keyword at line {lineNumber}");

            context.Macros[name] = value;
        }

        private static void ProcessLabel(string[] parts, int lineNumber, ParserContext context, int instructionIndex)
        {
            if (parts.Length != 2)
                throw new Exception($"Invalid label declaration at line {lineNumber}");

            string label_name = parts[1];

            if (int.TryParse(label_name, out _))
                throw new Exception($"Label name must be a identifier at line {lineNumber}");

            if (Utils.Keywords.list.ContainsKey(label_name))
                throw new Exception($"Label identifier can not be a reserved keyword at line {lineNumber}");
            
            if (context.Labels.ContainsKey(label_name))
                throw new Exception($"A label with this identifier already exists at line {lineNumber}");

            if (context.Subroutines.ContainsKey(label_name))
                throw new Exception($"A subroutine with this identifier already exists at line {lineNumber}");

            context.Labels.Add(label_name, instructionIndex);
        }

        private static void ProcessSubroutine(string[] parts, int lineNumber, ParserContext context, int instructionIndex)
        {
            if (parts.Length != 2)
                throw new Exception($"Invalid subroutine declaration at line {lineNumber}");

            string subroutine_name = parts[1];

            if (int.TryParse(subroutine_name, out _))
                throw new Exception($"Subroutine name must be a identifier at line {lineNumber}");

            if (Utils.Keywords.list.ContainsKey(subroutine_name))
                throw new Exception($"Subroutine identifier can not be a reserved keyword at line {lineNumber}");

            if (context.Labels.ContainsKey(subroutine_name))
                throw new Exception($"A label with this identifier already exists at line {lineNumber}");

            if (context.Subroutines.ContainsKey(subroutine_name))
                throw new Exception($"A subroutine with this identifier already exists at line {lineNumber}");

            int address = instructionIndex;
            context.Subroutines.Add(parts[1], address);
            context.SubroutineAddresses.Add(address); // for interpreter access
        }

        internal static string[] SplitterWithException(string str, char splitChar, char exceptionChar)
        {
            List<string> words = new List<string>();

            string word = "";
            bool exceptionIsOpen = false;
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] == exceptionChar)
                {
                    int backslashCount = 0;
                    int j = i - 1;
                    while (j >= 0 && str[j] == '\\')
                    {
                        backslashCount++;
                        j--;
                    }

                    // If the quote is not escaped (even number of preceding backslashes), toggle
                    if (backslashCount % 2 == 0)
                        exceptionIsOpen = !exceptionIsOpen;
                }

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