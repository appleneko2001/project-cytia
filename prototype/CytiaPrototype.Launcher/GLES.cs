using System.Runtime.InteropServices;

namespace CytiaPrototype.Launcher;

public static class GLES
{
    private const string Prefix = "gl";
    private const string LibName = "libGLESv2";
    
    [DllImport(LibName, EntryPoint = Prefix + nameof(Viewport))]
    public static extern void Viewport (int x, int y, int width, int height);
    
    [DllImport(LibName, EntryPoint = Prefix + nameof(ClearColor))]
    public static extern void ClearColor (float red, float green, float blue, float alpha);
    
    
    [DllImport(LibName, EntryPoint = Prefix + nameof(Clear))]
    public static extern void Clear (uint mask);
}