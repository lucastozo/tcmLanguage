class Interpreter
{
    private VirtualMachine vm;
    private const byte RAM_ADDRESS_CONTROLLER = 5; // REG5 controls the address of RAM

    public Interpreter(VirtualMachine vm)
    {
        this.vm = vm;
    }

    private enum ArgumentMode
    {
        None = 0, // No arguments are literals
        Arg1Only = 1, // ARG1 is literal (ARG2 is variable)
        Arg2Only = 2, // ARG2 is literal (ARG1 is variable)
        BothArgs = 3 // Both ARG1 and ARG2 are literals
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

    private string? SetVariable(byte variableCode, byte value)
    {
        // Use when "variables" are mentioned in destination (last byte)

        if (variableCode < VirtualMachine.MAX_REGISTERS)
        {
            vm.Registers[variableCode] = value;
            return $"REG{variableCode}";
        }
        if (variableCode == Keywords.list["OUTPUT"])
        {
            vm.Output = value;
            return "OUTPUT";
        }
        if (variableCode == Keywords.list["STACK"])
        {
            vm.CallStack.Push(value);
            return "STACK";
        }
        if (variableCode == Keywords.list["RAM"])
        {
            vm.RAM[vm.Registers[RAM_ADDRESS_CONTROLLER]] = value;
            return "RAM";
        }
        if (variableCode == Keywords.list["COUNTER"])
        {
            vm.IP = value;
            return "COUNTER";
        }
        return null;
    }

    private bool OpcodeIsConditional(byte opcode)
    {
        return opcode == Opcodes.IF_EQL || opcode == Opcodes.IF_GOE || opcode == Opcodes.IF_GRT || opcode == Opcodes.IF_LES || opcode == Opcodes.IF_LOE || opcode == Opcodes.IF_NEQ;
    }

    private bool IsSubroutineAddress(byte address)
    {
        return Parser.SubroutineAddresses.Contains(address);
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
        Log.PrintMessage("[INTERPRETER] initiating simulated program");
        while (vm.IP < program.Count)
        {
            vm.PrintState();
            bool ipWasChanged = Execute(program[vm.IP]);
            Log.PrintMessage("-------------------------------");
            if (!ipWasChanged) vm.IP++; // If Instruction Pointer was changed because of a goto, dont tamper it
        }
        vm.PrintState();
        Log.PrintMessage("-- END OF SIMULATED PROGRAM --");
    }

    private bool Execute(Instruction instr)
    {
        Log.PrintMessage($"Executing instruction: {instr.Opcode} {instr.Arg1} {instr.Arg2} {instr.Destination}");
        
        Instruction workingInstr = new Instruction(instr.Opcode, instr.Arg1, instr.Arg2, instr.Destination);
        
        // Change the value of arguments in case they are actually "variables"
        ResolveArguments(workingInstr);
        Log.PrintMessage($"Arguments of instruction changed to: {workingInstr.Arg1} {workingInstr.Arg2}");

        byte baseOpcode = (byte)(workingInstr.Opcode & 0b00111111); // removes the 2 most significant bits
        Log.PrintMessage($"Opcode decoded to: {baseOpcode}");

        // Execute OPCODE
        byte result = baseOpcode switch
        {
            Opcodes.ADD => (byte)(workingInstr.Arg1 + workingInstr.Arg2),
            Opcodes.SUB => (byte)(workingInstr.Arg1 - workingInstr.Arg2),
            Opcodes.AND => (byte)(workingInstr.Arg1 & workingInstr.Arg2),
            Opcodes.OR => (byte)(workingInstr.Arg1 | workingInstr.Arg2),
            Opcodes.NOT_A => (byte)~workingInstr.Arg1,
            Opcodes.XOR => (byte)(workingInstr.Arg1 ^ workingInstr.Arg2),
            Opcodes.MULTIPLY => (byte)(workingInstr.Arg1 * workingInstr.Arg2),
            Opcodes.DIV => (byte)(workingInstr.Arg1 / workingInstr.Arg2),
            Opcodes.MOD => (byte)(workingInstr.Arg1 % workingInstr.Arg2),
            Opcodes.SHL => (byte)(workingInstr.Arg1 << workingInstr.Arg2),
            Opcodes.SHR => (byte)(workingInstr.Arg1 >> workingInstr.Arg2),
            Opcodes.ASHR => (byte)(((sbyte)workingInstr.Arg1) >> workingInstr.Arg2),
            Opcodes.ROL => (byte)((workingInstr.Arg1 << workingInstr.Arg2) | (workingInstr.Arg1 >> (8 - workingInstr.Arg2))),
            Opcodes.ROR => (byte)((workingInstr.Arg1 >> workingInstr.Arg2) | (workingInstr.Arg1 << (8 - workingInstr.Arg2))),
            Opcodes.IF_EQL => (byte)(workingInstr.Arg1 == workingInstr.Arg2 ? 1 : 0),
            Opcodes.IF_NEQ => (byte)(workingInstr.Arg1 != workingInstr.Arg2 ? 1 : 0),
            Opcodes.IF_LES => (byte)(workingInstr.Arg1 < workingInstr.Arg2 ? 1 : 0),
            Opcodes.IF_LOE => (byte)(workingInstr.Arg1 <= workingInstr.Arg2 ? 1 : 0),
            Opcodes.IF_GRT => (byte)(workingInstr.Arg1 > workingInstr.Arg2 ? 1 : 0),
            Opcodes.IF_GOE => (byte)(workingInstr.Arg1 >= workingInstr.Arg2 ? 1 : 0),
            _ => throw new NotImplementedException($"Opcode {baseOpcode} not implemented")
        };
        
        Log.PrintMessage($"Result of ALU: {result}");

        if (OpcodeIsConditional(baseOpcode))
        {
            Log.PrintMessage("Conditional detected");
            if (result == 0) return false; // Conditional was not met

            // Check if explicit "CALL NOW SUBROUTINE" instruction
            bool isExplicitSubroutineCall = workingInstr.Opcode == Keywords.list["CALL"] &&
                                            workingInstr.Arg1 == Keywords.list["NOW"] &&
                                            workingInstr.Arg2 == Keywords.list["SUBROUTINE"];

            // Check if destination is a subroutine address (for conditional calls)
            bool isConditionalSubroutineCall = IsSubroutineAddress(workingInstr.Destination);

            if (isExplicitSubroutineCall || isConditionalSubroutineCall)
            {
                if (isExplicitSubroutineCall)
                {
                    Log.PrintMessage("Explicit subroutine call detected");
                }
                else
                {
                    Log.PrintMessage("Conditional subroutine call detected");
                }
                
                vm.CallStack.Push(vm.IP);
                Log.PrintMessage($"Return address {vm.IP} pushed to stack");
            }

            vm.IP = (byte)(workingInstr.Destination / 4);
            Log.PrintMessage($"Value {vm.IP} moved to Instruction Pointer");
            return true;
        }

        string? variableChanged = SetVariable(workingInstr.Destination, result);
        Log.PrintMessage($"Variable {variableChanged} received value {result}");

        if (workingInstr.Destination == Keywords.list["OUTPUT"])
        {
            Console.WriteLine(vm.Output);
        }

        Log.PrintMessage("Instruction executed");

        return false;
    }
}