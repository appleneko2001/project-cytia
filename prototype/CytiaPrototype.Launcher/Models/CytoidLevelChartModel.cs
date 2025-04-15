using System.Text.Json.Serialization;

namespace CytiaPrototype.Launcher.Models;

// https://github.com/Cytoid/wiki/blob/main/docs/zh/reference/chart/c2-format.md
[Serializable]
public class CytoidLevelChartModel
{
    [Serializable]
    public class Page
    {
        [JsonPropertyName("start_tick")]
        public long Start { get; set; }
        
        [JsonPropertyName("end_tick")]
        public long End { get; set; }
        
        [JsonPropertyName("scan_line_direction")]
        public int Direction { get; set; }
    }
    
    [Serializable]
    public class Tempo
    {
        [JsonPropertyName("tick")]
        public long Since { get; set; }
        
        [JsonPropertyName("value")]
        public long Value { get; set; }
    }
    
    [Serializable]
    public class Event
    {
        // TODO: event implementation
    }
    
    public enum NoteKind : int
    {
        Click =0,
        Hold =1,
        LongHold=2, 
        Drag =3, 
        DragChild = 4, 
        Flick = 5,
        ClickDrag = 6, 
        ClickDragChild = 7, 
        DropClick =8,
        DropDrag = 9
    }
    
    [Serializable]
    public class Note
    {
        [JsonPropertyName("page_index")]
        public int PageId { get; set; }
        
        [JsonPropertyName("type")]
        [JsonConverter(typeof(JsonNumberEnumConverter<NoteKind>))]
        public NoteKind Kind { get; set; }
        
        [JsonPropertyName("id")]
        public long Id { get; set; }
        
        [JsonPropertyName("tick")]
        public long Tick { get; set; }
        
        [JsonPropertyName("x")]
        public double X { get; set; }
        
        [JsonPropertyName("hold_tick")]
        public long HoldTicks { get; set; }
        
        [JsonPropertyName("next_id")]
        public long NextNoteId { get; set; }
        
        
    }
    
    [JsonPropertyName("format_version")]
    public int FormatVersion { get; set; }

    [JsonPropertyName("time_base")] 
    public int TimeBase { get; set; } = 480;
    
    [JsonPropertyName("start_offset_time")]
    public double StartOffsetTime { get; set; }
    
    [JsonPropertyName("music_offset")]
    public double MusicOffset { get; set; }
    
    [JsonPropertyName("page_list")]
    public IList<Page> Pages { get; set; }
    
    [JsonPropertyName("tempo_list")]
    public IList<Tempo> TempoList { get; set; }
    
    [JsonPropertyName("note_list")]
    public IList<Note> Notes { get; set; }

    public override string ToString()
    {
        return $"Format: {FormatVersion}, Pages: {Pages?.Count}, Notes: {Notes?.Count}";
    }
}