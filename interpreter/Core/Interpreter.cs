using interpreter.Utils;
using interpreter.Parsing;
using System.Text;

namespace interpreter.Core
{
    class Interpreter
    {
        private VirtualMachine vm;
        private List<ParserSettings> instructionSettings;
        public const byte REG_RAM_ADDRESS = Core.VirtualMachine.MAX_REGISTERS - 1; // Last REG controls the address of RAM

        private enum LastInstructionConditional
        {
            WasntConditional = 0,
            ConditionalButNotTrue = 1,
            ConditionalAndTrue = 2
        }
        private LastInstructionConditional lastInstruction = LastInstructionConditional.WasntConditional;

        public Interpreter(VirtualMachine vm, List<ParserSettings> settings)
        {
            this.vm = vm;
            this.instructionSettings = new List<ParserSettings>();
            this.instructionSettings = settings;
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
            const byte MASK_ARG1 = 1 << 7;
            const byte MASK_ARG2 = 1 << 6;
            const byte MASK_BOTH = MASK_ARG1 | MASK_ARG2;
            if ((opcode & MASK_BOTH) == MASK_BOTH) return ArgumentMode.BothArgs;
            if ((opcode & MASK_ARG1) == MASK_ARG1) return ArgumentMode.Arg1Only;
            if ((opcode & MASK_ARG2) == MASK_ARG2) return ArgumentMode.Arg2Only;
            return ArgumentMode.None;
        }
        
        private byte GetVariable(byte variableCode)
        {
            if (variableCode < VirtualMachine.MAX_REGISTERS) return vm.Registers[variableCode];
            if (variableCode == Keywords.list["INPUT"]) return GetUserInput();
            if (variableCode == Keywords.list["STACK"]) return vm.CallStack.Pop();
            if (variableCode == Keywords.list["RAM"]) return vm.RAM[vm.Registers[REG_RAM_ADDRESS]];
            if (variableCode == Keywords.list["INPUT_RAM"]) return vm.InputRAM[vm.Registers[REG_RAM_ADDRESS]];
            if (variableCode == Keywords.list["COUNTER"]) return (byte)vm.IP;

            throw new InvalidStorageAreaException(variableCode);
        }

        /// <summary>
        /// Reads user input from the console.
        /// </summary>
        /// <returns>
        /// input if input is a numeric value inside range 0-255. Or input.Length if input is a string.
        /// </returns>
        /// <remarks>
        /// <list type="bullet">
        ///   <item><description>Use #pragma string_input true/false to expect numeric values or strings as input.</description></item>
        ///   <item><description>If input is a string, the input RAM is cleared. Then, each character's ASCII code is stored sequentially in INPUT_RAM, ending with a null terminator (0)</description></item>
        /// </list>
        /// </remarks>
        private byte GetUserInput()
        {
            Console.InputEncoding = Encoding.UTF8;

            string? input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
                throw InvalidInputException.Generic();

            if (!instructionSettings[vm.IP].StringInput)
            {
                if (int.TryParse(input.Trim(), out int value) && value >= byte.MinValue && value <= byte.MaxValue)
                {
                    return (byte)value;
                }
                throw InvalidInputException.OutOfRangeChar(input);
            }

            Array.Clear(vm.InputRAM);

            // Its minus 1 because we need to leave space for the string terminator (0)
            if (input.Length >= VirtualMachine.MAX_RAM - 1)
                throw InvalidInputException.LengthExceeded(input, VirtualMachine.MAX_RAM - 1);

            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] > byte.MaxValue)
                    throw InvalidInputException.OutOfRangeChar(input);
                vm.InputRAM[i] = (byte)input[i];
            }
            return (byte)input.Length;
        }
    
        private void SetVariable(byte variableCode, byte value)
        {
            // Use when "variables" are mentioned in destination (last byte)
    
            if (variableCode < VirtualMachine.MAX_REGISTERS)
            {
                vm.Registers[variableCode] = value;
                return;
            }
            if (variableCode == Keywords.list["OUTPUT"])
            {
                vm.Output = value;
                return;
            }
            if (variableCode == Keywords.list["STACK"])
            {
                vm.CallStack.Push(value);
                return;
            }
            if (variableCode == Keywords.list["RAM"])
            {
                vm.RAM[vm.Registers[REG_RAM_ADDRESS]] = value;
                return;
            }
            if (variableCode == Keywords.list["COUNTER"])
            {
                vm.IP = value;
                return;
            }

            throw new InvalidStorageAreaException(variableCode);
        }
    
        private bool OpcodeIsConditional(byte opcode)
        {
            return opcode >= Opcodes.IF_EQL && opcode <= Opcodes.IF_GOE;
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
            try
            {
                while (vm.IP < program.Count)
                {
                    int oldIP = vm.IP;
                    Execute(program[vm.IP]);

                    if (vm.IP >= int.MaxValue)
                    {
                        /*
                         This is to prevent endless running a program with maxValue
                         instructions, without it, the pointer would overflow to 0
                         and restart endlessly
                        */
                        return;
                    }
                    if (vm.IP == oldIP) vm.IP++;
                }
            }
            catch (ProgramHaltException)
            {
                
            }
        }
    
        private void Execute(Instruction instr)
        {
            if (lastInstruction == LastInstructionConditional.ConditionalButNotTrue)
            {
                lastInstruction = LastInstructionConditional.WasntConditional;
                return;
            }

            

            // Handle system instructions before masking
            if (instr.Opcode >= Opcodes.SYSTEM_INSTRUCTION_START) // System instruction range
            {
                ExecuteSystemInstruction(instr);
                return;
            }
    
            Instruction workingInstr = new Instruction(instr.Opcode, instr.Arg1, instr.Arg2, instr.Destination);
    
            // Change the value of arguments in case they are actually "variables"
            ResolveArguments(workingInstr);
    
            byte baseOpcode = (byte)(workingInstr.Opcode & 0b00111111); // removes the 2 most significant bits
    
            // Execute OPCODE
            int _result = baseOpcode switch
            {
                Opcodes.ADD => workingInstr.Arg1 + workingInstr.Arg2,
                Opcodes.SUB => workingInstr.Arg1 - workingInstr.Arg2,
                Opcodes.AND => workingInstr.Arg1 & workingInstr.Arg2,
                Opcodes.OR => workingInstr.Arg1 | workingInstr.Arg2,
                Opcodes.XOR => workingInstr.Arg1 ^ workingInstr.Arg2,
                Opcodes.MUL => workingInstr.Arg1 * workingInstr.Arg2,
                Opcodes.DIV => workingInstr.Arg1 / workingInstr.Arg2,
                Opcodes.MOD => workingInstr.Arg1 % workingInstr.Arg2,
                Opcodes.SHL => workingInstr.Arg1 << workingInstr.Arg2,
                Opcodes.SHR => workingInstr.Arg1 >> workingInstr.Arg2,
                Opcodes.ROL => (workingInstr.Arg1 << workingInstr.Arg2) | (workingInstr.Arg1 >> (8 - workingInstr.Arg2)),
                Opcodes.ROR => (workingInstr.Arg1 >> workingInstr.Arg2) | (workingInstr.Arg1 << (8 - workingInstr.Arg2)),
                Opcodes.IF_EQL => workingInstr.Arg1 == workingInstr.Arg2 ? 1 : 0,
                Opcodes.IF_NEQ => workingInstr.Arg1 != workingInstr.Arg2 ? 1 : 0,
                Opcodes.IF_LES => workingInstr.Arg1 < workingInstr.Arg2 ? 1 : 0,
                Opcodes.IF_LOE => workingInstr.Arg1 <= workingInstr.Arg2 ? 1 : 0,
                Opcodes.IF_GRT => workingInstr.Arg1 > workingInstr.Arg2 ? 1 : 0,
                Opcodes.IF_GOE => workingInstr.Arg1 >= workingInstr.Arg2 ? 1 : 0,
                _ => throw new NotImplementedException($"Opcode {baseOpcode} not implemented")
            };

            bool allowOverflow = false;
            if (instructionSettings != null && vm.IP < instructionSettings.Count)
            {
                allowOverflow = instructionSettings[vm.IP].Overflow;
            }
            
            if (!allowOverflow && _result < byte.MinValue)
            {
                throw new ArithmeticUnderflowException(instr, _result);
            }
            else if (!allowOverflow && _result > byte.MaxValue)
            {
                throw new ArithmeticOverflowException(instr, _result);
            }

            byte result = (byte)_result;

            if (OpcodeIsConditional(baseOpcode))
            {
                if (result == 0)
                {
                    lastInstruction = LastInstructionConditional.ConditionalButNotTrue;
                    return; // Conditional was not met
                }

                lastInstruction = LastInstructionConditional.ConditionalAndTrue;
                return;
            }

            if (workingInstr.Destination == Keywords.list["COUNTER"] && IsSubroutineAddress(workingInstr.Arg1))
            {
                vm.CallStack.Push((byte)(vm.IP + 1));
            }

            SetVariable(workingInstr.Destination, result);

            if (workingInstr.Destination == Keywords.list["OUTPUT"])
            {
                bool charMode = false;
                bool signedMode = false;
                if (instructionSettings != null && vm.IP < instructionSettings.Count)
                {
                    charMode = instructionSettings[vm.IP].CharOutput;
                    signedMode = instructionSettings[vm.IP].SignedMode;
                }

                if (charMode)
                {
                    Console.Write((char)vm.Output);
                }
                else if (signedMode)
                {
                    Console.WriteLine((sbyte)vm.Output);
                }
                else
                {
                    Console.WriteLine(vm.Output);
                }
            }
        }
    
        private void ExecuteSystemInstruction(Instruction instr)
        {
            switch (instr.Opcode)
            {
                case Opcodes.HALT:
                    throw new ProgramHaltException();
                case Opcodes.WAIT:
                    Thread.Sleep(instr.Arg1);
                    break;
                case Opcodes.CLEAR:
                    Console.Clear();
                    break;
                default:
                    throw new NotImplementedException($"System instruction {instr.Opcode} not implemented");
            }
        }
    }
}