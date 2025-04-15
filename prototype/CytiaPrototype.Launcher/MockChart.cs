using CytiaPrototype.Extensions;
using CytiaPrototype.Levels;

namespace CytiaPrototype.Launcher;

public class MockChart : ChartBase
{
    protected override string Name => "Demo";
    protected override string VariantName => "None";
    public override double TotalDuration => throw new NotImplementedException();
    public override double TrimmedDuration => throw new NotImplementedException();

    private double DurationPerPage = 1.0;
    private double _nextPageAt;
    private ulong _currentPage;
    private PageDirection _nextDirection = PageDirection.Down;

    private ChartPage? _existedNextChart;

    public override bool TryPeekNextPage(double time, out ChartPage? page)
    {
        if (_existedNextChart == null)
            _existedNextChart = SpawnPage(_currentPage);

        page = _existedNextChart;
        return _existedNextChart != null;
    }

    public override bool TryGetNextPage(double time, out ChartPage? page)
    {
        var since = _currentPage;

        if (_existedNextChart != null)
        {
            page = _existedNextChart;
            _existedNextChart = null;
            return true;
        }

        page = SpawnPage(since);
        if (page == null)
            return false;

        Console.WriteLine($"Next Page#{_currentPage} {page} spawned");
        return true;
    }

    private ChartPage? SpawnPage(ulong since)
    {
        if (since > 5)
            return null;
        
        _currentPage++;
        
        return new ChartPage
        {
            Number = _currentPage,
            Duration = DurationPerPage,
            Direction = _nextDirection = 
                _nextDirection.Reverse(),
            Height = 480,
            Since = since
        };
    }
}