#pragma warning disable SYSLIB1054
#pragma warning disable CA2101
// ReSharper disable StringLiteralTypo

using System;
using System.Runtime.InteropServices;
using Java.Lang;
using Java.Util.Logging;

namespace CytiaPrototype.AndroidApp;

public class NvgLibraryLoader : NanoVG.ILibraryLoader
{
    [DllImport("dl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "dlopen")]
    private static extern IntPtr Open(string path, int flag);

    [DllImport("dl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "dlerror")]
    private static extern string GetError();
    
    [DllImport("dl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "dlsym")]
    private static extern IntPtr GetSymbol(IntPtr lib, string name);

    [DllImport("dl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "dlclose")]
    private static extern int Close(IntPtr lib);
    
    private readonly Logger _logger = Logger.Global;

    public IntPtr? Load(string libName)
    {
        JavaSystem.LoadLibrary(libName);
        var ptr = Open($"lib{libName}.so", 0x00002);

        if (ptr == IntPtr.Zero)
            _logger.Severe($"Unable to load library: {GetError()}");
        
        return ptr == IntPtr.Zero ? null : ptr;
    }

    public IntPtr? GetFunction(IntPtr pLib, string procName)
    {
        var ptr = GetSymbol(pLib, procName);

        if(ptr == IntPtr.Zero)
            _logger.Warning($"Unable to load symbol: {procName}");
        
        return ptr;
    }

    public void Free(IntPtr pLib)
    {
        Console.WriteLine(Close(pLib));
    }
}

#pragma warning restore CA2101
#pragma warning restore SYSLIB1054