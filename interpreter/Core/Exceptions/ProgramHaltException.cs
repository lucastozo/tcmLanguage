namespace interpreter.Core
{
    internal class ProgramHaltException : Exception
    {
        internal ProgramHaltException() : base("Program execution halted") { }
    }
}