using interpreter.Utils;

namespace interpreter.Core
{
    class VirtualMachine
    {
        public const int MAX_RAM = 256;
        public const int MAX_REGISTERS = 6; // REG0 - REG5 
    
        public float[] RAM { get; } = new float[MAX_RAM];
        public float[] Registers { get; } = new float[MAX_REGISTERS];
        public float[] UserInputRAM { get; } = new float[MAX_RAM];

        public int IP { get; set; } = 0;
        public Stack<float> CallStack { get; } = new Stack<float>();
        public float Output { get; set; } = 0;
    
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
                    ramState += $"{RAM[index]:F2} "; // Float format with 2 decimal places
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