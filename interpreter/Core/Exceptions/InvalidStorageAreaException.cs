namespace interpreter.Core
{
    internal class InvalidStorageAreaException : Exception
    {
        private InvalidStorageAreaException(string message) : base(message) { }

        internal static InvalidStorageAreaException Generic() =>
            new InvalidStorageAreaException("Attempted to access invalid storage area");
        internal static InvalidStorageAreaException ReadFailure(byte areaCode) =>
            new InvalidStorageAreaException($"Attempted to read invalid storage area: '{areaCode}'");

        internal static InvalidStorageAreaException WriteFailure(byte areaCode) =>
            new InvalidStorageAreaException($"Attempted to write in invalid storage area: '{areaCode}'");
    }
}