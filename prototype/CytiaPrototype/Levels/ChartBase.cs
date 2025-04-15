using CytiaPrototype.Levels.Elements;

namespace CytiaPrototype.Levels;

public abstract class ChartBase
{
    protected abstract string Name { get; }
    protected abstract string VariantName { get; }
    
    public abstract double TotalDuration { get; }

    public IReadOnlyDictionary<long, ChartNote> Notes => _notes;
    private Dictionary<long, ChartNote> _notes = new();
    
    internal void TryGetChart()
    {
        
    }

    public override string ToString()
    {
        return $"Chart {Name} ({VariantName})";
    }
    
    public void Reset()
    {
        throw new NotImplementedException();
    }

    public virtual bool TryPeekNextPage(double time, out ChartPage? page)
    {
        throw new NotImplementedException();
    }

    public virtual bool TryGetNextPage(double runTime, out ChartPage? page)
    {
        throw new NotImplementedException();
    }

    protected virtual void AddNoteInternal(long id, ChartNote note)
    {
        _notes.Add(id, note);
    }
}