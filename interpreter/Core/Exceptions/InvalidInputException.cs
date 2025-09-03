namespace interpreter.Core
{
    internal class InvalidInputException : Exception
    {
        private InvalidInputException(string message) : base(message) { }
        
        internal static InvalidInputException Generic() =>
            new InvalidInputException("Invalid input provided");
            
        internal static InvalidInputException LengthExceeded(string? input, int maxLength) =>
            new InvalidInputException($"Input exceeded maximum length of {maxLength}");
    }
}