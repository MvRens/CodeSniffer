Creating plugins
================

TODO: describe how to create a plugin

Note: add the following settings to your plugin's csproj in the first PropertyGroup.
The BaseOutputPath and AppendTargetFrameworkToOutputPath enables directly building to the folder where the plugin is loaded from by CodeSniffer for easy debugging.
EnableDynamicLoading ensures dependencies are included in the output as well.

::xml

    <BaseOutputPath>$(APPDATA)\CodeSniffer\Plugins\PluginFolderName\</BaseOutputPath>
    <AppendTargetFrameworkToOutputPath>false</AppendTargetFrameworkToOutputPath>
    <EnableDynamicLoading>true</EnableDynamicLoading>
    

TODO: document how to mark the NuGet reference to CodeSniffer.Core so that it's DLL is not included in the output, that would prevent loading.