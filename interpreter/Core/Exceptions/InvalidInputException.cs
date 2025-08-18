namespace interpreter.Core
{
    internal class InvalidInputException : Exception
    {
        internal InvalidInputException() : base("Invalid input provided") { }

        internal InvalidInputException(string? input)
            : base($"Invalid input provided: \"{input}\"") { }
    }
}