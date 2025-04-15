using System.Drawing;
using System.Numerics;
using CytiaPrototype.Extensions;
using CytiaPrototype.Graphics;
using NanoVG;
using NanoVG.Extensions;

namespace CytiaPrototype.Screens.Playfield.Elements;

public class ChartProgress : UIElementBase
{
    public double Height { get; set; }

    private double _now, _duration;

    public void Update(double time)
    {
        _now = time;
    }

    public void SetDuration(double duration)
    {
        _duration = duration;
    }

    public void Draw(NvgContext ctx)
    {
        var vSize = ViewSize;
        
        ctx.BeginPath();
        ctx.FillColor(Color.Aqua.ToVec4());
        ctx.Rect(0,0, (float)(vSize.X * (_now / _duration)), (float)Height);
        ctx.Fill();
        
        if(_now < 0 || _now > _duration)
            return;

        using var _ = ctx.PushPostTransform(Matrix3x2.CreateTranslation(0, (float)Height));

        ctx.FontFaceId(0);
        ctx.FontSize(16);
        ctx.TextAlign(NvgAlign.Left | NvgAlign.Top);
        ctx.Text(0, 0, $@"{TimeSpan.FromSeconds(_now):hh\:mm\:ss\.ff}");
        
        ctx.Text(0, 16, $@"{TimeSpan.FromSeconds(_duration):hh\:mm\:ss\.ff}");
    }
}