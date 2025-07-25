if (args.Length == 0) Log.PrintError("You must type the path to your code file as a argument.");
else if (args.Length > 1)
{
    if (args[1].ToLower() == "log") Log.ShowLogs = true;
    if (args.Length > 2 && args[2].ToLower() == "write") Log.WriteToFile = true;
    Log.PrintMessage("Executing program with logs enabled");
    if (Log.WriteToFile) Log.PrintMessage("Logs will be written to a file");
}

string filePath = args[0];

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

VirtualMachine vm = new VirtualMachine();
Interpreter i = new Interpreter(vm);
try
{
    List<Instruction> instructions = Parser.GetInstructions(filePath);
    i.Run(instructions);
    Log.PrintMessage("-- END OF PROGRAM --");
}
catch (Exception e)
{
    Log.PrintError(e.Message);
}