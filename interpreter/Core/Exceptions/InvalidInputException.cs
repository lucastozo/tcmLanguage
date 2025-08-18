namespace interpreter.Core
{
    public class InvalidInputException : Exception
    {
        public InvalidInputException() : base("Invalid input provided") { }

        public InvalidInputException(string? input)
            : base($"Invalid input provided: \"{input}\"") { }
    }
}