namespace CytiaPrototype.Levels.Elements;

public class ChartNote
{
    public double Time { get; set; }
    
    public double Horizontal { get; set; }
    
    public double Vertical { get; set; }
    
    public ChartNoteKind Kind { get; set; }
    
    public double Duration { get; set; }
    
    public long NextId { get; set; }

    public double End => Time + Duration;
}