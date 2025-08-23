using interpreter.Core;
using interpreter.Parsing;
using interpreter.Utils;

string filePath = ArgsHandler.Handle(args);

Log.PrintMessage(ArgsHandler.GetVersion()); // Print interpreter version in log
Log.PrintMessage("Executing program with logs enabled");
if (Log.WriteToFile) Log.PrintMessage("Logs will be written to a file");

Log.PrintMessage($"Trying to load file {filePath}...");
try
{
    if (!File.Exists(filePath)) throw new FileLoadException();
}
catch (Exception e)
{
    Log.PrintError(e.Message);
}

Log.PrintMessage("File loaded sucessfully");

try
{
    var (instructions, settings) = Parser.GetInstructionsWithSettings(filePath);

    VirtualMachine vm = new VirtualMachine();
    Interpreter interpreter = new Interpreter(vm, settings);
    
    interpreter.Run(instructions);
    Log.PrintMessage("-- END OF PROGRAM --");
}
catch (Exception e)
{
    Log.PrintError(e.Message);
}