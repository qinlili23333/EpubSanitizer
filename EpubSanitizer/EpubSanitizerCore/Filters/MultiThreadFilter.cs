namespace EpubSanitizerCore.Filters
{
    /// <summary>
    /// Abstract class of multi thread filter, but MT support is not ready so currently just process as single thread
    /// </summary>
    internal abstract class MultiThreadFilter(EpubSanitizer CoreInstance) : SingleThreadFilter(CoreInstance)
    {
        /// <summary>
        /// Processes the list of files concurrently using a parallel loop, or fallback to single thread if disabled.
        /// </summary>
        internal override void ProcessFiles()
        {
            if (Instance.Config.GetEnum<Threads>("threads") == Threads.Single)
            {
                Instance.Logger("Multi-threading is disabled, processing files in single thread mode.");
                ProcessFilesStatic(this);
                return;
            }
            string[] files = GetProcessList();
            if (files.Length == 0)
            {
                Instance.Logger($"No files to process in filter {GetType().Name}");
                return;
            }
            Instance.Logger($"Processing {files.Length} files in filter {GetType().Name} using multiple threads.");
            Parallel.ForEach(files, file =>
            {
                try
                {
                    Instance.Logger($"Processing file {file} in filter {GetType().Name}");
                    Process(file);
                }
                catch (Exception ex)
                {
                    Instance.Logger($"Error processing file {file} in filter {GetType().Name}: {ex.Message}");
                }
            });
        }
    }
}
