using EpubSanitizerCore;
using System.IO.Compression;
using System.Text;

namespace EpubSanitizerCLI
{
    internal static class CliEntry
    {
        enum ExitCode
        {
            IO_ERROR = -3,
            FILE_NOT_EXIST = -2,
            INVALID_ARGS = -1,
            DONE = 0
        }

        static DateTime LastActionTime;

        static CliEntry()
        {
            LastActionTime = DateTime.Now;
        }

        static string input = string.Empty;

        static string output = string.Empty;

        static Dictionary<string, string> Config = [];

        static void Exit(ExitCode code)
        {
#if RELEASEFREE
            // Free version with random exit code
            Environment.Exit(new Random().Next(-100, 0));
#else
            Environment.Exit((int)code);
#endif
        }

        static void Main(string[] args)
        {
            PrintWelcome();
            // Print help if no args
            if (args.Length == 0)
            {
                EpubSanitizer.PrintUsage();
                Exit(ExitCode.INVALID_ARGS);
            }
            ParseArgs(args);
            Log("Creating instance...");
            EpubSanitizer Instance = new()
            {
                Logger = Log
            };
            Instance.Config.LoadConfigString(Config);
            if (!File.Exists(input))
            {
                Error("Input file not exist!");
                Exit(ExitCode.FILE_NOT_EXIST);
            }
            Log("Loading file...");
            Stream FileStream = File.OpenRead(input);
            ZipArchive EpubFile = new(FileStream, ZipArchiveMode.Read);
            Instance.LoadFile(EpubFile);
            EpubFile.Dispose();
            FileStream.Close();
            Log("Processing...");
            Instance.Process();
            Log("Saving file...");
            if (File.Exists(output))
            {
                Log("Removing old file...");
                try
                {
                    File.Delete(output);
                }
                catch (Exception ex)
                {
                    Error("Failed to delete old file: " + ex.Message);
                    Exit(ExitCode.IO_ERROR);
                }
            }
            FileStream = File.OpenWrite(output);
            EpubFile = new(FileStream, ZipArchiveMode.Create, true, Encoding.UTF8);
            Instance.SaveFile(EpubFile);
            EpubFile.Dispose();
            FileStream.Close();
            Log("Cleaning...");
            Instance.Dispose();
            Log("Done!");
            Exit(ExitCode.DONE);
        }

        /// <summary>
        /// Print log message with time diff
        /// </summary>
        /// <param name="message"></param>
        static void Log(string message)
        {
            DateTime dateTime = DateTime.Now;
            Console.WriteLine($"[{dateTime:hh.mm.ss.fff}]{message}[+{(int)(dateTime - LastActionTime).TotalMilliseconds}ms]");
            LastActionTime = dateTime;
        }

        /// <summary>
        /// Log error message to console
        /// </summary>
        /// <param name="message"></param>
        static void Error(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[Error]" + message);
            Console.ResetColor();
        }

        /// <summary>
        /// Print welcome message
        /// </summary>
        static void PrintWelcome()
        {
            Console.WriteLine("EpubSanitizerCLI by Qinlili");
#if RELEASEFREE
            Console.WriteLine("You are using FREE version, enjoy these features added: random exit code, night sleep");
            if(IsBetween11PMAnd5AM())
            {
                Console.WriteLine("To avoid disturbing your sleep at night, both the text and background will turn black.");
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Black;
            }
#endif
        }

        /// <summary>
        /// Check if current time is between 11 PM and 5 AM
        /// </summary>
        /// <returns>true if in time</returns>
        public static bool IsBetween11PMAnd5AM() => DateTime.Now.TimeOfDay >= new TimeSpan(23, 0, 0) || DateTime.Now.TimeOfDay < new TimeSpan(5, 0, 0);



        /// <summary>
        /// Parse all arguments
        /// </summary>
        /// <param name="args"></param>
        static void ParseArgs(string[] args)
        {
            if (args[0] == "-v")
            {
                PrintVersion();
                Exit(ExitCode.DONE);
            }
            else if (args[0] == "-h")
            {
                if (args.Length > 1)
                {
                    EpubSanitizer.PrintFilterHelp(args[1]);
                    Exit(ExitCode.DONE);
                }
                else
                {
                    EpubSanitizer.PrintUsage();
                    Exit(ExitCode.DONE);
                }
            }
            else if (args[0] == "-f")
            {
                Console.WriteLine("Available filters:");
                foreach (var item in EpubSanitizer.GetFilters())
                {
                    Console.Write(item + ",");
                }
                Exit(ExitCode.DONE);
            }
            // Process normal parse
            int i;
            for (i = 0; i < args.Length; i++)
            {
                if (args[i].StartsWith("--"))
                {
                    if (args[i].Contains('='))
                    {
                        var div = args[i].Split('=');
                        Config.Add(div[0][2..], div[1]);
                    }
                    else
                    {
                        Config.Add(args[i][2..], "true");
                    }
                }
                else
                {
                    break;
                }
            }
            input = args[i];
            if (Config.ContainsKey("overwrite"))
            {
                output = (args.Length > i + 1) ? args[i + 1] : input;
            }
            else
            {
                output = (args.Length > i + 1) ? args[i + 1] : args[i].Replace(".epub", "_out.epub");
                if (File.Exists(output))
                {
                    Error("Output file already exists! Use --overwrite to overwrite it.");
                    Exit(ExitCode.INVALID_ARGS);
                }
            }
        }

        /// <summary>
        /// Print out version
        /// </summary>
        static void PrintVersion()
        {
            Console.WriteLine($"CLI Version: {typeof(CliEntry).Assembly.GetName().Version}");
            Console.WriteLine($"Core Version: {typeof(EpubSanitizer).Assembly.GetName().Version}");
        }
    }
}
