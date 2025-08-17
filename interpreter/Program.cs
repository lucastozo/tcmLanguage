using interpreter.Core;
using interpreter.Parsing;
using interpreter.Utils;

if (args.Length == 0) Log.PrintError("You must type the path to your code file as a argument.");

string filePath = args[0];

for (int i = 1; i < args.Length; i++)
{
    string arg = args[i].ToLower();
    switch (arg)
    {
        case "--log":
        case "-l":
            Log.ShowLogs = true;
            break;
        case "--write":
        case "-w":
            Log.WriteToFile = true;
            break;
        case "--size":
        case "-s":
            if (i + 1 < args.Length && int.TryParse(args[i + 1], out int sizeMB))
            {
                Log.MaxFileSizeBytes = sizeMB * 1024 * 1024;
                Log.PrintMessage($"Log file size limit set to {sizeMB}MB");
                i++;
            }
            else
            {
                Log.PrintError("Invalid or missing size value after --size");
            }
            break;
        case "--no-limit":
            Log.EnableSizeLimit = false;
            Log.PrintMessage("Log file size limit disabled");
            break;
    }
}

if (Log.ShowLogs) Log.PrintMessage("Executing program with logs enabled");
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

VirtualMachine vm = new VirtualMachine();
Interpreter interpreter = new Interpreter(vm);
try
{
    var (instructions, settings) = Parser.GetInstructionsWithSettings(filePath);
    interpreter.SetInstructionSettings(settings);
    interpreter.Run(instructions);
    Log.PrintMessage("-- END OF PROGRAM --");
}
catch (Exception e)
{
    Log.PrintError(e.Message);
}