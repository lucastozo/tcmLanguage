namespace interpreter.Core
{
    internal class ArithmeticOverflowException : Exception
    {
        internal ArithmeticOverflowException() : base("Illegal arithmetic result") { }

        internal ArithmeticOverflowException(Instruction instr, int result)
            : base($"Instruction {instr.Opcode} {instr.Arg1} {instr.Arg2} {instr.Destination} caused arithmetic overflow, result: {result}") { }
    }
}