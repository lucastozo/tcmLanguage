using interpreter.Core;
using interpreter.Parsing;
using interpreter.Utils;

string filePath = ArgsHandler.Handle(args);

try
{
    if (!File.Exists(filePath)) throw new FileLoadException();
}
catch (Exception e)
{
    Log.PrintError(e.Message);
}

try
{
    var (instructions, settings) = Parser.GetInstructionsWithSettings(filePath);

    VirtualMachine vm = new VirtualMachine();
    Interpreter interpreter = new Interpreter(vm, settings);
    
    interpreter.Run(instructions);
}
catch (Exception e)
{
    Log.PrintError(e.Message);
}