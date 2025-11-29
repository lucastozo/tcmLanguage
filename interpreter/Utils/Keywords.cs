namespace interpreter.Utils
{
    class Keywords
    {
        public static readonly IReadOnlyDictionary<string, byte> list;

        private const byte REG_MAX = Core.VirtualMachine.MAX_REGISTERS;

        static Keywords()
        {
            var dict = new Dictionary<string, byte>
            {
                // ["REG0"] ... ["REGMAX"]
                ["ADDRESS"] = Core.Interpreter.REG_RAM_ADDRESS,

                ["COUNTER"] = REG_MAX + 1,
                ["OUTPUT"] = REG_MAX + 2,
                ["STACK"] = REG_MAX + 3,
                ["INPUT"] = REG_MAX + 4,
                ["RAM"] = REG_MAX + 5,
                ["INPUT_RAM"] = REG_MAX + 6,

                ["COPY"] = Opcodes.ADD | 0b01000000,

                ["ADD"] = Opcodes.ADD,
                ["SUB"] = Opcodes.SUB,
                ["MUL"] = Opcodes.MUL,
                ["DIV"] = Opcodes.DIV,
                ["MOD"] = Opcodes.MOD,

                ["AND"] = Opcodes.AND,
                ["OR"] = Opcodes.OR,
                ["XOR"] = Opcodes.XOR,

                ["SHL"] = Opcodes.SHL,
                ["SHR"] = Opcodes.SHR,
                ["ROL"] = Opcodes.ROL,
                ["ROR"] = Opcodes.ROR,

                ["WAIT"] = Opcodes.WAIT,
                ["HALT"] = Opcodes.HALT,
                ["CLEAR"] = Opcodes.CLEAR
            };

            for (byte i = 0; i < REG_MAX; i++)
            {
                dict[$"REG{i}"] = i;
            }

            list = new System.Collections.ObjectModel.ReadOnlyDictionary<string, byte>(dict);
        }
    }
}