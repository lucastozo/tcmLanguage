using interpreter.Utils;

namespace interpreter.Core
{
    class VirtualMachine
    {
        public const int MAX_RAM = 256;
        public const int MAX_REGISTERS = 6; // REG0 - REG5 
    
        public byte[] RAM { get; } = new byte[MAX_RAM];
        public byte[] Registers { get; } = new byte[MAX_REGISTERS];
        public byte[] UserInputRAM { get; } = new byte[MAX_RAM];

        public int IP { get; set; } = 0;
        public Stack<byte> CallStack { get; } = new Stack<byte>();
        public byte Output { get; set; } = 0;
    }
}