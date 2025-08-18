namespace interpreter.Core
{
    internal class ArithmeticUnderflowException : Exception
    {
        internal ArithmeticUnderflowException() : base("Illegal arithmetic result") { }

        internal ArithmeticUnderflowException(Instruction instr, int result)
            : base($"Instruction {instr.Opcode} {instr.Arg1} {instr.Arg2} {instr.Destination} caused arithmetic underflow, result: {result}") { }
    }
}