const string pathToFile = "fibonacciExample.tc";
VirtualMachine vm = new VirtualMachine();
Interpreter i = new Interpreter(vm);
List<Instruction> instructions = Parser.GetInstructions(pathToFile);
i.Run(instructions);
System.Console.WriteLine("END");