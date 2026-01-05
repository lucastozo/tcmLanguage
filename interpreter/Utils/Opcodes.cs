namespace interpreter.Utils
{
    public static class Opcodes
    {
        public const byte SYSTEM_INSTRUCTION_START = 250;

        public const byte ADD = 0;
        public const byte SUB = 1;
        public const byte AND = 2;
        public const byte OR = 3;
        public const byte XOR = 4;
        public const byte MUL = 5;
        public const byte DIV = 6;
        public const byte MOD = 7;
        public const byte SHL = 8;
        public const byte SHR = 9;
        public const byte ROL = 10;
        public const byte ROR = 11;
        public const byte POW = 12;
        public const byte NRT = 13; // Nth Root
        public const byte RND = 14; // Random between A and B inclusive
        public const byte IF_EQL = 16;
        public const byte IF_NEQ = 17;
        public const byte IF_LES = 18;
        public const byte IF_LOE = 19;
        public const byte IF_GRT = 20;
        public const byte IF_GOE = 21;

        // System instructions, 250 - 255
        public const byte HALT = 250;
        public const byte WAIT = 251;
        public const byte CLEAR = 252;
        public const byte RETURN = 253;
    }
}
