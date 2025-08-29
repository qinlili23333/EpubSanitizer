// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

console.log = text => {
    const consoleOutput = document.getElementById("console-output");
    consoleOutput.innerText += text + "\n";
    consoleOutput.scrollTop = consoleOutput.scrollHeight; 
}
import { dotnet } from './_framework/dotnet.js'

const { setModuleImports, getAssemblyExports, getConfig, runMain } = await dotnet
    .withApplicationArguments("start")
    .create();

setModuleImports('main.js', {
    dom: {
        setInnerText: (selector, time) => document.querySelector(selector).innerText = time
    }
});

const config = getConfig();
const exports = await getAssemblyExports(config.mainAssemblyName);
document.getElementById("version").innerText = exports.EpubSanitizerWeb.GetVersion();
document.getElementById("fileInput").addEventListener("change", async (event) => {
    const file = event.target.files[0];
    if (file) {
        document.getElementById("console-output").innerText = "";
        document.getElementById("status").innerText = "Processing...";
        const arrayBuffer = await file.arrayBuffer();
        const uint8Array = new Uint8Array(arrayBuffer);
        try {
            const result = await exports.EpubSanitizerWeb.SanitizeEpub(uint8Array);
            const blob = new Blob([result], { type: 'application/epub+zip' });
            const url = URL.createObjectURL(blob);
            //download directly without link
            const a = document.createElement('a');
            a.href = url;
            a.download = `sanitized_${file.name}`;
            document.body.appendChild(a);
            a.click();
            document.body.removeChild(a);
            URL.revokeObjectURL(url);
            document.getElementById("status").innerText = "Processing completed. File downloaded.";
        } catch (error) {
            document.getElementById("status").innerText = `Error: ${error.message}`;
        }
    }
})

// run the C# Main() method and keep the runtime process running and executing further API calls
await runMain();