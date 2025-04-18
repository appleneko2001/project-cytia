using System;
using Android.Content;
using Android.Opengl;
using Javax.Microedition.Khronos.Egl;
using EGLConfig = Javax.Microedition.Khronos.Egl.EGLConfig;
using EGLContext = Javax.Microedition.Khronos.Egl.EGLContext;
using EGLDisplay = Javax.Microedition.Khronos.Egl.EGLDisplay;
using static Android.Opengl.EGL15;
using static Android.Opengl.EGL14;

namespace CytiaPrototype.AndroidApp;

public class GameSurfaceView : GLSurfaceView
{
    private class Factory : Java.Lang.Object, IEGLContextFactory
    {
        private static int EGL_CONTEXT_CLIENT_VERSION = 0x3098;
        
        public EGLContext? CreateContext(IEGL10? egl, EGLDisplay? display, EGLConfig? eglConfig)
        {
            ArgumentNullException.ThrowIfNull(display);
            ArgumentNullException.ThrowIfNull(eglConfig);
            ArgumentNullException.ThrowIfNull(egl);
            
            var contextAttribs = new[]
            {
                EglContextMajorVersion, 3,
                EglContextMinorVersion, 1,
                EglNone
            };

            var context = egl.EglCreateContext(display, eglConfig, IEGL10.EglNoContext, contextAttribs);
            if (context == null)
                throw new InvalidOperationException($"Unable to create context: {GetErrorCode(egl)}");

            return context;
        }
        
        public void DestroyContext(IEGL10? egl, EGLDisplay? display, EGLContext? context)
        {
            egl?.EglDestroyContext(display,context);
        }
        
        private string GetErrorCode(IEGL10 egl) => $"{egl.EglGetError():X}";
    }
    
    private readonly IRenderer _renderer;

    public GameSurfaceView(Context? context, Func<IRenderer> factory) : base(context)
    {
        base.SetEGLContextClientVersion(3);
        base.SetEGLContextFactory(new Factory());
        //base.SetEGLConfigChooser(8, 8, 8, 0, 0, 8);

        _renderer = factory();// new MockGameViewRenderer();
        base.SetRenderer(_renderer);
    }
}