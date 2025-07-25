class Interpreter
{
    private VirtualMachine vm;
    private const byte RAM_ADDRESS_CONTROLLER = 4; // REG4 controls the address of RAM

    private enum ArgumentMode
    {
        None = 0, // No arguments are literals
        Arg1Only = 1, // ARG1 is literal (ARG2 is variable)
        Arg2Only = 2, // ARG2 is literal (ARG1 is variable)
        BothArgs = 3 // Both ARG1 and ARG2 are literals
    }

    public Interpreter(VirtualMachine vm)
    {
        this.vm = vm;
    }

    private ArgumentMode GetArgumentMode(byte opcode)
    {
        const byte MASK_ARG1 = 128; // 10000000
        const byte MASK_ARG2 = 64;  // 01000000
        const byte MASK_BOTH = 192; // 11000000
        if ((opcode & MASK_BOTH) == MASK_BOTH) return ArgumentMode.BothArgs;
        if ((opcode & MASK_ARG1) == MASK_ARG1) return ArgumentMode.Arg1Only;
        if ((opcode & MASK_ARG2) == MASK_ARG2) return ArgumentMode.Arg2Only;
        return ArgumentMode.None;
    }

    private byte GetVariable(byte variableCode)
    {
        // Use when "variables" are mentioned in Arguments
        /*
            The possible "variables" are:
            REG0, REG1, REG2, REG3, REG4, REG5
            INPUT *wont be implemented here
            OUTPUT
            STACK
            RAM
        */

        if (variableCode <= VirtualMachine.MAX_REGISTERS) return vm.Registers[variableCode];
        if (variableCode == Keywords.list["OUTPUT"]) return vm.Output;
        if (variableCode == Keywords.list["STACK"]) return vm.CallStack.Pop();
        if (variableCode == Keywords.list["RAM"]) return vm.RAM[vm.Registers[RAM_ADDRESS_CONTROLLER]];

        return 0;
    }

    private void SetVariable(byte variableCode, byte value)
    {
        // Use when "variables" are mentioned in destination (last byte)

        if (variableCode <= VirtualMachine.MAX_REGISTERS)
        {
            vm.Registers[variableCode] = value;
        }
        else if (variableCode == Keywords.list["OUTPUT"])
        {
            vm.Output = value;
        }
        else if (variableCode == Keywords.list["STACK"])
        {
            vm.CallStack.Push(value);
        }
        else if (variableCode == Keywords.list["RAM"])
        {
            vm.RAM[vm.Registers[RAM_ADDRESS_CONTROLLER]] = value;
        }
    }

    private bool OpcodeIsConditional(byte opcode)
    {
        return opcode == Opcodes.IF_EQL || opcode == Opcodes.IF_GOE || opcode == Opcodes.IF_GRT || opcode == Opcodes.IF_LES || opcode == Opcodes.IF_LOE || opcode == Opcodes.IF_NEQ;
    }

    private void ResolveArguments(Instruction instr)
    {
        ArgumentMode mode = GetArgumentMode(instr.Opcode);

        switch (mode)
        {
            case ArgumentMode.None: // Both are variables
                instr.Arg1 = GetVariable(instr.Arg1);
                instr.Arg2 = GetVariable(instr.Arg2);
                break;
            case ArgumentMode.Arg1Only: // ARG1 is literal, ARG2 is variable
                instr.Arg2 = GetVariable(instr.Arg2);
                break;
            case ArgumentMode.Arg2Only: // ARG2 is literal, ARG1 is variable
                instr.Arg1 = GetVariable(instr.Arg1);
                break;
            case ArgumentMode.BothArgs:
                // Both arguments are literals, nothing to change
                break;
        }
    }

    public void Run(List<Instruction> program)
    {
        Console.WriteLine("PROGRAM:");
        foreach (Instruction instruction in program)
        {
            Console.WriteLine($"{instruction.Opcode} {instruction.Arg1} {instruction.Arg2} {instruction.Destination}");
        }
        Console.WriteLine();

        while (vm.IP < program.Count)
        {
            vm.PrintState();
            Console.WriteLine($"Executing instruction: {program[vm.IP].Opcode} {program[vm.IP].Arg1} {program[vm.IP].Arg2} {program[vm.IP].Destination}");
            bool ipWasChanged = Execute(program[vm.IP]);
            if (ipWasChanged) continue; // If Instruction Pointer was changed because of a goto
            vm.IP++;
        }
        vm.PrintState();
    }

    private bool Execute(Instruction instr)
    {
        // Change the value of arguments in case they are actually "variables"
        ResolveArguments(instr);

        // Down opcode to non immediate modes
        instr.Opcode = (byte)(instr.Opcode & 0b00111111); // removes the 2 most significant bytes

        // Get result from OPCODE
        byte result = instr.Opcode switch
        {
            Opcodes.ADD => (byte)(instr.Arg1 + instr.Arg2),
            Opcodes.SUB => (byte)(instr.Arg1 - instr.Arg2),
            Opcodes.AND => (byte)(instr.Arg1 & instr.Arg2),
            Opcodes.OR => (byte)(instr.Arg1 | instr.Arg2),
            Opcodes.NOT_A => (byte)~instr.Arg1,
            Opcodes.XOR => (byte)(instr.Arg1 ^ instr.Arg2),
            Opcodes.MULTIPLY => (byte)(instr.Arg1 * instr.Arg2),
            Opcodes.DIV => (byte)(instr.Arg1 / instr.Arg2),
            Opcodes.MOD => (byte)(instr.Arg1 % instr.Arg2),
            Opcodes.SHL => (byte)(instr.Arg1 << instr.Arg2),
            Opcodes.SHR => (byte)(instr.Arg1 >> instr.Arg2),
            Opcodes.ASHR => (byte)(((sbyte)instr.Arg1) >> instr.Arg2),
            Opcodes.ROL => (byte)((instr.Arg1 << instr.Arg2) | (instr.Arg1 >> (8 - instr.Arg2))),
            Opcodes.ROR => (byte)((instr.Arg1 >> instr.Arg2) | (instr.Arg1 << (8 - instr.Arg2))),
            Opcodes.IF_EQL => (byte)(instr.Arg1 == instr.Arg2 ? 1 : 0),
            Opcodes.IF_NEQ => (byte)(instr.Arg1 != instr.Arg2 ? 1 : 0),
            Opcodes.IF_LES => (byte)(instr.Arg1 < instr.Arg2 ? 1 : 0),
            Opcodes.IF_LOE => (byte)(instr.Arg1 <= instr.Arg2 ? 1 : 0),
            Opcodes.IF_GRT => (byte)(instr.Arg1 > instr.Arg2 ? 1 : 0),
            Opcodes.IF_GOE => (byte)(instr.Arg1 >= instr.Arg2 ? 1 : 0),
            _ => throw new NotImplementedException()
        };

        if (OpcodeIsConditional(instr.Opcode))
        {
            if (result == 0) return false; // Conditional was not meet

            // CALL NOW SUBROUTINE label ?
            bool isSubroutineCall = instr.Opcode == Opcodes.IF_LES && instr.Arg1 == Keywords.list["NOW"] && instr.Arg2 == Keywords.list["SUBROUTINE"];
            if (isSubroutineCall)
            {
                vm.CallStack.Push((byte)(instr.Destination / 4));
            }

            vm.IP = (byte)(instr.Destination / 4);
            return true;
        }

        SetVariable(instr.Destination, result);

        if (instr.Destination == Keywords.list["OUTPUT"])
        {
            Console.WriteLine(vm.Output);
        }

        return false;
    }
}