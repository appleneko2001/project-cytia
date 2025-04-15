using System.Runtime.InteropServices;
using NanoVG;

namespace CytiaPrototype.Launcher;

internal class Win32NvgLibraryLoader : ILibraryLoader
{
#pragma warning disable CA2101
#pragma warning disable SYSLIB1054
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern IntPtr LoadLibrary(string lpFileName);
    
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool FreeLibrary(IntPtr hModule);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);
#pragma warning restore SYSLIB1054
#pragma warning restore CA2101
    
    public IntPtr? Load(string libName)
    {
        return LoadLibrary(libName);
    }

    public IntPtr? GetFunction(IntPtr pLib, string procName)
    {
        return GetProcAddress(pLib, procName);
    }

    public void Free(IntPtr pLib)
    {
        if(!FreeLibrary(pLib))
            Console.WriteLine("Did not finalise native library!");
    }
}