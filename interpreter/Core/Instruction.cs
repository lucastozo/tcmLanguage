namespace interpreter.Core
{
    class Instruction
    {
        public int Opcode { get; } = 0;
        public float Arg1 { get; set; } = 0;
        public float Arg2 { get; set; } = 0;
        public int Destination { get; } = 0;
    
        public Instruction(int opcode, float arg1, float arg2, int destination)
        {
            Opcode = opcode;
            Arg1 = arg1;
            Arg2 = arg2;
            Destination = destination;
        }
    }
}