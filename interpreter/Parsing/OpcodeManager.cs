using interpreter.Utils;
using interpreter.Core;

namespace interpreter.Parsing
{
    public static class OpcodeManager
    {
        private const byte ARG1_LITERAL_MASK = 1 << 7;
        private const byte ARG2_LITERAL_MASK = 1 << 6;

        public static byte BuildOpcode(byte baseOpcode, string arg1, string arg2, ParserContext context, int lineNumber)
        {
            byte finalOpcode = baseOpcode;

            if (IsLiteralValue(arg1, context))
            {
                finalOpcode |= ARG1_LITERAL_MASK;
                Log.PrintMessage($"[OPCODE MANAGER] Setting ARG1 as literal for '{arg1}' at line {lineNumber}");
            }
            else
            {
                Log.PrintMessage($"[OPCODE MANAGER] Treating ARG1 as register/variable for '{arg1}' at line {lineNumber}");
            }

            if (IsLiteralValue(arg2, context))
            {
                finalOpcode |= ARG2_LITERAL_MASK;
                Log.PrintMessage($"[OPCODE MANAGER] Setting ARG2 as literal for '{arg2}' at line {lineNumber}");
            }
            else
            {
                Log.PrintMessage($"[OPCODE MANAGER] Treating ARG2 as register/variable for '{arg2}' at line {lineNumber}");
            }

            if (finalOpcode != baseOpcode)
            {
                Log.PrintMessage($"[OPCODE MANAGER] Opcode adjusted from {baseOpcode} to {finalOpcode} at line {lineNumber}");
            }

            return finalOpcode;
        }

        private static bool IsLiteralValue(string value, ParserContext context)
        {
            if (IsNumericLiteral(value))
                return true;

            if (context.Constants.ContainsKey(value))
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

        private static bool IsNumericLiteral(string value)
        {
            return value.All(char.IsDigit) && !string.IsNullOrEmpty(value);
        }

        private static bool ContainsOperators(string value)
        {
            return value.Contains('+') || value.Contains('-') || value.Contains('*') ||
                   value.Contains('/') || value.Contains('%') || value.Contains('|') ||
                   value.Contains('^') || value.Contains('&');
        }

        private static bool IsRegisterOrVariable(string value)
        {
            if (IsRegisterFormat(value))
                return true;

            var variableKeywords = new[]
            {
                Keywords.list["INPUT"],
                Keywords.list["OUTPUT"],
                Keywords.list["STACK"],
                Keywords.list["RAM"],
                Keywords.list["COUNTER"]
            };

            return variableKeywords.Contains(Keywords.list[value]);
        }

        private static bool IsRegisterFormat(string value)
        {
            if (value.Length == 4 && value.StartsWith("REG", StringComparison.OrdinalIgnoreCase))
            {
                char lastChar = value[3];
                return lastChar >= '0' && lastChar <= '0' + (VirtualMachine.MAX_REGISTERS - 1);
            }
            return false;
        }
    }
}