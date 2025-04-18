using System.Runtime.InteropServices;
using System.Text.Json;
using CytiaPrototype.Levels;
using CytiaPrototype.Levels.Cytoid;
using GLFW;
using NanoVG;
using Exception = System.Exception;

namespace CytiaPrototype.Launcher;

class Program
{
    private static bool _enableFramerateLock = true;
    private static int _targetFps = 100;
    private static float? _targetFrameDuration;

    private static CytiaGame? _instance;
    
    private static float GetFrameDuration() => _targetFrameDuration ??= 1.0f / (_targetFps + 1);
    
    [DllImport("glfw", EntryPoint = "glfwWindowHint", CallingConvention = CallingConvention.Cdecl)]
    public static extern void WindowHint(int hint, int value);
    
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        WindowHint(0x00050002, 0x00037002);
        
        Glfw.WindowHint(Hint.ContextCreationApi, ContextApi.Egl);
        Glfw.WindowHint(Hint.ClientApi, ClientApi.OpenGLES);
        Glfw.WindowHint(Hint.ContextVersionMajor, 3);
        Glfw.WindowHint(Hint.ContextVersionMinor, 1);

        if (!Glfw.Init())
            throw new Exception();

        if (!Nvg.LoadLibrary(new Win32NvgLibraryLoader(), $"{Nvg.LibNamePrefix}_GLES3"))
            throw new InvalidOperationException();
        
        using var window = new NativeWindow();
        
        window.MakeCurrent();
        using var game = new CytiaGame();

        Glfw.SwapInterval(1);
        Glfw.Time = 0;
        var prevTime = Glfw.Time;
        
        int wW = 0, wH = 0;
        int fbW = 0, fbH = 0;

        double ft = 0, dt = 0, rDt = 0, prevRenderTime = 0;
        
        window.FramebufferSizeChanged += WindowOnFramebufferSizeChanged;
        window.KeyPress += WindowOnKeyPress;

        var next = new Queue<ChartBase>();

        _instance = game;
        
        /*next.Enqueue(new MockChart
        {
            
        });*/
        
        game.Init();
        
        Glfw.GetFramebufferSize(window, out fbW, out fbH);
        WindowOnFramebufferSizeChanged(window, new SizeChangeEventArgs(fbW, fbH));

        game.OnStart();

        LoadAsync(next);
        
        while (!window.IsClosed)
        {
            ft = Glfw.Time;
            dt = ft - prevTime;
            prevTime = ft;
            
            Glfw.PollEvents();

            if (next.TryDequeue(out var nextChart)) 
                game.LoadChart(nextChart);
            
            rDt = ft - prevRenderTime;
            
            // Update with delta time here
            game.Update(ft, dt);
                
            if(_enableFramerateLock && GetFrameDuration() >= rDt)
                continue;
            
            //Glfw.GetCursorPos(window, ref mx, ref my);
            //Glfw.GetWindowSize(window, out wW, out wH);
            //Glfw.GetFramebufferSize(window, out fbW, out fbH);
            
            //var pxRatio = (float)fbW / wW;
            GLES.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            GLES.Clear(0x00000100 | 0x00000400 | 0x00004000);
            
            game.Draw(ft, rDt);
            
            if(!window.IsClosed)
                window.SwapBuffers();
            
            prevRenderTime = ft;
        }
    }

    private static void WindowOnKeyPress(object? sender, KeyEventArgs e)
    {
        if(_instance is not CytiaGame game)
            return;
        
        switch (e.Key)
        {
            case Keys.Left:
                game.PressLeft();
                break;
            
            case Keys.Right:
                game.PressRight();
                break;
            
            case Keys.R:
                game.NewAttempt();
                break;
        }
    }

    private static async void LoadAsync(Queue<ChartBase> queue)
    {
        var model = await LoadChartLevelAsync();
        var chart = new CytoidChart();
        await chart.LoadAsync(model);
        queue.Enqueue(chart);
    } 

    private static async Task<CytoidLevelChartModel> LoadChartLevelAsync()
    {
        var fileName = "maguro_sabakimasu_easy.txt";
        
        Console.WriteLine($"Loading cytoid json level: {fileName}");

        var stream = File.OpenRead(fileName);
        var inst = await JsonSerializer.DeserializeAsync<CytoidLevelChartModel>(stream);
        
        return inst ?? throw new InvalidOperationException();
    }

    private static void WindowOnFramebufferSizeChanged(object? sender, SizeChangeEventArgs e)
    {
        var fb = e.Size;
        GLES.Viewport(0, 0, fb.Width, fb.Height);
        _instance?.UpdateViewSize(fb.Width, fb.Height);
    }
}