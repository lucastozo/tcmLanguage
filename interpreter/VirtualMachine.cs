class VirtualMachine
{
    public const int MAX_RAM = 256;
    public const int MAX_REGISTERS = 6; // REG0 - REG5 

    public byte[] RAM { get; } = new byte[MAX_RAM];
    public byte[] Registers { get; } = new byte[MAX_REGISTERS];

    public byte IP { get; set; } = 0;
    public Stack<byte> CallStack { get; } = new Stack<byte>();
    public byte Output { get; set; } = 0;

    public void PrintState()
    {
        Console.WriteLine($"Instruction Pointer: {IP}");

        Console.WriteLine($"\nRAM:");
        for (int row = 0; row < MAX_RAM / 32; row++)
        {
            for (int col = 0; col < MAX_RAM / 8; col++)
            {
                int index = row * MAX_RAM / 8 + col;
                Console.Write($"{RAM[index]:X2} "); // Hex format, 2-digit
            }
            Console.WriteLine();
        }

        Console.WriteLine($"\nSTACK:");
        for (int i = 0; i < CallStack.Count; i++)
        {
            Console.WriteLine($"Item {i} of the stack: {CallStack.ElementAt(i)}");
        }
        if (CallStack.Count == 0) Console.WriteLine("No elements in stack");

        Console.WriteLine("\nRegisters:");
        for (int i = 0; i < MAX_REGISTERS; i++)
        {
            Console.WriteLine($"REG{i} = {Registers[i]}");
        }
        
        Console.WriteLine();
    }
}