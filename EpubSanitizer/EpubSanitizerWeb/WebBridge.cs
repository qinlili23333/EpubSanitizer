using EpubSanitizerCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices.JavaScript;
using System.Text;

Console.WriteLine("Hello, Browser!");

static partial class EpubSanitizerWeb
{
    [JSExport]
    internal static string GetVersion() => $"WebSDK Version: {typeof(EpubSanitizerWeb).Assembly.GetName().Version}{Environment.NewLine}Core Version: {typeof(EpubSanitizer).Assembly.GetName().Version}";


    [JSExport]
    internal static byte[] SanitizeEpub(byte[] inputEpub)
    {
        Dictionary<string, string> config = new() { { "cache", "ram" } };
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


