using System.ComponentModel;

namespace EpubSanitizerCore.Filters
{
    /// <summary>
    /// Abstract class of single thread filter
    /// </summary>
    internal abstract class SingleThreadFilter(EpubSanitizer CoreInstance) : Filter(CoreInstance)
    {
        internal override void ProcessFiles()
        {
            string[] files = GetProcessList();
            if (files.Length == 0)
            {
                Instance.Logger("No files to process in filter " + GetType().Name);
                return;
            }
            Instance.Logger($"Processing {files.Length} files in filter {GetType().Name}");
            foreach (string file in files)
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
            }
        }
    }
}
