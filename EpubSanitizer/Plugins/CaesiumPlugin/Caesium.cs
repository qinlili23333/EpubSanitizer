using EpubSanitizerCore.Filters;
using System.Runtime.InteropServices;

namespace EpubSanitizerCore.Plugins.CaesiumPlugin
{
    internal enum SupportedFileTypes
    {
        Jpeg,
        Png,
        Gif,
        WebP,
        Tiff,
        Unkn,
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct CCSResult
    {
        [MarshalAs(UnmanagedType.I1)] // bool is 1 byte in C
        public bool success;
        public uint code;

        public IntPtr error_message;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct CCSParameters
    {
        [MarshalAs(UnmanagedType.I1)] public bool keep_metadata;
        public uint jpeg_quality;
        public uint jpeg_chroma_subsampling;
        [MarshalAs(UnmanagedType.I1)] public bool jpeg_progressive;
        [MarshalAs(UnmanagedType.I1)] public bool jpeg_optimize;
        public uint png_quality;
        public uint png_optimization_level;
        [MarshalAs(UnmanagedType.I1)] public bool png_force_zopfli;
        [MarshalAs(UnmanagedType.I1)] public bool png_optimize;
        public uint gif_quality;
        public uint webp_quality;
        [MarshalAs(UnmanagedType.I1)] public bool webp_lossless;
        public uint tiff_compression;
        public uint tiff_deflate_level;
        public uint width;
        public uint height;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct CByteArray
    {
        public IntPtr data;     // uint8_t*
        public UIntPtr length;  // uintptr_t
    }
    internal static partial class NativeMethods
    {
        private const string DllName = "caesium.dll";

        [LibraryImport(DllName)]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        internal static partial CCSResult c_compress_in_memory(
            [In] byte[] input_data,
            UIntPtr input_length,
            CCSParameters parameters,
            out CByteArray output);

        [LibraryImport(DllName)]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        internal static partial void c_free_byte_array(CByteArray array);

        [LibraryImport(DllName)]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        internal static partial void c_free_string(IntPtr ptr);
    }

    internal class Caesium(EpubSanitizer CoreInstance) : MultiThreadFilter(CoreInstance)
    {
        static readonly Dictionary<string, object> ConfigList = new() {
            {"caesium.lossless", true},
            {"caesium.quality", 99}
        };
        static Caesium()
        {
            ConfigManager.AddDefaultConfig(ConfigList);
        }
        internal override string[] GetProcessList()
        {
            string[] supportedMimeTypes =
            [
                "image/jpeg",
                "image/png",
                "image/gif",
                "image/webp",
                "image/tiff"
            ];
            string[] files = [];
            foreach (var file in Instance.Indexer.ManifestFiles)
            {
                if (supportedMimeTypes.Contains(file.mimetype))
                {
                    files = [.. files, file.path];
                }
            }
            return files;
        }

        internal override void Process(string file)
        {
            byte[] inputData = Instance.FileStorage.ReadBytes(file);
            CCSParameters parameters = new()
            {
                keep_metadata = true,
                jpeg_quality = (uint)Instance.Config.GetInt("caesium.quality"),
                jpeg_chroma_subsampling = 0,
                jpeg_progressive = false,
                jpeg_optimize = Instance.Config.GetBool("caesium.lossless"),
                png_quality = (uint)Instance.Config.GetInt("caesium.quality"),
                png_optimization_level = 6,
                png_force_zopfli = true,
                png_optimize = Instance.Config.GetBool("caesium.lossless"),
                gif_quality = (uint)Instance.Config.GetInt("caesium.quality"),
                webp_quality = (uint)Instance.Config.GetInt("caesium.quality"),
                webp_lossless = Instance.Config.GetBool("caesium.lossless"),
                tiff_compression = 1,
                tiff_deflate_level = 6,
                width = 0,
                height = 0
            };
            CByteArray output;
            CCSResult result = NativeMethods.c_compress_in_memory(
                inputData,
                (UIntPtr)inputData.Length,
                parameters,
                out output);

            if (!result.success)
            {
                string error = Marshal.PtrToStringAnsi(result.error_message) ?? "Unknown error";
                if (result.error_message != IntPtr.Zero)
                {
                    NativeMethods.c_free_string(result.error_message); // free error string
                }
                throw new FormatException($"Caesium compression failed: {error}");
            }
            byte[] compressedData = new byte[(int)output.length];
            Marshal.Copy(output.data, compressedData, 0, compressedData.Length);
            NativeMethods.c_free_byte_array(output);
            if(compressedData.Length >= inputData.Length)
            {
                // No size reduction, skip
                Instance.Logger($"Caesium compression did not reduce size for file {file}, skipping.");
                return;
            }
            Instance.FileStorage.WriteBytes(file, compressedData);
        }
        public static new void PrintHelp()
        {
            Console.WriteLine("Filter applied to image files. Compression by libcaesium");
            Console.WriteLine("Options:");
            Console.WriteLine("    --caesium.lossless=true    Whether to perform loseless compression, default is true.");
            Console.WriteLine("    --caesium.quality=99       Quality when loseless is disabled. Default is 99.");
        }
    }
}
