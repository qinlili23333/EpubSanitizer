using EpubSanitizerCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices.JavaScript;
using System.Text;

foreach(var plugin in PluginManager.Plugins)
{
    // use reflection to get the class type
    Type pluginType = Type.GetType(plugin + ".PluginEntry, "+ plugin);
    if (pluginType == null)
    {
        Console.WriteLine($"Plugin not available for web: {plugin}"); 
        continue;
    }
    var pluginInstance = (PluginInterface?)Activator.CreateInstance(pluginType) ?? throw new Exception("Failed to create instance of plugin: " + plugin);
    pluginInstance.OnLoad(typeof(PluginManager).Assembly.GetName().Version);
    Console.WriteLine($"Loaded plugin: {plugin}");
}

static partial class EpubSanitizerWeb
{
    [JSExport]
    internal static string GetVersion() => $"WebSDK Version: {typeof(EpubSanitizerWeb).Assembly.GetName().Version}{Environment.NewLine}Core Version: {typeof(EpubSanitizer).Assembly.GetName().Version}";


    [JSExport]
    internal static byte[] SanitizeEpub(byte[] inputEpub)
    {
        Dictionary<string, string> config = new() { { "cache", "ram" } , { "filter", "all" } };
        using var ms = new MemoryStream(inputEpub);
        ZipArchive file = new(ms, ZipArchiveMode.Read);
        EpubSanitizer sanitizer = new();
        sanitizer.Logger = (msg) => Console.WriteLine(msg);
        sanitizer.Config.LoadConfigString(config);
        sanitizer.LoadFile(file);
        file.Dispose();
        sanitizer.Process();
        using var exportms = new MemoryStream();
        ZipArchive EpubFile = new(exportms, ZipArchiveMode.Create, true, Encoding.UTF8);
        sanitizer.SaveFile(EpubFile);
        EpubFile.Dispose();
        var outputEpub = exportms.ToArray();
        exportms.Close();
        return outputEpub;
    }
}



