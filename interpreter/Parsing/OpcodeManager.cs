using interpreter.Utils;
using interpreter.Core;

namespace interpreter.Parsing
{
    public static class OpcodeManager
    {
        private static readonly HashSet<int> VariableKeywords = InitializeVariableKeywords();
        private static HashSet<int> InitializeVariableKeywords()
        {
            var keywords = new HashSet<int>();
            
            for (int i = 0; i < VirtualMachine.MAX_REGISTERS; i++)
            {
                keywords.Add(Keywords.list[$"REG{i}"]);
            }

            string[] otherVars = {
                "INPUT",
                "OUTPUT",
                "STACK",
                "RAM",
                "INPUT_RAM",
                "COUNTER"
            };
            foreach (var varName in otherVars)
            {
                keywords.Add(Keywords.list[varName]);
            }
            
            return keywords;
        }

        private const byte ARG1_LITERAL_MASK = 1 << 7;
        private const byte ARG2_LITERAL_MASK = 1 << 6;

        public static int BuildOpcode(int baseOpcode, string arg1, string arg2, ParserContext context, int lineNumber)
        {
            int finalOpcode = baseOpcode;

            if (IsLiteralValue(arg1, context)) finalOpcode |= ARG1_LITERAL_MASK;

            if (IsLiteralValue(arg2, context)) finalOpcode |= ARG2_LITERAL_MASK;

            if (finalOpcode != baseOpcode)
            {
                Log.PrintMessage($"[OPCODE MANAGER] Opcode adjusted from {baseOpcode} to {finalOpcode} at line {lineNumber}");
            }

            return finalOpcode;
        }

        private static bool IsLiteralValue(string value, ParserContext context)
        {
            if (float.TryParse(value, System.Globalization.NumberStyles.Float, 
                   System.Globalization.CultureInfo.InvariantCulture, out _) && !string.IsNullOrEmpty(value))
                return true;

            if (context.Labels.ContainsKey(value))
                return true;

            if (context.Subroutines.ContainsKey(value))
                return true;

            if (ContainsOperators(value))
                return true;

            if (IsRegisterOrVariable(value))
                return false;

            return true;
        }

        private static bool ContainsOperators(string value)
        {
            return value.Contains('+') || value.Contains('-') || value.Contains('*') ||
                   value.Contains('/') || value.Contains('%') || value.Contains('|') ||
                   value.Contains('^') || value.Contains('&');
        }

        private static bool IsRegisterOrVariable(string value)
        {
            if (Keywords.list.TryGetValue(value, out byte keywordValue))
            {
                return VariableKeywords.Contains(keywordValue);
            }
            
            return false;
        }
    }
}