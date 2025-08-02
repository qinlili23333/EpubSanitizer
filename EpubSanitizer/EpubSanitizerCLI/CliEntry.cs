using EpubSanitizerCore;
using System.IO.Compression;

namespace EpubSanitizerCLI
{
    public enum ExitCode
    {
        FILE_NOT_EXIST = -2,
        INVALID_ARGS = -1,
        DONE = 0
    }

    public class CliEntry
    {
        static DateTime LastActionTime;

        static CliEntry()
        {
            LastActionTime = DateTime.Now; ;
        }

        static string input = string.Empty;

        static string output = string.Empty;

        static Dictionary<string, string> Config = [];

        static void Main(string[] args)
        {
            PrintWelcome();
            // Print help if no args
            if (args.Length == 0)
            {
                PrintUsage();
                Environment.Exit((int)ExitCode.INVALID_ARGS);
            }
            Log("Initialize parameters...");
            ParseArgs(args);
            Log("Creating instance...");
            EpubSanitizer Instance = new();
            Instance.Config.LoadConfigString(Config);
            if (!File.Exists(input))
            {
                Error("Input file not exist!");
                Environment.Exit((int)ExitCode.FILE_NOT_EXIST);
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
            FileStream = File.OpenWrite(output);
            EpubFile = new(FileStream, ZipArchiveMode.Create);
            Instance.SaveFile(EpubFile);
            EpubFile.Dispose();
            FileStream.Close();
            Log("Cleaning...");
            Instance.Dispose();
            Log("Done!");
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
        }

        /// <summary>
        /// Print command line usage
        /// </summary>
        static void PrintUsage()
        {
            Console.WriteLine("Usage: EpubSanitizerCLI <options> file <output>");
            Console.WriteLine("e.g. EpubSanitizerCLI --filter=default,vitalsource extract.epub sanitized.epub");
            Console.WriteLine();
            Console.WriteLine("Universal options:");
            Console.WriteLine("    --filter=xxx              The filter used for xhtml processing, default value is 'default' which only enables general filter");
            Console.WriteLine("    --compress=0              Compression level used for compressible file, value in number as CompressionLevel Enum of .NET, default value is 0. Not applicable to non-compressible files.");
            Console.WriteLine("    --cache=ram|disk          Where to store cache during sanitization, ram mode privides faster speed but may consume enormous memory, default value is 'ram'.");
            Console.WriteLine("    --threads=single|multi    Enable multithread processing or not, multithread provides faster speed on multi core devices, but may affect system responsibility on low end devices, default value is 'single', currently multithread is not implemented.");
            Console.WriteLine("    --overwrite               Overwrite sanitized file to input file. If process crashed of power lost, you may lose your file. Use at your own risk!");
            Console.WriteLine("Special arguments:");
            Console.WriteLine("    -v                        Print version information.");
            Console.WriteLine("    -h                        Print this general help.");
            Console.WriteLine("    -h filter_name            Print help of specific filter.");
        }

        /// <summary>
        /// Parse all arguments
        /// </summary>
        /// <param name="args"></param>
        static void ParseArgs(string[] args)
        {
            if (args[0] == "-v")
            {
                PrintVersion();
                Environment.Exit((int)ExitCode.DONE);
            }
            else if (args[0] == "-h")
            {
                if (args.Length > 1)
                {
                    // TODO: print help of specific filter
                }
                else
                {
                    PrintUsage();
                    Environment.Exit((int)ExitCode.DONE);
                }
            }
            // Process normal parse
            int i;
            for (i = 0; i < args.Length; i++)
            {
                if (args[i].StartsWith("--"))
                {
                    if (args[i].Contains("="))
                    {
                        var div = args[i].Split('=');
                        Config.Add(div[0][2..], div[1]);
                    }
                    else
                    {
                        Config.Add(args[i][2..], "1");
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
                output = input;
            }
            else
            {
                output = (args.Length > i + 1) ? args[i + 1] : args[i].Replace(".epub", "_out.epub");
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
