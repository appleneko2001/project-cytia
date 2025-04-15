using CytiaPrototype.Levels.Elements;

namespace CytiaPrototype.Levels;

public class ChartPage
{
    public PageDirection Direction;
    public float Height;
    public double Duration;
    public double Since;
    public ulong Number;

    internal double End => Since + Duration;

    public readonly List<ChartNote> Notes = new ();

    public virtual bool IsValid(double time)
    {
        return time >= Since && IsCompleted(time);
    }

    public virtual bool IsCompleted(double time)
    {
        return time >= End;
    }

    public float TryGetScanline(double time)
    {
        var point = time - Since;
        return (float)(point / Duration);
    }

    public override string ToString()
    {
        return $"{TimeSpan.FromSeconds(Since)} - {TimeSpan.FromSeconds(End)} {Direction}";
    }

    public void AddNote(ChartNote note)
    {
        Notes.Add(note);
    }
}