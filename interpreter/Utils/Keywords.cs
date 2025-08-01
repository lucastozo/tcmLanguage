namespace interpreter.Utils
{
    class Keywords
    {
        public static readonly IReadOnlyDictionary<string, byte> list = new Dictionary<string, byte>
        {
            ["ADD"] = Opcodes.ADD,
            ["ADDRESS"] = 5,
            ["AND"] = Opcodes.AND,
            ["ASHR"] = Opcodes.ASHR,
            ["BACK"] = 8,
            ["CALL"] = 210,
            ["CALLER"] = 6,
            ["COPY_CONST"] = 192,
            ["COUNTER"] = 6,
            ["DIV"] = Opcodes.DIV,
            ["HALT"] = Opcodes.HALT,
            ["IF_EQL"] = Opcodes.IF_EQL,
            ["IF_GOE"] = Opcodes.IF_GOE,
            ["IF_GRT"] = Opcodes.IF_GRT,
            ["IF_LES"] = Opcodes.IF_LES,
            ["IF_LOE"] = Opcodes.IF_LOE,
            ["IF_NEQ"] = Opcodes.IF_NEQ,
            ["IMD_ARG1"] = 128,
            ["IMD_ARG2"] = 64,
            ["IMD_ARG3"] = 192,
            ["IN"] = 1,
            //["INPUT"] = 7, // Only makes sense in-game
            ["JMP"] = 208,
            ["MEMORY"] = 5,
            ["MOD"] = Opcodes.MOD,
            ["MOV"] = 64,
            ["MULTIPLY"] = Opcodes.MULTIPLY,
            ["NEXT"] = 64,
            ["NOT_A"] = Opcodes.NOT_A,
            ["NOW"] = 0,
            ["OR"] = Opcodes.OR,
            ["OUTPUT"] = 7,
            ["PREVIOUS"] = 65,
            ["RAM"] = 128,
            ["REG0"] = 0,
            ["REG1"] = 1,
            ["REG2"] = 2,
            ["REG3"] = 3,
            ["REG4"] = 4,
            ["REG5"] = 5,
            ["RETURN"] = 64,
            ["ROL"] = Opcodes.ROL,
            ["ROR"] = Opcodes.ROR,
            ["SHL"] = Opcodes.SHL,
            ["SHR"] = Opcodes.SHR,
            ["STACK"] = 8,
            ["SUB"] = Opcodes.SUB,
            ["SUBROUTINE"] = 255,
            ["TO"] = 0,
            ["XOR"] = Opcodes.XOR
        };
    }
}