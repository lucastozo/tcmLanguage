const string pathToFile = "smallTest.tc";
VirtualMachine vm = new VirtualMachine();
Interpreter i = new Interpreter(vm);
List<Instruction> instructions = Parser.GetInstructions(pathToFile);
i.Run(instructions);
Console.WriteLine("-- END OF PROGRAM --");