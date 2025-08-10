namespace EpubSanitizerCore.Filters
{
    /// <summary>
    /// Abstract class of single thread filter
    /// </summary>
    internal abstract class SingleThreadFilter(EpubSanitizer CoreInstance) : Filter(CoreInstance)
    {
        internal override void ProcessFiles()
        {
            ProcessFilesStatic(this);
        }

        /// <summary>
        /// Static method to process files in a filter, share to multi thread filter when multi thread disabled
        /// </summary>
        /// <param name="filter">Filter instance</param>
        internal static void ProcessFilesStatic(Filter filter)
        {
            string[] files = filter.GetProcessList();
            if (files.Length == 0)
            {
                filter.Instance.Logger("No files to process in filter " + filter.GetType().Name);
                return;
            }
            filter.Instance.Logger($"Processing {files.Length} files in filter {filter.GetType().Name}");
            foreach (string file in files)
            {
                try
                {
                    filter.Instance.Logger($"Processing file {file} in filter {filter.GetType().Name}");
                    filter.Process(file);
                }
                catch (Exception ex)
                {
                    filter.Instance.Logger($"Error processing file {file} in filter {filter.GetType().Name}: {ex.Message}");
                }
            }
        }
    }
}
