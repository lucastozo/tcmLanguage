namespace interpreter.Core
{
    public class InvalidStorageAreaException : Exception
    {
        public InvalidStorageAreaException() : base("Attempted to access invalid storage area") { }
        
        public InvalidStorageAreaException(byte areaCode) 
            : base($"Attempted to access invalid storage area: '{areaCode}'") { }
    }
}