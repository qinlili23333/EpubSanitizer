# EpubSanitizer

EpubSanitizer is a .NET 9.0 solution that makes extracted EPUB files more compliant with standards. It provides a Core library, CLI application, and Web interface (Blazor WebAssembly) for sanitizing EPUB files.

Always reference these instructions first and fallback to search or bash commands only when you encounter unexpected information that does not match the info here.

## Working Effectively

- Install .NET 9.0 SDK (REQUIRED - system default is .NET 8.0):
  - `curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --version 9.0.101 --install-dir ~/.dotnet`
  - `export PATH="$HOME/.dotnet:$PATH"`
  - Verify: `dotnet --version` should show 9.0.101 or higher

- Bootstrap and build the repository:
  - `cd EpubSanitizer` (enter the inner EpubSanitizer directory)
  - `dotnet restore` -- takes 5 seconds. NEVER CANCEL. Set timeout to 30+ seconds.
  - `dotnet build` -- takes 9 seconds with some warnings and BuildAllPlugins failure (expected on Linux). NEVER CANCEL. Set timeout to 60+ seconds.

- Build individual components (faster and more reliable):
  - Core library: `dotnet build EpubSanitizerCore` -- takes 1.5 seconds. Set timeout to 30+ seconds.
  - CLI application: `dotnet build EpubSanitizerCLI` -- takes 1.5 seconds. Set timeout to 30+ seconds.
  - Web application: `dotnet build EpubSanitizerWeb` -- takes 2.3 seconds. Set timeout to 60+ seconds.

- Run the CLI application:
  - Help: `dotnet run --project EpubSanitizerCLI -- -h`
  - List filters: `dotnet run --project EpubSanitizerCLI -- -f`
  - Process EPUB: `dotnet run --project EpubSanitizerCLI -- input.epub output.epub`
  - With options: `dotnet run --project EpubSanitizerCLI -- --filter=default,privacy,epub3 input.epub output.epub`

- Run the Web application:
  - `dotnet run --project EpubSanitizerWeb`
  - Access at: http://localhost:5294/
  - HTTPS variant: https://localhost:7197/
  - The web interface shows "Current Version", "WebSDK Version", and "Core Version" with file upload functionality

## Cross-Platform Limitations

- **BuildAllPlugins project fails on Linux/macOS** due to Windows batch script in post-build event. This is expected and does not affect core functionality.
- Individual project builds work correctly on all platforms.
- Plugins can be built individually if needed: `dotnet build Plugins/DemoPlugin`, `dotnet build Plugins/CssPlugin`

## Validation

- ALWAYS manually validate CLI changes by processing a test EPUB file:
  1. Create a minimal test EPUB or use existing sample files
  2. Run: `dotnet run --project EpubSanitizerCLI -- input.epub output.epub`
  3. Verify the command succeeds and produces an output file
  4. Check that sanitization filters are applied correctly (default, privacy, epub3)

- ALWAYS manually validate Web changes:
  1. Run: `dotnet run --project EpubSanitizerWeb`
  2. Navigate to http://localhost:5294/
  3. Verify the interface loads with version information
  4. Test file upload functionality if modifying UI

- **Critical Validation Scenarios:**
  - CLI help display: `dotnet run --project EpubSanitizerCLI -- -h`
  - Filter listing: `dotnet run --project EpubSanitizerCLI -- -f`
  - Basic EPUB processing: Use a sample EPUB file and verify successful sanitization
  - Web interface loading: Verify version display and file upload controls

## Common Tasks

The following are outputs from frequently run commands. Reference them instead of viewing, searching, or running bash commands to save time.

### Repository Structure
```
EpubSanitizer/
├── EpubSanitizer.sln
├── EpubSanitizerCLI/          # Command-line interface (.NET 9.0 console app)
├── EpubSanitizerCore/         # Core library (.NET 9.0 library, published to NuGet)
├── EpubSanitizerWeb/          # Web interface (Blazor WebAssembly, .NET 9.0)
└── Plugins/                   # Plugin system
    ├── BuildAllPlugins/       # Fails on Linux due to Windows batch script
    ├── CssPlugin/            # CSS processing plugin
    └── DemoPlugin/           # Example plugin
```

### Available CLI Filters
```
default,general,epub3,vitalsource,privacy,
```

### CLI Options Summary
```
--filter=xxx              Filter for XHTML processing (default: 'default,privacy')
--compress=0              Compression level (CompressionLevel enum)
--cache=ram|disk          Cache storage location (default: 'ram')
--threads=single|multi    Thread processing (default: 'multi')
--overwrite               Overwrite existing files
--sanitizeNcx=true        Sanitize NCX file (default: enabled)
--epubVer=0               Target EPUB version (0=auto, 2, 3)
--correctMime=true        Correct MIME types (default: enabled)
--xmlCache=true           Cache XML parsing (default: enabled)
--enablePlugins           Enable plugin support (disabled by default)
```

### Key Project Files
- **EpubSanitizerCore**: Main sanitization logic, filters, file system abstraction
- **EpubSanitizerCLI**: Command-line entry point in `CliEntry.cs`
- **EpubSanitizerWeb**: Blazor WebAssembly interface
- **Solution Configuration**: Debug, ReleaseFree, ReleasePro (CLI has different behavior based on build config)

### Build Output Locations
- CLI: `EpubSanitizerCLI/bin/Debug/net9.0/`
- Core: `EpubSanitizerCore/bin/Debug/net9.0/`
- Web: `EpubSanitizerWeb/bin/Debug/net9.0/`

## Development Tips

- The project uses .NET 9.0 target framework exclusively
- No test infrastructure exists - manual validation required
- CLI has conditional compilation for Free vs Pro versions
- Plugins use InternalsVisibleTo for access to core internals
- Web version runs as client-side Blazor WebAssembly
- Build warnings about nullable references are expected and not errors
- Always use the inner `EpubSanitizer/` directory as working directory
- BuildAllPlugins failure is expected on non-Windows platforms

## Performance Notes

- Core library build: ~1.5 seconds
- CLI build: ~1.5 seconds  
- Web build: ~2.3 seconds
- Full solution build: ~9 seconds (with expected BuildAllPlugins failure)
- CLI processing: Typically <2 seconds for small EPUB files
- NEVER CANCEL builds or long-running commands - they complete quickly

## Required Dependencies

- .NET 9.0 SDK (must be manually installed)
- zip/unzip utilities (for EPUB testing)
- Standard Linux development tools (curl, bash)

Always ensure .NET 9.0 SDK is installed and in PATH before any development work.