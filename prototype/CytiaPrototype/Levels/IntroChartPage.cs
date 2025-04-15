namespace CytiaPrototype.Levels;

internal class IntroChartPage : ChartPage
{
    public override string ToString()
    {
        return $"Intro {TimeSpan.FromSeconds(Duration)}";
    }
}