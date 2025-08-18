namespace interpreter.Core
{
    internal class InvalidStorageAreaException : Exception
    {
        internal InvalidStorageAreaException() : base("Attempted to access invalid storage area") { }
        
        internal InvalidStorageAreaException(byte areaCode) 
            : base($"Attempted to access invalid storage area: '{areaCode}'") { }
    }
}