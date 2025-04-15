using System.Numerics;
using CytiaPrototype.Assets;
using CytiaPrototype.Graphics;
using CytiaPrototype.Levels;
using CytiaPrototype.Screens.Playfield;
using NanoVG;

namespace CytiaPrototype;

public class CytiaGame : IDisposable
{
    private NvgContext _nvgContext;
    private int viewW = 0, viewH = 0;

    private Font _systemFont;
    
    private PerfCounter _perfCounter = new ();
    private PlayAreaScreen? _playfieldScreen;
    
    public CytiaGame()
    {
        var nvgContextFlags = NvgCreateFlags.Antialias | NvgCreateFlags.StencilStrokes | NvgCreateFlags.Debug;
        var ctx = Nvg.CreateGLES3(nvgContextFlags) ?? throw new InvalidOperationException();

        if (ctx.Handle == IntPtr.Zero)
            throw new InvalidOperationException("Unable to initialise graphics");
        
        _nvgContext = ctx;
    }
    
    public void Dispose()
    {
        // TODO release managed resources here
        Nvg.DeleteGLES3(_nvgContext);
    }

    public void Init()
    {
        _systemFont = new Font(["assets","fonts","system"]);
        _perfCounter.Init(_systemFont);
    }

    public void OnStart()
    {
    }

    public void LoadChart(ChartBase chart)
    {
        var screen = new PlayAreaScreen();

        screen.UpdateViewSize(viewW, viewH);
        screen.UseChart(chart);
        
        _playfieldScreen = screen;
    }

    public void UpdateViewSize(int w, int h)
    {
        viewW = w;
        viewH = h;

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
        var ctx = _nvgContext;
        ctx.BeginFrame(viewW, viewH, 1.0f);

        _playfieldScreen?.Draw(ctx);

        _perfCounter.DrawUpdate(ctx, runTime, deltaTime);
        
        ctx.EndFrame();
    }

    public void PressLeft()
    {
        _playfieldScreen?.CurrentAttempt?.Seek(-3);
    }

    public void PressRight()
    {
        _playfieldScreen?.CurrentAttempt?.Seek(3);
    }

    public void NewAttempt()
    {
        _playfieldScreen?.Attempt();
    }
}