using System.Numerics;
using CytiaPrototype.Extensions;
using NanoVG;
using NanoVG.Extensions;

namespace CytiaPrototype.Screens.Playfield.Elements;

public class AttemptCounter
{
    private ulong _counter = 0;
    private double _time;
    private double _fadeOutShowTime = 5;
    private double _maxShowTime = 6;

    internal void Increase()
    {
        _counter++;
        
        // First attempt can be ignored
        if (_counter <= 1)
        {
            _time = _maxShowTime;
            return;
        }

        _time = 0;
    }

    public void Draw(NvgContext ctx)
    {
        var visibility = (float)_time.LinearFadeEdge(0, 0, _fadeOutShowTime, _maxShowTime);

        if (visibility <= 0.0)
            return;
        
        ctx.GlobalAlpha(visibility);
        
        ctx.FillColor(Vector4.One);
        ctx.FontFaceId(0);
        ctx.FontSize(16);
        ctx.Text(0, 48, $"Attempt: {_counter}");
    }

    public void Update(double deltaTime)
    {
        if(_time > _maxShowTime)
            return;

        _time += deltaTime;
    }
}