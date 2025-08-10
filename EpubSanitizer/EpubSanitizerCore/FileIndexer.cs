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
        internal string id = string.Empty;
        /// <summary>
        /// Relative path to OPF file
        /// </summary>
        internal string opfpath = string.Empty;
        /// <summary>
        /// Path inside Epub file
        /// </summary>
        internal string path = string.Empty;
        /// <summary>
        /// mimetype of the file
        /// </summary>
        internal string mimetype = string.Empty;
        /// <summary>
        /// Original XML element in the OPF manifest
        /// </summary>
        internal XmlElement? originElement;
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
        /// Path of OPF file, relative to Epub root
        /// </summary>
        internal string OpfPath = string.Empty;
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
            OpfPath = containerDoc.GetElementsByTagName("rootfile")[0].Attributes["full-path"].Value;
            string opfcontent;
            try
            {
                opfcontent = Instance.FileStorage.ReadString(OpfPath);
            }
            catch (FileNotFoundException)
            {
                throw new InvalidEpubException("OPF file not found in the Epub file.");
            }
            opfDoc.LoadXml(opfcontent);
            XmlNodeList manifestNodes = opfDoc.GetElementsByTagName("manifest")[0].ChildNodes;
            foreach (XmlNode file in manifestNodes)
            {
                // Skip comment nodes
                if (file.NodeType == XmlNodeType.Comment)
                {
                    continue;
                }
                OpfFile FileInfo = new()
                {
                    id = file.Attributes["id"]?.Value ?? string.Empty,
                    opfpath = file.Attributes["href"]?.Value ?? string.Empty,
                    path = Utils.PathUtil.ComposeOpfPath(OpfPath, file.Attributes["href"]?.Value) ?? string.Empty,
                    mimetype = file.Attributes["media-type"]?.Value ?? string.Empty,
                    originElement = file as XmlElement
                };
                if (FileInfo.path == string.Empty)
                {
                    Instance.Logger($"Invalid file entry in manifest: {file.OuterXml}, file will be excluded.");
                    continue;
                }
                if (FileInfo.opfpath == '/' + FileInfo.path)
                {
                    Instance.Logger($"File '{FileInfo.path}' is absolute path, try normalizing.");
                    try
                    {
                        FileInfo.path = Utils.PathUtil.ComposeRelativePath(OpfPath, FileInfo.path);
                    }
                    catch (ArgumentException)
                    {
                        Instance.Logger($"File '{FileInfo.path}' is outside of OPF path '{OpfPath}', will be moved.");
                        // TODO: move file directory to OPF path
                    }
                }
                if (!Instance.FileStorage.FileExists(FileInfo.path))
                {
                    Instance.Logger($"File '{FileInfo.path}' not found in Epub file, will be excluded from manifest.");
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
            string[] AllFiles = Instance.FileStorage.GetAllFiles();
            foreach (string file in AllFiles)
            {
                if (file.StartsWith("META-INF/") || file == "mimetype" || file == OpfPath)
                {
                    continue;
                }
                if (!ManifestFiles.Any(f => f.path == file))
                {
                    Instance.Logger($"File '{file}' not found in manifest, try adding to list.");
                    OpfFile FileInfo = new()
                    {
                        id = Instance.FileStorage.GetSHA256(file),
                        opfpath = Utils.PathUtil.ComposeRelativePath(OpfPath, file),
                        path = file,
                        mimetype = MimeTypesMap.GetMimeType(file)
                    };
                }
            }
        }
    }
}
