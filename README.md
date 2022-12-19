# CodeSniffer
Scans your source code repositories to perform configurable checks.
Very much a work in progress.


## Creating sniffer plugins
A dotnet template is provided for quickly generating the boilerplate for a CodeSniffer Sniffer plugin.

1. Install .NET 7 if required
2. Install or upgrade the template using ```dotnet new install CodeSniffer.Sniffer.Template```
3. Create a new folder. Convention is `CodeSniffer.Sniffer.MyPluginName`.
4. Open a terminal in the newly created folder and run ```dotnet new sniffer -p MyPluginName```
5. Modify the code where needed. If you are using Github, an Actions build script is provided which will generate the ZIP file as an artifact which can be uploaded to a CodeSniffer instance.


## Related repositories

**Sniffers**
[Grep](https://github.com/MvRens/CodeSniffer.Sniffer.Grep)
[.NET version](https://github.com/MvRens/CodeSniffer.Sniffer.DotNetVersion)
[NuGet package version](https://github.com/MvRens/CodeSniffer.Sniffer.NuGetPackageVersion)
[Vue i18n](https://github.com/MvRens/CodeSniffer.Sniffer.Vue18n)

**Libraries**
[CodeSniffer.SnifferLib](https://github.com/MvRens/CodeSniffer.SnifferLib)
