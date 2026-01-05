using System.Reflection;
using System.Text;

namespace interpreter.Utils
{
    class ArgsHandler
    {
        public static int VmCodePage {get; private set;}
        public static Encoding VmEncoding { get; private set;} = Encoding.GetEncoding("iso-8859-1");
        
        public static string Handle(string[] args)
        {
            if (args.Length == 0)
            {
                PrintHelp();
                Environment.Exit(0);
            }

            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i].ToLower();

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
                    case "--encoding":
                    case "-e":
                        if (i + 1 < args.Length)
                        {
                            string encodingID = args[i + 1];
                            try
                            {
                                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                                VmCodePage = Convert.ToInt32(encodingID);
                                VmEncoding = Encoding.GetEncoding(VmCodePage); // validate
                                i++;
                            }
                            catch (Exception)
                            {
                                Log.PrintError($"Invalid encoding identifier '{encodingID}'." +
                                    " Please refer to https://learn.microsoft.com/en-us/windows/win32/intl/code-page-identifiers for valid identifiers.");
                                Environment.Exit(1);
                            }
                        }
                        else
                        {
                            Log.PrintError("No encoding specified after --encoding/-e option.");
                            Environment.Exit(1);
                        }
                        break;
                    default:
                        if (arg.StartsWith("-"))
                        {
                            Log.PrintError($"Invalid option '{arg}'. Use -h or --help to get instructions");
                        }
                        break;
                }
            }

            string filePath = args[0];
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
                ("--encoding, -e <encoding>", "Specify the file encoding (default is ISO-8859-1)."),
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