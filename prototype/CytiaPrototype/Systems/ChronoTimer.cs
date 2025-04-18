using System.Diagnostics;

namespace CytiaPrototype.Systems;

public class ChronoTimer : IDisposable
{
    private readonly Stopwatch _stopwatch;
    private TimeSpan _prevTime;

    public ChronoTimer()
    {
        _stopwatch = new Stopwatch();
        _stopwatch.Start();
    }

    public void Collect(out double runTime, out double deltaTime)
    {
        var prevTime = _prevTime;
        var now = _stopwatch.Elapsed;
        _prevTime = now;

        deltaTime = (now - prevTime).TotalSeconds;
        runTime = now.TotalSeconds;
    }
    
    public void Dispose()
    {
        _stopwatch.Stop();
    }
}