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

## License
This project is licensed under the AGPL-v3.0 License or [Qinlili Universal License](https://github.com/qinlili23333/QinliliUniversalLicense).  

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

