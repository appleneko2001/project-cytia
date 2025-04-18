using System;
using Android.Opengl;
using CytiaPrototype.Systems;
using Java.Lang;
using Javax.Microedition.Khronos.Opengles;
using NanoVG;
using EGLConfig = Javax.Microedition.Khronos.Egl.EGLConfig;

namespace CytiaPrototype.AndroidApp;

public class GameViewRenderer : Java.Lang.Object, GLSurfaceView.IRenderer
{
    private CytiaGame? _instance;
    private ChronoTimer _chronoTimer;
    private bool _hadInitNvg;

    private int viewWidth, viewHeight;

    public GameViewRenderer()
    {
        _chronoTimer = new ChronoTimer();
    }

    public void Dispose()
    {
        // TODO release managed resources here
        _instance?.Dispose();
    }

    private void InitNvg()
    {
        var loader = new NvgLibraryLoader();
            
        JavaSystem.LoadLibrary("GLESv2");
            
        if (!Nvg.LoadLibrary(loader, $"NanoVG_GLES3"))
            throw new InvalidOperationException($"Unable to init graphics engine");
    }

    public void OnDrawFrame(IGL10? gl)
    {
        if (!_hadInitNvg)
        {
            _hadInitNvg = true;
            InitNvg();
        }
        
        _chronoTimer.Collect(out var time, out var deltaTime);
        
        GLES20.GlClear(GLES20.GlColorBufferBit | GLES20.GlDepthBufferBit);
        GLES20.GlClearColor(0.5f, 0.5f, MathF.Sin((float)time) * 0.25f + 0.5f, 1f);

        var inst = _instance;

        if (inst == null)
        {
            _instance = _createInstance();
            _instance?.UpdateViewSize(viewWidth, viewHeight);
        }
        
        _instance?.Draw(time, deltaTime);
    }

    public void OnSurfaceChanged(IGL10? _, int width, int height)
    {
        viewWidth = width;
        viewHeight = height;
        
        GLES.Viewport(0, 0, width, height);
        _instance?.UpdateViewSize(width, height);
    }

    public void OnSurfaceCreated(IGL10? _, EGLConfig? config)
    {

    }

    private Func<CytiaGame> _createInstance;
    public void SetConstructor(Func<CytiaGame> callback)
    {
        _createInstance = callback;
    }
}