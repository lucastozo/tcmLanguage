namespace interpreter.Core
{
    public class ProgramHaltException : Exception
    {
        public ProgramHaltException() : base("Program execution halted") { }
    }
}