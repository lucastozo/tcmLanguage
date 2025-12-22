using System.Reflection;

namespace interpreter.Utils
{
    class ArgsHandler
    {
        public static string Handle(string[] args)
        {
            if (args.Length == 0)
            {
                PrintHelp();
                Environment.Exit(0);
            }

            foreach (string arg in args)
            {
                switch (arg)
                {
                    case "--help":
                    case "-h":
                        PrintHelp();
                        Environment.Exit(0);
                        break;
                    case "--version":
                    case "-v":
                        Console.WriteLine(GetVersion());
                        Environment.Exit(0);
                        break;
                }
            }

            string filePath = args[0];
            
            for (int i = 1; i < args.Length; i++)
            {
                string arg = args[i].ToLower();
                switch (arg)
                {
                    default:
                        Log.PrintError($"Invalid option '{arg}'. Use -h or --help to get instructions");
                        break;
                }
            }

            return filePath;
        }

        private static void PrintHelp()
        {
            Console.WriteLine($"Usage: {Assembly.GetExecutingAssembly().GetName().Name} <file> [options]");
            Console.WriteLine("\nOptions:");

            var options = new List<(string Option, string Description)>
            {
                ("--help, -h", "Show this help message."),
                ("--version, -v", "Show the installed version of the interpreter."),
            };

            int width = 0;
            foreach (var option in options)
            {
                width = Math.Max(width, option.Option.Length);
            }

            foreach (var option in options)
            {
                Console.WriteLine("{0,-" + (width + 5) + "} {1}", option.Option, option.Description);
            }

            Console.WriteLine();
        }

        public static string GetVersion()
        {
            var name = Assembly.GetExecutingAssembly().GetName().Name;
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            string versionString = Assembly.GetExecutingAssembly()
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion ?? version?.ToString() ?? "unknown";
            
            int plusIndex = versionString.IndexOf('+');
            if (plusIndex >= 0 && versionString.Length > plusIndex + 8)
            {
                versionString = versionString.Substring(0, plusIndex + 8);
            }

            return $"{name} {versionString}";
        }
    }
}