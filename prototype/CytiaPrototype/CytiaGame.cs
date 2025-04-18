using System.Numerics;
using CytiaPrototype.Assets;
using CytiaPrototype.Levels;
using CytiaPrototype.Screens.Playfield;
using NanoVG;

namespace CytiaPrototype;

public class CytiaGame : UIElementBase, IDisposable
{
    private NvgContext? _nvgContext;

    private Font _systemFont;
    
    private PerfCounter _perfCounter = new ();
    private PlayAreaScreen? _playfieldScreen;

    private GameAssets _embeddedAssets = new();
    
    private NvgContext DrawingContext => _nvgContext ??= NvgInitPrivate();

    private NvgContext NvgInitPrivate()
    {
        var nvgContextFlags = NvgCreateFlags.Antialias |
                              NvgCreateFlags.StencilStrokes
                              |
                              NvgCreateFlags.Debug
            ;
        var ctx = Nvg.CreateGLES3(nvgContextFlags) ?? throw new InvalidOperationException();

        if (ctx.Handle == IntPtr.Zero)
            throw new InvalidOperationException("Unable to initialise graphics");
        
        return ctx;
    }

    public void Dispose()
    {
        // TODO release managed resources here
        Nvg.DeleteGLES3(DrawingContext);
    }

    public void Init()
    {
        Console.WriteLine(DrawingContext);
        
        _systemFont = new Font(_embeddedAssets, ["assets","fonts","system"]);
        _perfCounter.Init(_systemFont);
    }

    public void OnStart()
    {
    }

    public void LoadChart(ChartBase chart)
    {
        var screen = new PlayAreaScreen();

        screen.UpdateViewSize(ViewSize);
        screen.UseChart(chart);
        
        _playfieldScreen = screen;
    }

    protected override void UpdateViewSizePrivate(float w, float h)
    {
        _playfieldScreen?.UpdateViewSize(w, h);

        _perfCounter.UpdateViewSize(w, h);
    }

    public void Update(double runTime, double deltaTime)
    {
        _playfieldScreen?.Update(runTime, deltaTime);
        
        _perfCounter.Update(runTime, deltaTime);
    }

    public void Draw(double runTime, double deltaTime)
    {
        var ctx = DrawingContext;
        ctx.BeginFrame(ViewSize.X, ViewSize.Y, 2.0f);

        _playfieldScreen?.Draw(ctx);

        _perfCounter.DrawUpdate(ctx, runTime, deltaTime);
        
        ctx.EndFrame();
    }

    public void PressLeft()
    {
        //_playfieldScreen?.CurrentAttempt?.Seek(-3);
    }

    public void PressRight()
    {
        //_playfieldScreen?.CurrentAttempt?.Seek(3);
    }

    public void NewAttempt()
    {
        _playfieldScreen?.Attempt();
    }
}