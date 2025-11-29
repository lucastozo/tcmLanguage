namespace interpreter.Parsing
{
    public static class StringPrintProcessor
    {
        public static List<string> ProcessStringPrint(string[] parts, int lineNumber, ParserSettings settings)
        {
            bool hasString = false;
            foreach (string part in parts)
            {
                if (part.Length < 2) continue;
                if (part[0] == '\"' && part[^1] == '\"') hasString = true;
            }
            if (!hasString) return new List<string>();

            if (parts[0] != "ADD" || parts[2] != "0" || parts[3] != "OUTPUT")
            {
                throw new Exception($"Invalid string usage at line {lineNumber}. Expected \"PRINT <string>\"");
            }

            string str = parts[1];

            List<string> expansions = new List<string>();
            expansions.Add("#pragma char true");

            str = str[1..^1]; // remove quotes
            for (int i = 0; i < str.Length; i++)
            {
                char c = str[i];
                if (c == '\\' && i < str.Length - 1) // escape sequence
                {
                    i++;
                    char nextChar = str[i];

                    switch (nextChar)
                    {
                        case 'n': // Newline \n
                            expansions.Add($"ADD {(byte)'\n'} 0 OUTPUT");
                            break;
                        case 'b': // Backspace \b
                            expansions.Add($"ADD {(byte)'\b'} 0 OUTPUT");
                            break;
                        case 't': // hTab \t
                            expansions.Add($"ADD {(byte)'\t'} 0 OUTPUT");
                            break;
                        case 'v': // vTab \v
                            expansions.Add($"ADD {(byte)'\v'} 0 OUTPUT");
                            break;
                        case 'r': // Carriage return \r
                            expansions.Add($"ADD {(byte)'\r'} 0 OUTPUT");
                            break;
                        case '\\': // Backslash \\
                            expansions.Add($"ADD {(byte)'\\'} 0 OUTPUT");
                            break;
                        case '\"': // Quote \"
                            expansions.Add($"ADD {(byte)'\"'} 0 OUTPUT");
                            break;
                        case '/': // Forward slash \/
                            expansions.Add($"ADD {(byte)'/'} 0 OUTPUT");
                            break;
                        default:
                            throw new Exception($"Invalid escape sequence \"{c}{nextChar}\" inside string at line {lineNumber}");
                    }
                }
                else
                {
                    if (c == '"')
                    {
                        throw new Exception($"Unescaped quote inside string at line {lineNumber}. Use \\\" to escape quotes.");
                    }
                    expansions.Add($"ADD {(byte)c} 0 OUTPUT"); // normal char
                }
            }
            
            if (!settings.CharOutput)
                expansions.Add("#pragma char false");

            return expansions;
        }
    }
}