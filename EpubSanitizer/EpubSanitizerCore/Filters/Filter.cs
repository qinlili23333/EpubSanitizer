namespace EpubSanitizerCore.Filters
{
    /// <summary>
    /// Abstract class of filter
    /// </summary>
    internal abstract class Filter
    {

        /// <summary>
        /// All available filters
        /// </summary>
        internal static Dictionary<string, Type> Filters = new(){
            {"default", typeof(General)},
            {"general", typeof(General)}
        };

    }
}
