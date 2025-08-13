namespace EpubSanitizerCore.Filters
{
    public enum Threads
    {
        Single = 1,
        Multi = 2
    }
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

        internal Filter(EpubSanitizer CoreInstance)
        {
            Instance = CoreInstance;
        }

        /// <summary>
        /// All available filters
        /// </summary>
        internal static readonly Dictionary<string, Type> Filters = new(){
            {"default", typeof(General)},
            {"general", typeof(General)},
            {"vitalsource", typeof(VitalSource)}
        };

        /// <summary>
        /// Calculate what files need to be processed by this filter
        /// </summary>
        /// <returns>Array of files</returns>
        internal abstract string[] GetProcessList();

        /// <summary>
        /// Do preparations for processing
        /// Do not put heavy compute here
        /// </summary>
        internal virtual void PreProcess()
        {

        }

        /// <summary>
        /// Process all files in the filter
        /// </summary>
        internal abstract void ProcessFiles();

        /// <summary>
        /// Do post process actions like cleaning or merging
        /// </summary>
        internal virtual void PostProcess()
        {

        }

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
