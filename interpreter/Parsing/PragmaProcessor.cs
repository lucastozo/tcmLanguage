using interpreter.Utils;

namespace interpreter.Parsing
{
    public static class PragmaProcessor
    {
        public static bool ProcessPragma(string[] parts, int lineNumber, ParserSettings settings)
        {
            if (!parts[0].Equals("@pragma", StringComparison.OrdinalIgnoreCase))
                return false;
    
            if (parts.Length != 2) 
                throw new Exception($"Invalid pragma directive at line {lineNumber}. Expected: @pragma <option>");
            
            string pragmaOption = parts[1].ToLowerInvariant();
            switch (pragmaOption)
            {
                case "allow_overflow":
                    settings.AllowOverflow = true;
                    Log.PrintMessage($"[PARSER] Pragma: overflow behavior enabled");
                    break;
                case "disallow_overflow":
                    settings.AllowOverflow = false;
                    Log.PrintMessage($"[PARSER] Pragma: overflow behavior disabled");
                    break;
                default:
                    throw new Exception($"Unknown pragma option '{parts[1]}' at line {lineNumber}.");
            }
            
            return true;
        }
    }
}