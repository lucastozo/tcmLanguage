namespace interpreter.Parsing
{
    public class ParserSettings
    {
        public bool Overflow { get; set; } = false; // When true, allow things like 255+1
        public bool CharOutput { get; set; } = false; // When true, values in output will correspond their extended ASCII characters
        public bool SignedMode { get; set; } = false; // When true, uses most significant bit to display as negative/positive in output
    }
}