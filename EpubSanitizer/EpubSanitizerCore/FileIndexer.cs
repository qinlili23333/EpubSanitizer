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
        internal required string id;
        /// <summary>
        /// Relative path to OPF file
        /// </summary>
        internal required string opfpath;
        /// <summary>
        /// Path inside Epub file
        /// </summary>
        internal required string path;
        /// <summary>
        /// mimetype of the file
        /// </summary>
        internal required string mimetype;
        /// <summary>
        /// properties of the file, used for OPF 3.0
        /// </summary>
        internal string properties = string.Empty;
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
        /// NCX file path, relative to Epub root, if exists
        /// </summary>
        internal string NcxPath = string.Empty;
        /// <summary>
        /// NCX XML Document, if exists
        /// </summary>
        internal XmlDocument? NcxDoc = null;
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
            LoadOpf();
            XmlNodeList manifestNodes = opfDoc.GetElementsByTagName("manifest")[0].ChildNodes;
            foreach (XmlNode file in manifestNodes)
            {
                // Skip comment nodes
                if (file.NodeType == XmlNodeType.Comment)
                {
                    continue;
                }
                AddManifestFile(file);
            }
            CheckMissingFile();
            DetectNcx();
        }

        /// <summary>
        /// Parse container xml file then load OPF file.
        /// </summary>
        /// <exception cref="InvalidEpubException"></exception>
        private void LoadOpf()
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
            XmlNodeList rootfiles = containerDoc.GetElementsByTagName("rootfile");
            if (rootfiles.Count > 1)
            {
                Instance.Logger("Support to EPUB 3 Multiple-Rendition Publications is not finished. Currently only the first one will be processed.");
            }
            OpfPath = rootfiles[0].Attributes["full-path"].Value;
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
            Utils.XmlUtil.NormalizeXmlns(opfDoc, "http://www.idpf.org/2007/opf");
            if (opfDoc.GetElementsByTagName("package")[0] is XmlElement packageElement && packageElement.GetAttribute("version") != "3.0")
            {
                if (Instance.Config.GetInt("epubVer") == 3 || (Instance.Config.GetInt("epubVer") == 0 && !Instance.Config.GetBool("overwrite")))
                {
                    Instance.Logger("Epub 2.x found, will upgrade to 3.x.");
                    packageElement.SetAttribute("version", "3.0");
                }
                else
                {
                    if (Instance.Config.GetInt("epubVer") == 0 && Instance.Config.GetBool("overwrite"))
                    {
                        Instance.Logger("Epub 2.x found but overwrite is enabled, upgrade will not enable. You can force upgrade with --epubVer=3.");
                    }
                    else
                    {
                        Instance.Logger("Epub 2.x found, but keep it based on config.");
                    }
                    Instance.TargetEpubVer = 2;
                }
            }
        }

        /// <summary>
        /// Check and add files not in manifest.
        /// </summary>
        private void CheckMissingFile()
        {
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
                    ManifestFiles = [.. ManifestFiles, FileInfo];
                }
            }
        }

        /// <summary>
        /// Parse XmlNode of file item in manifest and add to list
        /// </summary>
        /// <param name="file">XmlNode element</param>
        private void AddManifestFile(XmlNode file)
        {
            OpfFile FileInfo = new()
            {
                id = file.Attributes["id"]?.Value ?? string.Empty,
                opfpath = file.Attributes["href"]?.Value ?? string.Empty,
                path = Utils.PathUtil.ComposeOpfPath(OpfPath, file.Attributes["href"]?.Value) ?? string.Empty,
                mimetype = file.Attributes["media-type"]?.Value ?? string.Empty,
                properties = file.Attributes["properties"]?.Value ?? string.Empty,
                originElement = file as XmlElement
            };
            if (FileInfo.path == string.Empty || !Instance.FileStorage.FileExists(FileInfo.path))
            {
                Instance.Logger($"Invalid file entry in manifest: {file.OuterXml}, file will be excluded.");
                return;
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

        /// <summary>
        /// Find whether there is a NCX file in the Epub.
        /// </summary>
        private void DetectNcx()
        {
            if (opfDoc.GetElementsByTagName("spine")[0] is XmlElement spineElement && spineElement.GetAttribute("toc") != string.Empty)
            {
                string ncxid = spineElement.GetAttribute("toc");
                foreach (OpfFile file in ManifestFiles)
                {
                    if (file.id == ncxid)
                    {
                        NcxPath = file.path;
                        Instance.Logger($"NCX file found: {NcxPath}");
                        if (file.mimetype != "application/x-dtbncx+xml")
                        {
                            Instance.Logger($"NCX mimetype mismatch, fixing...");
                            file.mimetype = "application/x-dtbncx+xml";
                        }
                        string ncxContent = Instance.FileStorage.ReadString(NcxPath);
                        NcxDoc = new XmlDocument();
                        NcxDoc.LoadXml(ncxContent);
                        if (Instance.Config.GetBool("sanitizeNcx"))
                        {
                            Instance.Logger("Sanitizing NCX file...");
                            SanitizeNcx();
                        }
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Fix common errors in NCX file.
        /// Only call when NCX file is loaded, otherwise will crash due to null reference.
        /// </summary>
        private void SanitizeNcx()
        {
            // Fix uid element in NCX file
            var uid = Utils.NcxUtil.GetUidElement(NcxDoc);
            if (uid != null)
            {
                string ncxuid = uid.GetAttribute("content");
                string opfuid = Utils.OpfUtil.GetUniqueIdentifier(opfDoc);
                if (ncxuid != opfuid)
                {
                    Instance.Logger($"NCX UID '{ncxuid}' does not match OPF UID '{opfuid}', updating NCX UID.");
                    uid.SetAttribute("content", opfuid);
                }
            }
            //Write updated NCX file back to Epub.
            Instance.Logger("Updating NCX file...");
            Instance.FileStorage.WriteBytes(NcxPath, Utils.XmlUtil.ToXmlBytes(NcxDoc, false));
        }

        /// <summary>
        /// Write updated OPF file back to Epub.
        /// Calling this will result in isolated XmlElement in OpfFile object, suggest to only call once before saving Epub file.
        /// </summary>
        internal void UpdateOpf()
        {
            Instance.Logger("Updating OPF manifest...");
            // Remove all existing manifest entries
            XmlNode manifest = opfDoc.GetElementsByTagName("manifest")[0];
            while (manifest.HasChildNodes)
            {
                manifest.RemoveChild(manifest.FirstChild);
            }
            // Write new manifest entries
            foreach (OpfFile file in ManifestFiles)
            {
                if (file.originElement != null)
                {
                    // If the file already exists in the manifest, use the original element with updated attributes (id, href, and media-type).
                    file.originElement.SetAttribute("id", file.id);
                    file.originElement.SetAttribute("href", file.opfpath);
                    file.originElement.SetAttribute("media-type", file.mimetype);
                    if (file.properties != string.Empty)
                    {
                        file.originElement.SetAttribute("properties", file.properties);
                    }
                    manifest.AppendChild(file.originElement);
                    continue;
                }
                XmlElement newElement = opfDoc.CreateElement("item", "http://www.idpf.org/2007/opf");
                newElement.SetAttribute("id", file.id);
                newElement.SetAttribute("href", file.opfpath);
                newElement.SetAttribute("media-type", file.mimetype);
                if (file.properties != string.Empty)
                {
                    newElement.SetAttribute("properties", file.properties);
                }
                manifest.AppendChild(newElement);
            }
            // Save the updated OPF document back to the file system
            Instance.FileStorage.WriteBytes(OpfPath, Utils.XmlUtil.ToXmlBytes(opfDoc, false));
        }
    }
}
