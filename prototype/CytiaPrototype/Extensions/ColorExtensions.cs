using System.Drawing;
using System.Numerics;
using NanoVG;

namespace CytiaPrototype.Extensions;

public static class ColorExtensions
{
    public static Vector4 ToVec4(this Color c) => Nvg.RGBA(c.R, c.G, c.B, c.A);
}