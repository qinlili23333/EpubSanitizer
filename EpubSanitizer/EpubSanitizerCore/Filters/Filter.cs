using System.Runtime.CompilerServices;

namespace EpubSanitizerCore.Filters
{
    public interface IHelpProvider
    {
        /// <summary>
        /// When implemented in a type, provides a static method to print help text.
        /// </summary>
        static abstract void PrintHelp();
    }

    /// <summary>
    /// Abstract class of filter
    /// </summary>
    internal abstract class Filter : IHelpProvider
    {
        internal readonly EpubSanitizer Instance;

        internal Filter(EpubSanitizer CoreInstance) {
            Instance = CoreInstance;
        }

        /// <summary>
        /// All available filters
        /// </summary>
        internal static Dictionary<string, Type> Filters = new(){
            {"default", typeof(General)},
            {"general", typeof(General)}
        };

        /// <summary>
        /// Calculate what files need to be processed by this filter
        /// </summary>
        /// <returns>Array of files</returns>
        internal abstract string[] GetProcessList();

        /// <summary>
        /// Process all files in the filter
        /// </summary>
        internal abstract void ProcessFiles();

        /// <summary>
        /// Process a single file
        /// </summary>
        /// <param name="file">path of file</param>
        internal abstract void Process(string file);

        public static void PrintHelp()
        {
            throw new NotImplementedException();
        }
    }
}
