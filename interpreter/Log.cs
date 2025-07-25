static class Log
{
    public static bool ShowLogs { get; set; } = false;
    public static bool WriteToFile { get; set; } = false;
    public static string FilePath { get; } = Environment.CurrentDirectory + "/" +
                                            $"Log-{DateTime.Now}".Replace(' ', '-').Replace('/','-') +
                                            ".txt";

    public static void PrintMessage(string message = "")
    {
        if (!ShowLogs) return;

        message += "\n";
        Console.Write(message);
        WriteOnFile(message);
    }

    public static void PrintError(string message)
    {
        ShowLogs = true;
        Console.ForegroundColor = ConsoleColor.Red;
        PrintMessage(message);
        Console.ResetColor();
        Environment.Exit(1);
    }

    private static void WriteOnFile(string message)
    {
        if (!WriteToFile) return;
        
        DateTime moment = DateTime.Now;

        if (message.Trim().Length > 0)
        {
            message = $"[{moment.Hour}:{moment.Minute}:{moment.Second}] {message}";
        }
        
        try
        {
            File.AppendAllText(FilePath, message);
        }
        catch (Exception e)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Failed to write to log file: {e.Message}");
            Console.ResetColor();
        }
    }
}