using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Android.App;
using Android.OS;
using CytiaPrototype.Systems;
using Java.Lang;
using NanoVG;
using Org.Libsdl.App;
using Exception = System.Exception;

namespace CytiaPrototype.AndroidApp;

public class GameInstance : IDisposable
{
    private CytiaGame? _game;
    private readonly Handler _updateLoop;
    private readonly ChronoTimer _chronoTimer;
    private bool _isPaused;
    private Exception? _isCrashed;
    //rivate GLSurfaceView _surfaceView;
    private Android.Views.SurfaceView _surfaceView;

    private readonly ConcurrentQueue<Func<CytiaGame, Task>> _updateTask = new ();

    public GameInstance(Activity activity)
    {
        _chronoTimer = new ChronoTimer();
        _updateLoop = new Handler(Looper.MyLooper());

        try
        {
            var surface = new GameSurfaceView(activity, () =>
            {
                var renderer = new GameViewRenderer();
                renderer.SetConstructor(CreateGameInstanceWithGlContext);

                return renderer;
            });

            _surfaceView = surface;
            activity.SetContentView(surface);

            /*
            _surfaceView = new Android.Views.SurfaceView(activity);
            activity.SetContentView(_surfaceView);
           
            _renderer = new();
            _renderer.Init(_surfaceView, CreateGameInstanceWithGlContext);*/
        }
        catch (Exception e)
        {
            _isCrashed = e;
        }
    }

    internal void PushUpdateTask(Func<CytiaGame, Task> action) => _updateTask.Enqueue(action);

    private CytiaGame CreateGameInstanceWithGlContext()
    {
        _game = new CytiaGame();
        _game.Init();
        
        return _game;
    }

    public bool TryCheckError(out Exception? e)
    {
        e = _isCrashed;
        _isCrashed = null;

        return e != null;
    }

    public void Dispose()
    {
        _game?.Dispose();
        _updateLoop.Dispose();
        _chronoTimer.Dispose();
    }

    public void OnResume()
    {
        _isPaused = false;
        //_surfaceView.OnResume();
        OnUpdateLoopOnce();
        
    }

    public void OnPaused()
    {
        //_surfaceView.OnPause();
        _isPaused = true;
    }

    private void OnUpdateLoopOnce()
    {
        var game = _game;
        _chronoTimer.Collect(out var time, out var deltaTime);

        if (game != null && _updateTask.TryDequeue(out var task))
        {
            task.Invoke(game);
        }
            
        game?.Update(time, deltaTime);
            
        if(!_isPaused)
            _updateLoop.Post(OnUpdateLoopOnce);
    }
}