namespace interpreter.Parsing
{
    public class ParserSettings
    {
        public bool Overflow { get; set; } = false; // When true, allow things like 255+1
    }
}