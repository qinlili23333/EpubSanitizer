using HeyRed.Mime;
using System.Xml;
using static EpubSanitizerCore.Exceptions;

namespace EpubSanitizerCore
{
    internal class OpfFile
    {
        /// <summary>
        /// id in the manifest
        /// </summary>
        internal string id;
        /// <summary>
        /// Path inside Epub file
        /// </summary>
        internal string path;
        /// <summary>
        /// mimetype of the file
        /// </summary>
        internal string mimetype;
    }
    internal class FileIndexer
    {
        /// <summary>
        /// The instance, mainly used for getting config
        /// </summary>
        internal readonly EpubSanitizer Instance;
        /// <summary>
        /// XML Document for container file
        /// </summary>
        internal XmlDocument containerDoc = new();
        /// <summary>
        /// XML Document for OPF file
        /// </summary>
        internal XmlDocument opfDoc = new();
        /// <summary>
        /// List of files in the manifest
        /// </summary>
        internal OpfFile[] ManifestFiles = [];
        internal FileIndexer(EpubSanitizer CoreInstance)
        {
            Instance = CoreInstance;
        }
        /// <summary>
        /// Parse package manifest and index files
        /// </summary>
        internal void IndexFiles()
        {
            string container;
            try
            {
                container = Instance.FileStorage.ReadString("META-INF/container.xml");
            }
            catch (FileNotFoundException)
            {
                throw new InvalidEpubException("Container file not found in the Epub file.");
            }
            containerDoc.LoadXml(container);
            string opfpath = containerDoc.GetElementsByTagName("rootfile")[0].Attributes["full-path"].Value;
            string opfcontent;
            try
            {
                opfcontent = Instance.FileStorage.ReadString(opfpath);
            }
            catch (FileNotFoundException)
            {
                throw new InvalidEpubException("OPF file not found in the Epub file.");
            }
            opfDoc.LoadXml(opfcontent);
            XmlNodeList manifestNodes = opfDoc.GetElementsByTagName("manifest")[0].ChildNodes;
            foreach (XmlNode file in manifestNodes)
            {
                OpfFile FileInfo = new()
                {
                    id = file.Attributes["id"]?.Value ?? string.Empty,
                    path = file.Attributes["href"]?.Value ?? string.Empty,
                    mimetype = file.Attributes["media-type"]?.Value ?? string.Empty
                };
                if (FileInfo.path == string.Empty)
                {
                    Instance.Logger($"Invalid file entry in manifest: {file.OuterXml}, file will be excluded.");
                    continue;
                }
                if (FileInfo.id == string.Empty)
                {
                    Instance.Logger($"Lack file id: {file.OuterXml}, use hash as id.");
                    FileInfo.id = Instance.FileStorage.GetSHA256(FileInfo.path);
                }
                if (FileInfo.mimetype == string.Empty)
                {
                    Instance.Logger($"Lack file mimetype: {file.OuterXml}, try getting by extension.");
                    FileInfo.mimetype = MimeTypesMap.GetMimeType(FileInfo.path);
                }
                ManifestFiles = [.. ManifestFiles, FileInfo];
            }
        }
    }
}
