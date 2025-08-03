namespace EpubSanitizerCore
{
    public class Exceptions
    {
        /// <summary>
        /// An exception raised when config not found
        /// </summary>
        public class ConfigNotFoundException : Exception
        {
            internal ConfigNotFoundException(string message) : base($"'{message}' does not exist in config")
            {
            }
        }

        /// <summary>
        /// Epub file is not an Epub file
        /// </summary>
        public class InvalidEpubException : Exception
        {
            internal InvalidEpubException(string message) : base($"Invalid EPUB file: {message}")
            {
            }
        }
    }
}
