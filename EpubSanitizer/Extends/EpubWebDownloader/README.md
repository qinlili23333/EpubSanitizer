# Epub Web Downloader Extend

## Introduction

This extend helps you to download extracted Epub hosted on the web completely automatically.  

You must use it with the core library.  

## Usage

1. Create new instance of EpubSanitizer  

```
EpubSanitizer Instance = new();
```

2. Create ProxyFS

```
EpubWebDownloader.SetupProxyFS(Instance, "http://base_url_of_epub/");
```

3. Just process as normal

```
Instance.Process();
```