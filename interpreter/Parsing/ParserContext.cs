namespace interpreter.Parsing
{
    public class ParserContext
    {
        public Dictionary<string, string> Macros { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, int> Labels { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> Subroutines { get; set; } = new Dictionary<string, int>();
        public List<string> RawInstructionLines { get; set; } = new List<string>();
        public List<int> OriginalLineNumbers { get; set; } = new List<int>();
        public HashSet<int> SubroutineAddresses { get; set; } = new HashSet<int>();
        public List<ParserSettings> InstructionSettings { get; set; } = new List<ParserSettings>();
    }
}