namespace EpubSanitizerCLI
{
    public enum ExitCode
    {
        INVALID_ARGS = -1,
        DONE = 0
    }

    public class CliEntry
    {

        static void Main(string[] args)
        {
            PrintWelcome();
            // Print help if no args
            if (args.Length == 0)
            {
                PrintUsage();
                Environment.Exit((int)ExitCode.INVALID_ARGS);
            }
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
            Console.WriteLine("    --threads=single|multi    Enable multithread processing or not, multithread provides faster speed on multi core devices, but may affect system responsibility on low end devices, default value is 'single'.");
            Console.WriteLine("    --overwrite               Overwrite sanitized file to input file. If process crashed of power lost, you may lose your file. Use at your own risk!");
            Console.WriteLine("Special arguments:");
            Console.WriteLine("    -v                        Print version information.");
            Console.WriteLine("    -h                        Print this general help.");
            Console.WriteLine("    -h filter_name            Print help of specific filter.");
        }
    }
}
