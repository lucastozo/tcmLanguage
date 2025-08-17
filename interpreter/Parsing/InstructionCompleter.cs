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
            
            ["PREVIOUS"] = new InstructionTemplate(
                pattern: ["PREVIOUS", "{VARIABLE}"],
                expansion: ["PREVIOUS", "{VARIABLE}", "IN", "{VARIABLE}"]
            ),

            ["NEXT"] = new InstructionTemplate(
                pattern: ["NEXT", "{VARIABLE}"],
                expansion: ["NEXT", "{VARIABLE}", "IN", "{VARIABLE}"]
            ),

            ["RETURN"] = new InstructionTemplate(
                pattern: ["RETURN"],
                expansion: ["RETURN", "BACK", "TO", "CALLER"]
            ),

            ["CALL"] = new InstructionTemplate(
                pattern: ["CALL", "{SUBROUTINE}"],
                expansion: ["CALL", "NOW", "SUBROUTINE", "{SUBROUTINE}"]
            ),

            ["PRINT"] = new InstructionTemplate(
                pattern: ["PRINT", "{VALUE}"],
                expansion: ["MOV", "{VALUE}", "TO", "OUTPUT"]
            ),

            ["GOTO"] = new InstructionTemplate(
                pattern: ["GOTO", "{LABEL}"],
                expansion: ["JMP", "NOW", "TO", "{LABEL}"]
            )
        };

        public static string CompleteInstruction(string line, int lineNumber)
        {
            if (string.IsNullOrWhiteSpace(line))
                return line;

            string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
                return line;

            foreach (var kvp in Templates)
            {
                string templateName = kvp.Key;
                InstructionTemplate template = kvp.Value;

                if (TryMatchPattern(parts, template.Pattern, out Dictionary<string, string> placeholders))
                {
                    Log.PrintMessage($"[INSTRUCTION COMPLETER] Expanding '{templateName}' at line {lineNumber}");
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