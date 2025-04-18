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
    //private double _time;

    // Play area vertical margin
    const float margin = 120f;

    // Inner play area height 
    private float _playfieldViewHeight;

    // Fixed play area height
    private float _playfieldWidth = 800;
    private float _playfieldHeight = 480;

    // Acquired element scale amount
    private float _scaleElementAmount;

    private float _userPreferenceScaleAmount = 1.0f;

    // Play state
    private bool _scanlineVisible;
    private PlayAreaAttempt? _currentAttempt;
    private ChartBase _loadSource;

    // Play area decorations
    private ScrollCellsLine _upperLine = new() { Reverse = true };

    private ScrollCellsLine _lowerLine = new() { Reverse = false };

    private ChartProgress _progress = new()
    {
        Height = 8.0
    };

    private AttemptCounter _attemptCounter = new();

    internal PlayAreaAttempt? CurrentAttempt => _currentAttempt;

    protected override void UpdateViewSizePrivate(float w, float h)
    {
        _upperLine.UpdateViewSize(w, h);
        _lowerLine.UpdateViewSize(w, h);
        _progress.UpdateViewSize(w, h);

        _playfieldViewHeight = h - margin - margin;
        _scaleElementAmount = h / _playfieldHeight;
    }

    public void Update(double runTime, double deltaTime)
    {
        _upperLine.Update(deltaTime);
        _lowerLine.Update(deltaTime);
        _attemptCounter.Update(deltaTime);

        var attempt = CurrentAttempt;

        if (attempt == null)
            return;
        
        attempt.Update(deltaTime);
        _progress.Update(attempt.CurrentTime);
        
        if(attempt.GetUpperScrollLineTrigger())
            _upperLine.ScaleUp();
        
        if (attempt.GetLowerScrollLineTrigger())
            _lowerLine.ScaleUp();
    }

    public void UseChart(ChartBase chart)
    {
        // Intro
        _loadSource = chart;
        Attempt();

        // TODO: refactor
        Console.WriteLine($"Play {chart}, begin in {TimeSpan.FromSeconds(0):g}");
    }

    internal void Attempt()
    {
        var attempt = new PlayAreaAttempt(_loadSource);
        
        _currentAttempt = attempt;
        _progress.SetDuration(attempt.GetChartDuration());
        
        _attemptCounter.Increase();
        
        Console.WriteLine($"Play {_loadSource}, begin in {TimeSpan.FromSeconds(0):g}");
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

        _attemptCounter.Draw(ctx);
    }

    private void DrawPlayAreaPrivate(NvgContext ctx)
    {
        var vSize = ViewSize;
        var h = _playfieldViewHeight;

        var page = _currentAttempt?.CurrentPage;
        var dir = page?.Direction;
        var intensity = page is not IntroChartPage ? 1.0f:0.0f; 
        var y = (_currentAttempt?.CurrentPageProgress % 1.0f)  * intensity;

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
        var notesTable = CurrentAttempt?.GetChartNotes();

        // Draw current page notes
        ctx.Save();
        for (var i = 0; i < (notesTable?.Count ?? 0); i++)
        {
            var note = notesTable?[i];

            if (note == null)
                continue;

            DrawNotePrivate(ctx, vSize with { Y = h }, dir.Value, note, notesTable!);
        }
        ctx.Restore();
        
        // Scanline
        DrawScanlinePrivate(ctx, vSize.X, v);
    }

    private void DrawNotePrivate(NvgContext ctx, Vector2 vSize, PageDirection dir, ChartNote note,
        IReadOnlyDictionary<long, ChartNote> notes)
    {
        const double min = -0.25, end = 0, max = 0.25;

        var time = (_currentAttempt?.CurrentTime ?? 0) - note.Time;
        var remains = Math.Abs(time);
        
        var fEnd = note.Duration + end;
        var fMax = note.Duration + max;
        
        if (time < min || remains > fMax)
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

        ctx.GlobalAlpha((float)time.LinearFadeEdge(min, 0, fEnd, fMax).Clamp(0, 1));
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

        switch (note.Kind)
        {
            case ChartNoteKind.Hold:
            case ChartNoteKind.LongHold:
                var a = note.Vertical;
                var b = note.Vertical - note.Duration;

                ctx.BeginPath();
                ctx.StrokeColor(colour);
                ctx.StrokeWidth(48 * scale);
                ctx.MoveTo(GetX(note.Horizontal), GetY(a));
                ctx.LineTo(GetX(note.Horizontal), GetY(b));
                ctx.Stroke();
                //note.Duration;
                break;
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

    private void DrawScanlinePrivate(NvgContext ctx, float w, double y)
    {
        var duration = _currentAttempt?.GetChartDuration() ?? 0;
        var end = duration + 1;
        var halfW = w * 0.5f;

        var time = _currentAttempt?.CurrentTime ?? 0;
        
        var visibility = time.LinearFadeEdge(-1, 0, duration, end);
        _scanlineVisible = visibility > 0;
        if(!_scanlineVisible)
            return;
        
        var scale = visibility.Clamp(0, 1);

        using var _ = ctx.PushPostTransform(
            Matrix3x2.CreateScale((float)scale, 1)*
            Matrix3x2.CreateTranslation(halfW, (float)y) 
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
    

}