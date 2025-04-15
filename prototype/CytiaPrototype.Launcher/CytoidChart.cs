using CytiaPrototype.Launcher.Models;
using CytiaPrototype.Levels;
using CytiaPrototype.Levels.Elements;

namespace CytiaPrototype.Launcher;

public class CytoidChart : ChartBase
{
    private IList<ChartPage> _pages;

    private Queue<ChartPage> _pageQueues;

    // TODO: _lastNoteEnd
    private double _totalDuration, _lastNoteEndDuration;
    
    public CytoidChart()
    {

    }

    internal async Task LoadAsync(CytoidLevelChartModel model)
    {
        var timeBase = model.TimeBase;

        var pages = new List<ChartPage>();

        var sourceNotes = new Queue<CytoidLevelChartModel.Note>(model.Notes
            .OrderBy(a => a.Tick));
        var notes = new Dictionary<long, ChartNote>();
        
        var index = 0ul;
        foreach (var tempoNode in model.TempoList)
        {
            var tempo = tempoNode.Value;

            double ToSecond(long tick)
            {
                return (double)tick / timeBase * tempo / 1000_000;
            }

            var bpm = 60000000.0 / tempo;
            Console.WriteLine($"BPM: {bpm:F2}");
            foreach (var page in model.Pages)
            {
                _totalDuration = Math.Max(_totalDuration, ToSecond(page.End));
                
                var duration = page.End - page.Start;

                var chartPage = new ChartPage
                {
                    Direction = page.Direction switch
                    {
                        > 0 => PageDirection.Down,
                        < 0 => PageDirection.Up,
                        0 => PageDirection.Idle,
                    },
                    Since = ToSecond(page.Start),
                    Duration = ToSecond(duration),
                    Height = 480,
                    Number = index++
                };

                while (sourceNotes.TryPeek(out var peek))
                {
                    if(peek != null && peek.Tick >= page.End)
                        break;

                    sourceNotes.Dequeue();

                    var tick = peek!.Tick;
                    var y = tick - page.Start;

                    var fY = (double)y / duration;

                    var note = new ChartNote
                    {
                        Time = ToSecond(tick),
                        Vertical = chartPage.Direction switch
                        {
                            PageDirection.Up => 1.0 - fY,
                            PageDirection.Down => fY,
                            PageDirection.Idle => fY,
                            _ => throw new ArgumentOutOfRangeException()
                        },
                        Horizontal = peek.X,
                        Kind = peek.Kind switch
                        {
                            CytoidLevelChartModel.NoteKind.Click => ChartNoteKind.Click,
                            CytoidLevelChartModel.NoteKind.Drag => ChartNoteKind.Drag,
                            CytoidLevelChartModel.NoteKind.DragChild => ChartNoteKind.DragChild,
                            CytoidLevelChartModel.NoteKind.Flick => ChartNoteKind.Flick,
                            CytoidLevelChartModel.NoteKind.Hold => ChartNoteKind.Hold,
                            CytoidLevelChartModel.NoteKind.LongHold => ChartNoteKind.LongHold,
                            CytoidLevelChartModel.NoteKind.ClickDrag => ChartNoteKind.ClickDrag,
                            CytoidLevelChartModel.NoteKind.ClickDragChild => ChartNoteKind.ClickDragChild,
                            _ => throw new ArgumentOutOfRangeException()
                        },
                        Duration = ToSecond(peek.HoldTicks),
                        NextId = peek.NextNoteId
                    };

                    // TODO: more precise and correct duration calculation
                    _lastNoteEndDuration = Math.Max(_lastNoteEndDuration, note.Time + note.Duration);
                    
                    chartPage.AddNote(note);
                    AddNoteInternal(peek.Id, note);
                }
                
                pages.Add(chartPage);
            }
        }
        
        _pages = pages;
        
        _pageQueues = new Queue<ChartPage>(pages);
        Console.WriteLine("Parsed a cytoid level");
    }

    protected override string Name => "Cytoid Json Chart";
    protected override string VariantName => "N/A";
    public override double TotalDuration => _totalDuration;
    public override double TrimmedDuration => _lastNoteEndDuration;

    private KeyValuePair<double, ulong>? _prevPagePull;

    public override bool TryPeekNextPage(double time, out ChartPage? page)
    {
        //page =_pages.FirstOrDefault(a => !a.IsCompleted(time));
        //return page != null;

        page = _pages.FirstOrDefault(a => !a.IsCompleted(time));
        return page != null;

        //return _pageQueues.TryPeek(out page); // 
    }

    public override bool TryGetNextPage(double time, out ChartPage? page)
    {
        if (time < (_prevPagePull?.Key ?? 0.0))
        {
            Console.WriteLine("Rewind detected");
            _prevPagePull = null;
        }
        
        page = _pages
            .FirstOrDefault(a => !a.IsCompleted(time) && 
                                 a.Number != _prevPagePull?.Value);

        var result = page != null;

        if (result)
            _prevPagePull = new KeyValuePair<double, ulong>(time, page.Number);
        
        return page != null;
        //return _pageQueues.TryDequeue(out page);
    }
}