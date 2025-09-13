# EpubSanitizer

Make extracted EPUB more comply with standard  

[![NuGet Version](https://img.shields.io/nuget/v/Qinlili.EpubSanitizer)](https://www.nuget.org/packages/Qinlili.EpubSanitizer)[![CodeQL](https://github.com/qinlili23333/EpubSanitizer/actions/workflows/github-code-scanning/codeql/badge.svg?branch=main)](https://github.com/qinlili23333/EpubSanitizer/actions/workflows/github-code-scanning/codeql)[![Codacy Badge](https://app.codacy.com/project/badge/Grade/84e2b3e6b75e4682b96f742dc5d10a4d)](https://app.codacy.com/gh/qinlili23333/EpubSanitizer/dashboard?utm_source=gh&utm_medium=referral&utm_content=&utm_campaign=Badge_grade)  
[See Roadmap](https://github.com/users/qinlili23333/projects/4/views/1)  

## Why this project?

I love Epub more than PDF, but I found due to the generation method difference most Epub files cannot pass EpubCheck, some even cannot work with some readers.  

So I made this project to fix the common problems in EPUB files, and make them more comply with standard.  

The standard I follow is the latest Epub 3 standard, which currently is 3.3.  

## Project structure

`EpubSanitizerCore` is the core library provides the sanitization logic. You can add it to your project and call it in your project.  

`EpubSanitizerCLI` is the command line tool to use the core library. It's more user friendly and can be used directly.  

## How to use CLI

`EpubSanitizerCLI -h` to see the help message.  

Since usage may varies across versions, please refer to the help message for the specific version you are using.  

## How to use Core

Add `EpubSanitizerCore` to your project, then you can use the `EpubSanitizer` class to sanitize EPUB files.  

You can refer to the `EpubSanitizerCLI` project for usage examples.  

## Web Version

EpubSanitizer now provides a web version that can be accessed at [EpubSanitizer Web](https://epubsanitizer.qinlili.bid).  

This version contains the same functionality as the desktop version, allowing users to sanitize EPUB files directly in their web browser, but without customization options.  

## License

This project is licensed under the AGPL-v3.0 License or [Qinlili Universal License](https://github.com/qinlili23333/QinliliUniversalLicense).  

You can choose either of them, or contact Qinlili to get an unlimited commercial license without any restriction.  

## CLI Version Compare

There is some little difference for Free and Pro version on CLI.  

| Features | Free Version | Pro Version |
| ------- | ----------- | ----------- |
| All filters for Epub with all options  | ✅ | ✅ |
| Process unlimited files  | ✅ | ✅ |
| Latest version available  | ✅ | ✅ |
| Community support  | ✅ | ✅ |
| Commercial usage  | ✅ | ✅ |
| Embed in your project  | ✅ | ✅ |
| ReadyToRun acceleration  | ❌ | ✅ |
| Random exit code  | ✅ | ❌ |
| Black both console text and background at night  | ✅ | ❌ |
| Support HarmonyOS NEXT  | ❌ | ❌ |

You can get Pro version CLI by any of these methods:  

- Become a sponsor on [GitHub](https://github.com/sponsors/qinlili23333)/[Buy Me a Coffee](https://www.buymeacoffee.com/qinlili)/[爱发电](https://afdian.com/a/qinliliAPP)  
- **Mutual follow** with Qinlili on any platform  
- Shopping on Amazon through Qinlili's [Affiliate Link](https://amzn.to/460zTkR)  
- Download from Ctfile for free (with ads on download page)  
- Submit your art to [QinliliGallery](https://github.com/qinlili23333/QinliliGallery) as illustrator and authorize Qinlili to use with [Qinlili Universal License](https://github.com/qinlili23333/QinliliUniversalLicense)  
- Write articles or make videos on social media to promote any project made by Qinlili  
- **Compile your own version from source code**  
- Crack from Free version (There will never be any anti-cracking protection, I'm DRM-Free enthusiast)  

**If you call core library in your project there is no version difference.**  

## Plugin Support

Since 1.4.0, EpubSanitizer now supports plugins for additional features. Plugins are used for filters with external dependencies, in order to keep the core library can be lightweight.  

Plugins are disabled by default and you need to enable them manually. This is designed for security concerns, as for maximum flexibility there is no restriction on what plugins can do. You should only use plugins when you know what you want to do and ensure the plugins come from trusted source.  

In CLI version, you can enable plugins by passing `--enablePlugins` to enable all supported plugins. In core library, you can call `PluginManager.EnablePlugin` to enable specific plugins or `PluginManager.EnablePlugins` to enable all supported plugins.  

Plugins should be put in the same folder of core library or CLI executable. Due to .NET limitation of InternalsVisibleTo, supported plugins are hardcoded in core library. So if you want to add your own plugin, you need to modify the core library source code, or use `DemoPlugin` name without modifying core library for testing purpose. You can refer to `DemoPlugin` project to see how to make a plugin.  

There is a project `BuildAllPlugins`, after modifying plugin source code, build this project will copy all built plugins to CLI output folder.  

## Copyright tips

This project will never add features that removing DRM or other copyright protection. We only focus on fixing problems in Epub files with no DRM.  

If you want to report an issue about specific Epub file, please ensure it's legal to share the file in public.  

## Contributing

Contributions are welcome! Feel free to open issues or pull requests.  

Please ensure your contributions comply with the project's license.  

## Acknowledgements

Thanks to these projects for their inspiration and support:  

- [EpubCheck](https://github.com/w3c/epubcheck)
- [MimeTypesMap](https://github.com/hey-red/MimeTypesMap)
- [AngleSharp.Css](https://github.com/AngleSharp/AngleSharp.Css)
