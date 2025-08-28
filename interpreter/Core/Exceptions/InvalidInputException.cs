namespace interpreter.Core
{
    internal class InvalidInputException : Exception
    {
        private InvalidInputException(string message) : base(message) { }
        
        internal static InvalidInputException Generic() =>
            new InvalidInputException("Invalid input provided");

        internal static InvalidInputException OutOfRangeChar(string? input) =>
            new InvalidInputException($"Input contains a character out of range ({byte.MinValue}-{byte.MaxValue}) ");
        
        internal static InvalidInputException LengthExceeded(string? input, int maxLength) =>
            new InvalidInputException($"Input exceeded maximum length of {maxLength} bytes");
    }
}