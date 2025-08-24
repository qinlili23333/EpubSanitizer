namespace EpubSanitizerCore.Filters
{
    internal class Privacy(EpubSanitizer CoreInstance) : MultiThreadFilter(CoreInstance)
    {
        internal override string[] GetProcessList()
        {
            return Utils.PathUtil.GetAllXHTMLFiles(Instance);
        }

        internal override void Process(string file)
        {
            throw new NotImplementedException();
        }
    }
}
