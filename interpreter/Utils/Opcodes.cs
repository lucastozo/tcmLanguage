namespace interpreter.Utils
{
    public static class Opcodes
    {
        public const byte SYSTEM_INSTRUCTION_START = 250;
    
        public const byte ADD = 0;
        public const byte SUB = 1;
        public const byte AND = 2;
        public const byte OR = 3;
        public const byte NOT_A = 4;
        public const byte XOR = 5;
        public const byte MULTIPLY = 6;
        public const byte DIV = 7;
        public const byte MOD = 8;
        public const byte SHL = 9;
        public const byte SHR = 10;
        public const byte ASHR = 11;
        public const byte ROL = 12;
        public const byte ROR = 13;
        public const byte IF_EQL = 16;
        public const byte IF_NEQ = 17;
        public const byte IF_LES = 18;
        public const byte IF_LOE = 19;
        public const byte IF_GRT = 20;
        public const byte IF_GOE = 21;
    
        // System instructions, 250 - 255
        public const byte HALT = 250;
    }
}