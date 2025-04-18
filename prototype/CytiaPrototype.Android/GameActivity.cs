using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Opengl;
using Android.OS;
using Android.Views;
using CytiaPrototype.Levels;
using CytiaPrototype.Levels.Cytoid;
using Java.Lang;
using NanoVG;
using Org.Libsdl.App;
using SDL;

using static SDL.SDL3;
using Exception = System.Exception;

namespace CytiaPrototype.AndroidApp;
//"@style/AppTheme.NoActionBar"
[Activity(ConfigurationChanges = DEFAULT_CONFIG_CHANGES, 
    Label = "@string/app_name",
    LaunchMode = LaunchMode.SingleInstance,
    Theme = "@style/AppTheme" ,
    MainLauncher = true)]
public class GameActivity : SDLActivity
{
    protected const ConfigChanges DEFAULT_CONFIG_CHANGES = ConfigChanges.Keyboard
                                                           | ConfigChanges.KeyboardHidden
                                                           | ConfigChanges.Navigation
                                                           | ConfigChanges.Orientation
                                                           | ConfigChanges.ScreenLayout
                                                           | ConfigChanges.ScreenSize
                                                           | ConfigChanges.SmallestScreenSize
                                                           | ConfigChanges.Touchscreen
                                                           | ConfigChanges.UiMode;
    
    private const int events_per_peep = 64;
    private readonly SDL_Event[] events = new SDL_Event[events_per_peep];
    
    protected override string[] GetLibraries() => ["SDL3"];
    
    private static bool _enableFramerateLock = true;
    private static int _targetFps = 100;
    private static float? _targetFrameDuration;
    
    private static float GetFrameDuration() => _targetFrameDuration ??= 1.0f / (_targetFps + 1);
    
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        System.Environment.CurrentDirectory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile);

        base.OnCreate(savedInstanceState);
    }

    protected override void Main()
    {
        try
        {
            /*Window.AddFlags(WindowManagerFlags.Fullscreen |
                            WindowManagerFlags.HardwareAccelerated |
                            WindowManagerFlags.TranslucentNavigation |
              *              WindowManagerFlags.TranslucentStatus);*/
            AppMainPrivate();
        }
        catch (Exception ee)
        {
            RunOnUiThread(() =>
            {
                using var builder = new AlertDialog.Builder(this) ?? throw new ArgumentNullException();
            
                builder
                    .SetCancelable(false)!
                    .SetTitle("Unable to start the game")!
                    .SetMessage(ee?.ToString())!
                    .SetIcon(Android.Resource.Drawable.IcDialogAlert)!
                    .SetNeutralButton("OK", Handler)!
                    .Show();

                void Handler(object? sender, DialogClickEventArgs e)
                {
                    throw ee;
                }
            });

            new ManualResetEventSlim().Wait();
        }
    }

    private unsafe void AppMainPrivate()
    {
        if (!SDL_Init(SDL_InitFlags.SDL_INIT_VIDEO | SDL_InitFlags.SDL_INIT_GAMEPAD))
        {
            throw new InvalidOperationException($"Failed to initialise SDL: {SDL_GetError()}");
        }

        
        int version = SDL_GetVersion();
        Console.WriteLine($@"SDL3 Initialized
                          SDL3 Version: {SDL_VERSIONNUM_MAJOR(version)}.{SDL_VERSIONNUM_MINOR(version)}.{SDL_VERSIONNUM_MICRO(version)}
                          SDL3 Revision: {SDL_GetRevision()}
                          SDL3 Video driver: {SDL_GetCurrentVideoDriver()}");
        SDL_GL_SetAttribute(SDL_GLAttr.SDL_GL_CONTEXT_PROFILE_MASK, (int)SDL_GLProfile.SDL_GL_CONTEXT_PROFILE_ES);

        // Minimum OpenGL version for ES profile:
        SDL_GL_SetAttribute(SDL_GLAttr.SDL_GL_CONTEXT_MAJOR_VERSION, 3);
        SDL_GL_SetAttribute(SDL_GLAttr.SDL_GL_CONTEXT_MINOR_VERSION, 0);
        
        SDL_WindowFlags flags = SDL_WindowFlags.SDL_WINDOW_RESIZABLE |
                                SDL_WindowFlags.SDL_WINDOW_HIGH_PIXEL_DENSITY |
                                SDL_WindowFlags.SDL_WINDOW_OPENGL;
        var window = SDL_CreateWindow("Project Cytia", 1366, 768, flags);

        var context = SDL_GL_CreateContext(window);
        if(!SDL_GL_MakeCurrent(window, context))
            throw new InvalidOperationException();
        
        var loader = new NvgLibraryLoader();
            
        JavaSystem.LoadLibrary("GLESv2");
            
        if (!Nvg.LoadLibrary(loader, $"NanoVG_GLES3"))
            throw new InvalidOperationException($"Unable to init graphics engine");
        
        var next = new Queue<ChartBase>();
        
        using var game = new CytiaGame();
        
        game.Init();
        
        double ft = 0, dt = 0, rDt = 0, prevRenderTime = 0, prevTime = 0;
        
        game.OnStart();

        var since = DateTime.UtcNow;

        double GetTime()
        {
            return (DateTime.UtcNow - since).TotalSeconds;
        }
        try
        {
            Console.WriteLine($"Loading cytoid json level");

            using var stream = Assets!.Open("maguro_sabakimasu_easy.txt");
            var inst = JsonSerializer.DeserializeAsync<CytoidLevelChartModel>(stream).Result;

            var level = new CytoidChart();
            level.LoadAsync(inst ?? throw new InvalidOperationException()).Wait();

            next.Enqueue(level);
        }
        catch(System.Exception e)
        {
            Console.WriteLine($"Unable to load level: {e}");
        }

        int w, h;

        game.Draw(ft, rDt);

        var dummy = new SDL_Event
        {
            type = (uint)SDL_EventType.SDL_EVENT_WINDOW_RESIZED
        };
        SDL_PushEvent(&dummy);
        
        var quit = false;
        while (!quit)
        {
            ft = GetTime();
            dt = ft - prevTime;
            prevTime = ft;

            SDL_Event @event;
            while (SDL_PollEvent(&@event))
            {
                switch (@event.Type)
                {
                    case SDL_EventType.SDL_EVENT_QUIT:
                        quit = true;
                        break;
                    
                    case SDL_EventType.SDL_EVENT_WINDOW_RESIZED:
                        SDL_GetWindowSize(window, &w, &h);
                        GLES.Viewport(0,0,w,h);
                        game.UpdateViewSize(w, h);
                        break;
                }
            }

            if (next.TryDequeue(out var nextChart)) 
                game.LoadChart(nextChart);
            
            rDt = ft - prevRenderTime;
            
            // Update with delta time here
            game.Update(ft, dt);
                
            if(_enableFramerateLock && GetFrameDuration() >= rDt)
                continue;
            
            GLES.Clear(GLES20.GlColorBufferBit | GLES20.GlDepthBufferBit);
            GLES.ClearColor(0f, 0f, 0f, 1f);
            
            game.Draw(ft, rDt);
            
            GLES.Flush();
            SDL_GL_SwapWindow(window);
            
            prevRenderTime = ft;
        }
    }
}