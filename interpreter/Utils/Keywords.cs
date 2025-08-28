namespace interpreter.Utils
{
    class Keywords
    {
        public static readonly IReadOnlyDictionary<string, byte> list = new Dictionary<string, byte>
        {
            ["ADD"] = Opcodes.ADD,
            ["ADDRESS"] = Core.Interpreter.REG_RAM_ADDRESS,
            ["AND"] = Opcodes.AND,
            ["ASHR"] = Opcodes.ASHR,
            ["CLEAR"] = Opcodes.CLEAR,
            ["COUNTER"] = 6,
            ["DIV"] = Opcodes.DIV,
            ["HALT"] = Opcodes.HALT,
            ["INPUT"] = 9,
            ["INPUT_RAM"] = 11,
            ["MOD"] = Opcodes.MOD,
            ["MOV"] = 64,
            ["MULTIPLY"] = Opcodes.MULTIPLY,
            ["NOT_A"] = Opcodes.NOT_A,
            ["OR"] = Opcodes.OR,
            ["OUTPUT"] = 7,
            ["RAM"] = 10,
            ["REG0"] = 0,
            ["REG1"] = 1,
            ["REG2"] = 2,
            ["REG3"] = 3,
            ["REG4"] = 4,
            ["REG5"] = 5,
            ["ROL"] = Opcodes.ROL,
            ["ROR"] = Opcodes.ROR,
            ["SHL"] = Opcodes.SHL,
            ["SHR"] = Opcodes.SHR,
            ["STACK"] = 8,
            ["SUB"] = Opcodes.SUB,
            ["TO"] = 0,
            ["WAIT"] = Opcodes.WAIT,
            ["XOR"] = Opcodes.XOR
        };
    }
}