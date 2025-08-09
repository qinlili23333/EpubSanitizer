namespace EpubSanitizerCore.Filters
{
    /// <summary>
    /// Abstract class of multi thread filter, but MT support is not ready so currently just process as single thread
    /// TODO: implement multi thread
    /// </summary>
    internal abstract class MultiThreadFilter(EpubSanitizer CoreInstance) : SingleThreadFilter(CoreInstance)
    {
    }
}
