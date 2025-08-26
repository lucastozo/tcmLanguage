using interpreter.Utils;

namespace interpreter.Parsing
{
    public static class PragmaProcessor
    {
        public static bool ProcessPragma(string[] parts, int lineNumber, ParserSettings settings)
        {
            if (!parts[0].Equals("#pragma", StringComparison.OrdinalIgnoreCase))
                return false;
    
            if (parts.Length != 3) 
                throw new Exception($"Invalid pragma directive at line {lineNumber}. Expected: @pragma <option> true/false");

            if (!bool.TryParse(parts[2].ToLowerInvariant(), out bool enable))
                throw new Exception($"Invalid pragma directive at line {lineNumber}. Expected: @pragma <option> true/false");

            string pragmaOption = parts[1].ToLowerInvariant();
            switch (pragmaOption)
                {
                    case "overflow":
                        settings.Overflow = enable;
                        if (enable) Log.PrintMessage($"[PARSER] Pragma: overflow behavior enabled");
                        else Log.PrintMessage($"[PARSER] Pragma: overflow behavior disabled");
                        break;
                    case "char":
                        settings.CharOutput = enable;
                        if (enable) Log.PrintMessage($"[PARSER] Pragma: character output mode enabled");
                        else Log.PrintMessage($"[PARSER] Pragma: character output mode disabled");
                        break;
                    case "signed":
                        settings.SignedMode = enable;
                        if (enable) Log.PrintMessage($"[PARSER] Pragma: signed output mode enabled");
                        else Log.PrintMessage($"[PARSER] Pragma: signed output mode disabled");
                        break;
                    default:
                        throw new Exception($"Unknown pragma option '{parts[1]}' at line {lineNumber}.");
                }
            
            return true;
        }
    }
}