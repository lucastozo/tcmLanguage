using interpreter.Utils;

namespace interpreter.Core
{
    class VirtualMachine
    {
        public const int MAX_RAM = 256;
        public const int MAX_REGISTERS = 6; // REG0 - REG5 
    
        public byte[] RAM { get; } = new byte[MAX_RAM];
        public byte[] Registers { get; } = new byte[MAX_REGISTERS];
    
        public int IP { get; set; } = 0;
        public Stack<byte> CallStack { get; } = new Stack<byte>();
        public byte Output { get; set; } = 0;
    
        public void PrintState()
        {
            Log.PrintMessage($"Printing current state of virtual machine...");
    
            Log.PrintMessage($"Instruction Pointer: {IP}");

            Log.PrintMessage("RAM:");
            string ramState = "";
            for (int row = 0; row < MAX_RAM / 32; row++)
            {
                for (int col = 0; col < MAX_RAM / 8; col++)
                {
                    int index = row * MAX_RAM / 8 + col;
                    ramState += $"{RAM[index]:X2} "; // Hex format, 2-digit
                }
                ramState += "\n";
            }
            Log.PrintMessage(ramState);

            Log.PrintMessage("STACK:");
            for (int i = 0; i < CallStack.Count; i++)
            {
                Log.PrintMessage($"Item {i} of the stack: {CallStack.ElementAt(i)}\n");
            }
            if (CallStack.Count == 0) Log.PrintMessage("No elements in stack");

            Log.PrintMessage("REGISTERS:");
            for (int i = 0; i < MAX_REGISTERS; i++)
            {
                Log.PrintMessage($"REG{i} = {Registers[i]}");
            }
            
            Log.PrintMessage();
        }
    }
}