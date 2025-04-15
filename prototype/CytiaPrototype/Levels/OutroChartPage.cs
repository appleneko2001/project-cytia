namespace CytiaPrototype.Levels;

internal class OutroChartPage : ChartPage
{
    public override string ToString()
    {
        return $"Outro {TimeSpan.FromSeconds(Duration)}";
    }
}