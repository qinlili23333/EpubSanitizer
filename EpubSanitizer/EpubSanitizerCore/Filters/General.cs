namespace EpubSanitizerCore.Filters
{
    internal class General(EpubSanitizer CoreInstance) : SingleThreadFilter(CoreInstance)
    {
        internal override string[] GetProcessList()
        {
            throw new NotImplementedException();
        }

        internal override void Process(string file)
        {
            throw new NotImplementedException();
        }
    }
}
