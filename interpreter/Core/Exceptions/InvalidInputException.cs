namespace interpreter.Core
{
    public class InvalidInputException : Exception
    {
        public InvalidInputException() : base("Invalid input provided") { }
    }
}