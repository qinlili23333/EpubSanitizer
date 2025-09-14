@echo off
title EpubSanitizer Batch Process
rem Change path as your condition
set EpubSanitizer=C:\DevEnv\EpubSanitizer\EpubSanitizer\EpubSanitizerCLI\bin\Debug\net9.0\EpubSanitizerCLI.exe
set EpubCheck=C:\Users\QINLILI\Downloads\epubcheck-5.2.1\epubcheck.jar
if "%1"=="" (
	echo No path given, exit.
	exit /b -1
)
if not exist %1 (
	echo Path not exist, exit.
	exit /b -1
)
mkdir "%1\out"
for %%f in ("%1\*.epub") do (
    echo Processing %%~nxf
    %EpubSanitizer% --filter=general,epub3,privacy,css --epubVer=3 --cache=ram --compress=3 --enablePlugins "%%f" "%1\out\%%~nxf"
)
for %%f in ("%1\out\*.epub") do (
    echo Validating %%f
    java -jar %EpubCheck% "%%f"
)