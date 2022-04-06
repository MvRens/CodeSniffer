using System;
using System.Reflection;
using System.Runtime.Loader;

namespace CodeSniffer.Plugins
{
    public class PluginLoadContext : AssemblyLoadContext
    {
        private readonly AssemblyDependencyResolver resolver;


        public PluginLoadContext(string pluginAssemblyPath) : base("CsPlugins", true)
        {
            resolver = new AssemblyDependencyResolver(pluginAssemblyPath);
        }


        protected override Assembly? Load(AssemblyName assemblyName)
        {
            var assemblyPath = resolver.ResolveAssemblyToPath(assemblyName);
            return assemblyPath != null 
                ? LoadFromAssemblyPath(assemblyPath) 
                : null;
        }


        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            var libraryPath = resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
            return libraryPath != null 
                ? LoadUnmanagedDllFromPath(libraryPath) 
                : IntPtr.Zero;
        }
    }
}
