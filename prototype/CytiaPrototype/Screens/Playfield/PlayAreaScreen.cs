using System.Drawing;
using System.Numerics;
using CytiaPrototype.Extensions;
using CytiaPrototype.Graphics;
using CytiaPrototype.Levels;
using CytiaPrototype.Levels.Elements;
using CytiaPrototype.Screens.Playfield.Elements;
using NanoVG;

namespace CytiaPrototype.Screens.Playfield;

public class PlayAreaScreen : UIElementBase, IDisposable
{
    private double _time;

    // Play area vertical margin
    const float margin = 120f;

    // Play area view size


    // Inner play area height 
    private float _playfieldViewHeight;

    // Fixed play area height
    private float _playfieldWidth = 800;
    private float _playfieldHeight = 480;

    // Acquired element scale amount
    private float _scaleElementAmount;

    private float _userPreferenceScaleAmount = 0.6f;

    // Play state
    private ChartBase? _chart;
    private double? _chartPlayStartTimeSpan;
    private double? _currentTime;
    private bool _started;
    private bool _introPushed;
    private bool _endOfPages;
    private bool _scanlineVisible;

    private ChartPage? _currentPage;

    // Progress number
    private float? _scanlineY;

    // Play area decorations
    private ScrollCellsLine _upperLine = new()
    {
        IsUpperLine = true
    };

    private ScrollCellsLine _lowerLine = new()
    {
        IsUpperLine = false
    };

    private ChartProgress _progress = new()
    {
        Height = 8.0
    };

    private ChartPage? CurrentPage
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
                
                if(!_scanlineVisible)
                    break;

                switch (old.Direction)
                {
                    case PageDirection.Up:
                        // Scale up upper scroll line
                        _upperLine.ScaleUp();
                        break;

                    case PageDirection.Down:
                        // Scale up lower scroll line
                        _lowerLine.ScaleUp();
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

    internal ChartPage? NextPage { get; set; }

    protected override void UpdateViewSizePrivate(float w, float h)
    {
        _upperLine?.UpdateViewSize(w, h);
        _lowerLine?.UpdateViewSize(w, h);
        _progress?.UpdateViewSize(w, h);

        _playfieldViewHeight = h - margin - margin;
        _scaleElementAmount = h / _playfieldHeight;
    }

    public void Update(double runTime, double deltaTime)
    {
        _time += deltaTime;

        _upperLine.Update(runTime, deltaTime);
        _lowerLine.Update(runTime, deltaTime);

        TryActivePlaySession(runTime);
        TryDoSessionStateUpdate(_time);
    }


    private const int _introFadeInDuration = 1;
    
    

    private void TryActivePlaySession(double runTime)
    {
        if (!_chartPlayStartTimeSpan.HasValue || runTime < _chartPlayStartTimeSpan - _introFadeInDuration)
            return;

        if (!_started)
        {
            //_progress.SetStart(_chartPlayStartTimeSpan ?? 0);
            _progress.SetDuration(GetChartDuration());

            _started = true;
            Console.WriteLine("Intro");
        }
    }

    private double GetChartDuration()
    {
        return _chart?.TotalDuration ?? 0;
        return _chart?.TrimmedDuration ?? 0;
    }

    private void TryDoSessionStateUpdate(double srcTime)
    {
        var chart = _chart;

        if (chart == null)
            return;

        if (!_started)
            return;

        var since = _chartPlayStartTimeSpan ?? throw new InvalidOperationException();

        var time = Math.Max(srcTime - since, -1);
        _progress.Update(time);
        _currentTime = time;

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

    public void UseChart(ChartBase chart)
    {
        // Intro
        _chart = chart;
        var whenStart = _time + 3;

        _chartPlayStartTimeSpan = whenStart;
        Console.WriteLine($"Play {chart}, begin in {TimeSpan.FromSeconds(whenStart):g}");
    }

    public void Draw(NvgContext ctx)
    {
        var h = ViewSize.Y;

        // upper scroll cell line
        using (ctx.PushPostTransform(Matrix3x2.CreateTranslation(0, margin)))
            _upperLine.Draw(ctx);

        // lower scroll cell line
        using (ctx.PushPostTransform(Matrix3x2.CreateTranslation(0, h - margin)))
            _lowerLine.Draw(ctx);

        // Play area and scanline
        using (ctx.PushPostTransform(Matrix3x2.CreateTranslation(0, margin)))
            DrawPlayAreaPrivate(ctx);

        // Chart progress bar (upper)
        _progress.Draw(ctx);
    }

    private void DrawPlayAreaPrivate(NvgContext ctx)
    {
        var vSize = ViewSize;
        var h = _playfieldViewHeight;

        var page = CurrentPage;
        var dir = page?.Direction;
        var intensity = page is not IntroChartPage ? 1.0f:0.0f; 
        var y = (_scanlineY % 1.0f)  * intensity;

        if (y.HasValue == false)
            return;

        if (dir.HasValue == false)
            return;

        y = dir.Value switch
        {
            PageDirection.Down => y,
            PageDirection.Up => 1.0f - y,
            _ => y * 0.0f
        };

        var v = (y * h)!.Value;
        
        // Debug: Play area frame
#if DEBUG_
        ctx.BeginPath();
        ctx.StrokeColor(Vector4.One);
        ctx.StrokeWidth(8);
        ctx.Rect(0,0, vSize.X, h);
        ctx.Stroke();
#endif
        var notesTable = _chart?.Notes;

        // Draw current page notes
        ctx.Save();
        for (var i = 0; i < (notesTable?.Count ?? 0); i++)
        {
            var note = notesTable?[i];

            if (note == null)
                continue;

            DrawNotePrivate(ctx, vSize with { Y = h }, note, notesTable!);
        }
        ctx.Restore();
        
        // Scanline
        DrawScanlinePrivate(ctx, vSize.X, v);
    }

    private void DrawNotePrivate(NvgContext ctx, Vector2 vSize, ChartNote note, IReadOnlyDictionary<long, ChartNote> notes)
    {
        const double min = -0.25, max = 1.0;

        var time = (_currentTime ?? 0) - note.Time;
        var remains = Math.Abs(time);
        
        if (time < min || remains > max)
            return;
        
        float GetX(double abs) => (float)(vSize.X * abs);
        float GetY(double abs) => (float)(vSize.Y * abs);

        var size = note.Kind switch
        {
            ChartNoteKind.DragChild => 32,
            ChartNoteKind.ClickDragChild => 32,
            _ => 56
        };

        var colour = note.Kind switch
        {
            ChartNoteKind.Drag => Color.MediumPurple.ToVec4(),
            ChartNoteKind.DragChild => Color.MediumPurple.ToVec4(),
            ChartNoteKind.Hold => Color.Gold.ToVec4(),
            ChartNoteKind.LongHold => Color.OrangeRed.ToVec4(),
            _ => Color.CornflowerBlue.ToVec4()
        };
        var scale = _scaleElementAmount * _userPreferenceScaleAmount;

        ctx.GlobalAlpha((float)time.LinearFadeEdge(min, 0, 0.5, max).Clamp(0, 1));
        var nextNoteId = note.NextId;

        if (nextNoteId >= 0)
        {
            switch (note.Kind)
            {
                case ChartNoteKind.Drag:
                case ChartNoteKind.DragChild:
                case ChartNoteKind.ClickDrag:
                case ChartNoteKind.ClickDragChild:
                    var a = note;
                    var b = notes[nextNoteId] ?? throw new ArgumentNullException();

                    ctx.BeginPath();
                    ctx.StrokeColor(colour);
                    ctx.StrokeWidth(24 * scale);
                    ctx.MoveTo(GetX(a.Horizontal), GetY(a.Vertical));
                    ctx.LineTo(GetX(b.Horizontal), GetY(b.Vertical));
                    ctx.Stroke();
                    break;
            }
        }

        using (ctx.PushPostTransform(Matrix3x2.CreateTranslation(
                   GetX(note.Horizontal), 
                   GetY(note.Vertical))))
        {
            ctx.BeginPath();
            ctx.Circle(0, 0, size * scale);
            ctx.FillColor(colour);

            ctx.Fill();
            ctx.StrokeColor(Vector4.One);
            ctx.StrokeWidth(16 * scale);
            ctx.Stroke();
        }
    }

    private void DrawScanlinePrivate(NvgContext ctx, float w, float y)
    {
        var duration = GetChartDuration();
        var end = duration + 1;
        var halfW = w * 0.5f;

        var time = _currentTime ?? 0;
        
        var visibility = time.LinearFadeEdge(-1, 0, duration, end);
        _scanlineVisible = visibility > 0;
        if(!_scanlineVisible)
            return;
        
        var scale = visibility.Clamp(0, 1);

        using var _ = ctx.PushPostTransform(
            Matrix3x2.CreateScale((float)scale, 1)*
            Matrix3x2.CreateTranslation(halfW, y) 
        );
        ctx.BeginPath();

        ctx.FillColor(Vector4.One);
        ctx.Rect(-halfW, -2f, w, 4f);
        ctx.Fill();
    }

    public void Dispose()
    {
        // TODO release managed resources here
    }
    
    public void Seek(int amount)
    {
        _time = (_time + amount).Clamp(0, GetChartDuration());
        NextPage = null;
        CurrentPage = null;
    }
}