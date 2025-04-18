using System.Drawing;
using System.Numerics;
using CytiaPrototype.Assets;
using CytiaPrototype.Extensions;
using NanoVG;
using NanoVG.Extensions;

namespace CytiaPrototype;

public class PerfCounter : UIElementBase
{
    private string _updateCounter = "", _renderCounter = "";
    private Font? _font;
    private float[] _textBounds = new float[4];
    
    public void Init(Font font)
    {
        _font = font;
    }

    public void Update(double runTime, double deltaTime)
    {
        //_updateCounter = $"Update {deltaTime * 1000:F2}ms ({(int)(1.0 / deltaTime)} Hz)";
    }

    public void DrawUpdate(NvgContext ctx, double runTime, double deltaTime)
    {
        _renderCounter = $"Render {deltaTime * 1000:F2} ms";

        var font = _font;
        
        if(font == null)
            return;

        var v = ViewSize;
        
        ctx.FontSize(16.0f);
        font.Use(ctx);
        ctx.TextAlign(NvgAlign.Right | NvgAlign.Bottom);
        
        ctx.TextBounds(-4, -4, _renderCounter, _textBounds);
        
        ctx.BeginPath();
        ctx.RoundedRect(
            v.X + _textBounds[0], 
            v.Y + _textBounds[1],
            _textBounds[2] - _textBounds[0], 
            _textBounds[3] - _textBounds[1], 4.0f);
        ctx.FillColor(Color.DarkOrange.ToVec4());
        ctx.Fill();
        
        ctx.FillColor(Color.Black.ToVec4());
        ctx.FontBlur(1);
        ctx.Text(v.X-4, v.Y-2, _renderCounter);
        
        ctx.FillColor(Vector4.One);
        ctx.FontBlur(0);
        ctx.Text(v.X-4, v.Y-2, _renderCounter);
    }

    protected override void UpdateViewSizePrivate(float w, float h)
    {
    }
}