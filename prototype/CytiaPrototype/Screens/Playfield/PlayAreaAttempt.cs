using CytiaPrototype.Extensions;
using CytiaPrototype.Levels;
using CytiaPrototype.Levels.Elements;

namespace CytiaPrototype.Screens.Playfield;

public class PlayAreaAttempt
{
    private ChartBase _chart;
    private double _currentTime = -1;
    private bool _started;
    private bool _introPushed;
    private bool _endOfPages;

    private bool _upperLineTrigger, _lowerLineTrigger;
    
    private const int _introFadeInDuration = 1;
    
    private ChartPage? _currentPage;

    // Progress number
    private double? _scanlineY;

    public PlayAreaAttempt(ChartBase chart)
    {
        _chart = chart;
    }

    public double CurrentTime
    {
        get => _currentTime;
        private set => _currentTime = value;
    }
    
    internal double GetChartDuration()
    {
        //return _chart?.TotalDuration ?? 0;
        return _chart?.TrimmedDuration ?? 0;
    }

    public double CurrentPageProgress => _scanlineY ?? 0;

    public ChartPage? NextPage { get; set; }

    internal ChartPage? CurrentPage
    {
        get => _currentPage;
        set
        {
            var cur = value;
            var old = _currentPage;
            _currentPage = cur;

            do
            {
                if (old == null)
                    break;

                switch (old.Direction)
                {
                    case PageDirection.Up:
                        // Scale up upper scroll line
                        //_upperLine.ScaleUp();
                        _upperLineTrigger = true;
                        break;

                    case PageDirection.Down:
                        // Scale up lower scroll line
                        //_lowerLine.ScaleUp();
                        _lowerLineTrigger = true;
                        break;
                }
            } while (false);

            do
            {
                if (cur == null)
                    return;

                Console.WriteLine($"Current Page#{cur.Number} {cur}");
            } while (false);
        }
    }

    internal bool GetUpperScrollLineTrigger()
    {
        var old = _upperLineTrigger;
        _upperLineTrigger = false;
        return old;
    }
    
    internal bool GetLowerScrollLineTrigger()
    {
        var old = _lowerLineTrigger;
        _lowerLineTrigger = false;
        return old;
    }

    internal void TryDoSessionStateUpdate(double time)
    {
        var chart = _chart;
        CurrentTime = time;

        var curPage = CurrentPage;
        _scanlineY = curPage?.TryGetScanline(time);

        if (NextPage == null)
        {
            ChartPage? page = null;

            if (!_introPushed)
            {
                _introPushed = true;

                if (!chart.TryPeekNextPage(time, out var peek))
                {
                    throw new InvalidOperationException();
                }

                page = new IntroChartPage
                {
                    Duration = _introFadeInDuration,
                    Since = -_introFadeInDuration,
                    Direction = peek?.Direction ?? PageDirection.Down,
                    Height = 0,
                    Number = 0
                };
            }

            if (page == null)
            {
                TryGetNextPagePrivate(chart, time, curPage, out page);
            }

            NextPage = page;
        }

        if (curPage?.IsCompleted(time) ?? false)
            CurrentPage = null;

        if (CurrentPage == null)
        {
            CurrentPage = NextPage;
            NextPage = null;
        }
    }

    private void TryGetNextPagePrivate(ChartBase chart, double time, ChartPage? curPage, out ChartPage? page)
    {
        //Console.WriteLine($"Query next page at {TimeSpan.FromSeconds(time)}");
        if (chart.TryGetNextPage(time, out page))
        {
            _endOfPages = false;
            Console.WriteLine($"Returned: {page}");
            return;
        }
        if (_endOfPages)
            return;

        _endOfPages = true;
        Console.WriteLine("End of page");
        
        page = new OutroChartPage
        {
            Duration = 1.0f,
            Height = 0f,
            Number = ulong.MaxValue,
            Direction = curPage?.Direction.Reverse() ?? PageDirection.Idle,
            Since = curPage?.End ?? 0
        };
    }

    public IReadOnlyDictionary<long, ChartNote> GetChartNotes()
    {
        return _chart.Notes;
    }

    public void Update(double deltaTime)
    {
        CurrentTime += deltaTime;
        
        //TryActivePlaySession(CurrentTime);
        TryDoSessionStateUpdate(CurrentTime);
    }

    internal void Draw()
    {
        
    }

    public void Seek(int amount)
    {
        return;
        
        CurrentTime = (CurrentTime + amount)
            .Clamp(0, GetChartDuration());
        NextPage = null;
        CurrentPage = null;
    }
}