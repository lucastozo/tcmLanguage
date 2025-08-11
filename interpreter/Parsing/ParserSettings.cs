namespace interpreter.Parsing
{
    public class ParserSettings
    {
        public bool Overflow { get; set; } = false; // When true, allow things like 255+1
        public bool CharOutput { get; set; } = false; // When true, values in output will correspond their extended ASCII characters
    }
}