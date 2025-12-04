namespace interpreter.Core
{
    internal class EmptyStackException : Exception
    {
        private EmptyStackException(string message) : base(message) { }
        
        internal static EmptyStackException UserStackEmpty() =>
            new EmptyStackException("Stack empty");
        
        internal static EmptyStackException CallStackEmpty() =>
            new EmptyStackException("Call stack empty. This was possibly caused by using RETURN without calling a subroutine");
    }
}