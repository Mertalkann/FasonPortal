using System;
using System.Reflection;
using System.Runtime.Loader;

namespace FasonPortal.Helpers;
public class CustomAssemblyLoadContext : AssemblyLoadContext
{
    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    {
        string basePath = AppContext.BaseDirectory;
        string dllPath = Path.Combine(basePath, "wwwroot", "DinkToPdf", unmanagedDllName);

        if (File.Exists(dllPath))
        {
            return LoadUnmanagedDllFromPath(dllPath);
        }

        return IntPtr.Zero;
    }

    protected override Assembly Load(AssemblyName assemblyName)
    {
        return null;
    }
}
