using interpreter.Utils;

namespace interpreter.Parsing
{
    public static class InstructionCompleter
    {
        private static readonly Dictionary<string, InstructionTemplate> Templates = new Dictionary<string, InstructionTemplate>
        {
            // -- SYSTEM INSTRUCTIONS --
            ["HALT"] = new InstructionTemplate(
                pattern: ["HALT"],
                expansion: ["HALT", "0", "0", "0"]
            ),

            ["WAIT"] = new InstructionTemplate(
                pattern: ["WAIT", "{MS}"],
                expansion: ["WAIT", "{MS}", "0", "0"]
            ),

            ["CLEAR"] = new InstructionTemplate(
                pattern: ["CLEAR"],
                expansion: ["CLEAR", "0", "0", "0"]
            ),

            ["INC"] = new InstructionTemplate(
                pattern: ["INC", "{SOURCE}"],
                expansion: ["ADD", "{SOURCE}", "1", "{SOURCE}"]
            ),

            ["DEC"] = new InstructionTemplate(
                pattern: ["DEC", "{SOURCE}"],
                expansion: ["SUB", "{SOURCE}", "1", "{SOURCE}"]
            ),

            ["COPY"] = new InstructionTemplate(
                pattern: ["COPY", "{SOURCE}", "{DESTINATION}"],
                expansion: ["ADD", "{SOURCE}", "0", "{DESTINATION}"]
            ),

            ["RETURN"] = new InstructionTemplate(
                pattern: ["RETURN"],
                expansion: ["RETURN", "0", "0", "0"]
            ),

            ["PRINT"] = new InstructionTemplate(
                pattern: ["PRINT", "{VALUE}"],
                expansion: ["ADD", "{VALUE}", "0", "OUTPUT"]
            ),

            ["GOTO"] = new InstructionTemplate(
                pattern: ["GOTO", "{LABEL}"],
                expansion: [$"ADD", "{LABEL}", "0", "COUNTER"]
            ),

            ["IF_EQL"] = new InstructionTemplate(
                pattern: ["IF", "{A}", "=", "{B}", "THEN"],
                expansion: [$"{Opcodes.IF_EQL}", "{A}", "{B}", "0"]
            ),

            ["IF_GOE"] = new InstructionTemplate(
                pattern: ["IF", "{A}", ">=", "{B}", "THEN"],
                expansion: [$"{Opcodes.IF_GOE}", "{A}", "{B}", "0"]
            ),

            ["IF_GRT"] = new InstructionTemplate(
                pattern: ["IF", "{A}", ">", "{B}", "THEN"],
                expansion: [$"{Opcodes.IF_GRT}", "{A}", "{B}", "0"]
            ),

            ["IF_LES"] = new InstructionTemplate(
                pattern: ["IF", "{A}", "<", "{B}", "THEN"],
                expansion: [$"{Opcodes.IF_LES}", "{A}", "{B}", "0"]
            ),

            ["IF_LOE"] = new InstructionTemplate(
                pattern: ["IF", "{A}", "<=", "{B}", "THEN"],
                expansion: [$"{Opcodes.IF_LOE}", "{A}", "{B}", "0"]
            ),

            ["IF_NEQ"] = new InstructionTemplate(
                pattern: ["IF", "{A}", "!=", "{B}", "THEN"],
                expansion: [$"{Opcodes.IF_NEQ}", "{A}", "{B}", "0"]
            )
        };

        public static string CompleteInstruction(string line, int lineNumber)
        {
            if (string.IsNullOrWhiteSpace(line))
                return line;

            string[] parts = Preprocessor.SplitterWithException(line, splitChar: ' ', exceptionChar: '"');
            if (parts.Length == 0)
                return line;

            foreach (var kvp in Templates)
            {
                InstructionTemplate template = kvp.Value;

                if (TryMatchPattern(parts, template.Pattern, out Dictionary<string, string> placeholders))
                {
                    return ExpandTemplate(template.Expansion, placeholders);
                }
            }

            return line;
        }

        private static bool TryMatchPattern(string[] parts, string[] pattern, out Dictionary<string, string> placeholders)
        {
            placeholders = new Dictionary<string, string>();

            if (parts.Length != pattern.Length)
                return false;

            for (int i = 0; i < parts.Length; i++)
            {
                string part = parts[i];
                string patternElement = pattern[i];

                if (IsPlaceholder(patternElement))
                {
                    string placeholderName = ExtractPlaceholderName(patternElement);
                    placeholders[placeholderName] = part;
                }
                else
                {
                    if (!string.Equals(part, patternElement, StringComparison.OrdinalIgnoreCase))
                        return false;
                }
            }

            return true;
        }

        private static string ExpandTemplate(string[] expansion, Dictionary<string, string> placeholders)
        {
            string[] expandedParts = new string[expansion.Length];

            for (int i = 0; i < expansion.Length; i++)
            {
                string element = expansion[i];
                
                if (IsPlaceholder(element))
                {
                    string placeholderName = ExtractPlaceholderName(element);
                    if (placeholders.TryGetValue(placeholderName, out string? value))
                    {
                        expandedParts[i] = value;
                    }
                    else
                    {
                        throw new Exception($"Placeholder '{placeholderName}' not found during template expansion");
                    }
                }
                else
                {
                    expandedParts[i] = element;
                }
            }

            return string.Join(" ", expandedParts);
        }

        private static bool IsPlaceholder(string str)
        {
            return str.StartsWith("{") && str.EndsWith("}") && str.Length > 2;
        }

        private static string ExtractPlaceholderName(string placeholder)
        {
            return placeholder.Substring(1, placeholder.Length - 2);
        }
    }

    class InstructionTemplate
    {
        public string[] Pattern { get; }

        public string[] Expansion { get; }

        public InstructionTemplate(string[] pattern, string[] expansion)
        {
            Pattern = pattern ?? throw new ArgumentNullException(nameof(pattern));
            Expansion = expansion ?? throw new ArgumentNullException(nameof(expansion));
        }
    }
}