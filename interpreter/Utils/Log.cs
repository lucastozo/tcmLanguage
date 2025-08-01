using System.Text;

namespace interpreter.Utils
{
    static class Log
    {
        public static long MaxFileSizeBytes { get; set; } = 12 * 1024 * 1024; // 12MB
        public static bool EnableSizeLimit { get; set; } = true;
        private static long currentFileSize = 0;
        public static bool ShowLogs { get; set; } = false;
        public static bool WriteToFile { get; set; } = false;
        public static string FilePath { get; } = Path.Combine(Environment.CurrentDirectory, 
        $"Log-{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.txt");
    
        static Log()
        {
            if (File.Exists(FilePath))
            {
                currentFileSize = new FileInfo(FilePath).Length;
            }
        }
    
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
    
            if (EnableSizeLimit && currentFileSize >= MaxFileSizeBytes)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Log file size limit reached. Logging to file disabled.");
                Console.ResetColor();
                WriteToFile = false;
                return;
            }
            
            DateTime moment = DateTime.Now;
    
            if (message.Trim().Length > 0)
            {
                message = $"[{moment:HH:mm:ss}] {message}";
            }
    
            try
            {
                File.AppendAllText(FilePath, message);
                currentFileSize += Encoding.UTF8.GetByteCount(message);
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Failed to write to log file: {e.Message}");
                Console.ResetColor();
            }
        }
    }
}