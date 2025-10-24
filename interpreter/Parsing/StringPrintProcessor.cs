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
            if (!hasString) return [];

            if (parts[0] != "COPY" || parts[2] != "0" || parts[3] != "OUTPUT")
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
                            expansions.Add($"COPY {(byte)'\n'} 0 OUTPUT");
                            break;
                        case 'b': // Backspace \b
                            expansions.Add($"COPY {(byte)'\b'} 0 OUTPUT");
                            break;
                        case 't': // hTab \t
                            expansions.Add($"COPY {(byte)'\t'} 0 OUTPUT");
                            break;
                        case 'v': // vTab \v
                            expansions.Add($"COPY {(byte)'\v'} 0 OUTPUT");
                            break;
                        case 'r': // Carriage return \r
                            expansions.Add($"COPY {(byte)'\r'} 0 OUTPUT");
                            break;
                        case '\\': // Backslash \\
                            expansions.Add($"COPY {(byte)'\\'} 0 OUTPUT");
                            break;
                        case '\"': // Quote \"
                            expansions.Add($"COPY {(byte)'\"'} 0 OUTPUT");
                            break;
                        case '/': // Forward slash \/
                            expansions.Add($"COPY {(byte)'/'} 0 OUTPUT");
                            break;
                        default:
                            throw new Exception($"Invalid escape sequence \"{c}{nextChar}\" inside string at line {lineNumber}");
                    }
                }
                else
                {
                    expansions.Add($"COPY {(byte)c} 0 OUTPUT"); // normal char
                }
            }
            
            if (!settings.CharOutput)
                expansions.Add("#pragma char false");

            return expansions;
        }
    }
}