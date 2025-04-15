using System.Numerics;
using System.Runtime.InteropServices;
using NanoVG;

namespace CytiaPrototype.Graphics;

public static class NvgBindingsExtensions
{
    private const string PrivatePatchFixesImplementation = "This entry uses NanoVG API that have applied my personal patch, which is not released yet and their usage for debugging. Please consider not to use it or use alternative API."; 
    private const string LibName = "libNanoVG_GLES3";
    private const string Prefix = "nvg";
    
    [Obsolete(PrivatePatchFixesImplementation)]
    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = Prefix + nameof(GetStateStackLevel))]
    public static extern int GetStateStackLevel(this NvgContext ctx);
    
    [Obsolete(PrivatePatchFixesImplementation)]
    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = Prefix + nameof(GetTransform))]
    public static extern void GetTransform(this NvgContext ctx, out Matrix3x2 mat);
    
    [Obsolete(PrivatePatchFixesImplementation)]
    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = Prefix + nameof(GetTransform))]
    public static extern void GetTransform(this NvgContext ctx, float[] mat);

    public static void Transform(this NvgContext ctx, Matrix3x2 mat) => 
        ctx.Transform(mat.M11, mat.M12, mat.M21, mat.M22, mat.M31, mat.M32);
}