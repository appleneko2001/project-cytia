using System.Numerics;
using CytiaPrototype.Graphics;
using NanoVG;

namespace CytiaPrototype.Screens.Playfield.Elements;

public class ScrollCellsLine : UIElementBase
{
    public float ScaleUpAmount = 1.5f;
    public float Thickness = 16;
    public bool IsUpperLine = false;
    private float _offset;
    private float _scaleY;
    
    const float lineSpacing = 120f;

    private double time;

    public void Update(double runTime, double deltaTime)
    {
        if (time > lineSpacing)
            time -= lineSpacing;
        
        time += deltaTime * lineSpacing;

        _offset = (float)time;

        _scaleY -= (float)(deltaTime * 10f);
        _scaleY = Math.Max(1, _scaleY);
    }

    public void ScaleUp()
    {
        _scaleY = ScaleUpAmount;
    }

    public void Draw(NvgContext ctx)
    {
        var vSize = ViewSize;
        var halfH = (Thickness * _scaleY) * 0.5f;
        var rX = halfH * 2f;


        const float cellWidth = 4f;

        var offset = _offset;
        
        ctx.FillColor(Vector4.One);

        var y = 0;
        
        void DrawCell(float x)
        {
            using var _ = ctx.PushPostTransform(Matrix3x2.CreateTranslation(x, y));
            ctx.BeginPath();
            ctx.Ellipse(0, 0, rX, rX);
            //ctx.Rect(x, y, cellWidth, halfH * 2f);
            ctx.Fill();
        }

        if (IsUpperLine)
        {
            // Upper vertical scroll cells that goes to right direction
            for (var x = vSize.X; ; x -= lineSpacing)
            {
                var finalX = x + offset % lineSpacing;
                if(finalX <= -rX)
                    break;
            
                if (finalX >= vSize.X + rX)
                    continue;
            
                DrawCell(finalX);
            }
        }
        else
        {
            // Lower vertical scroll cells that goes to left direction
            for (var x = 0f; ; x += lineSpacing)
            {
                var finalX = x - offset % lineSpacing;
                if(finalX >= vSize.X + rX)
                    break;
            
                if (finalX <= -rX)
                    continue;
            
                DrawCell(finalX);
            }
        }
    }
}