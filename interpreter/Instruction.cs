class Instruction
{
    public byte Opcode { get; set; } = 0;
    public byte Arg1 { get; set; } = 0;
    public byte Arg2 { get; set; } = 0;
    public byte Destination { get; set; } = 0;

    public Instruction(byte opcode, byte arg1, byte arg2, byte destination)
    {
        Opcode = opcode;
        Arg1 = arg1;
        Arg2 = arg2;
        Destination = destination;
    }
}