using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpubSanitizerCore.Utils
{
    /// <summary>
    /// A class for all path related methods
    /// </summary>
    internal static class PathUtil
    {
        /// <summary>
        /// Calculate the full path of a file in OPF, given the OPF path and the file path.
        /// </summary>
        /// <param name="opfPath">OPF path</param>
        /// <param name="filePath">File path in OPF manifest</param>
        /// <returns></returns>
        internal static string ComposeOpfPath(string opfPath, string filePath)
        {
            if(filePath.StartsWith('/'))
            {
                return filePath[1..];
            }
            string normalizedOpfPath = opfPath[..(opfPath.LastIndexOf('/') + 1)];
            return normalizedOpfPath + filePath;
        }

        /// <summary>
        /// Calculate the relative path of a file which should be used in OPF
        /// </summary>
        /// <param name="opfPath">OPF path</param>
        /// <param name="filePath">absolute file path</param>
        /// <returns></returns>
        internal static string ComposeRelativePath(string opfPath, string filePath)
        {
            string normalizedOpfPath = opfPath[..(opfPath.LastIndexOf('/') + 1)];
            if (filePath.StartsWith(normalizedOpfPath))
            {
                return filePath[(normalizedOpfPath.Length)..];
            }
            else
            {
                throw new ArgumentException($"File path '{filePath}' is not under OPF path '{opfPath}'.");
            }
        }
    }
}
