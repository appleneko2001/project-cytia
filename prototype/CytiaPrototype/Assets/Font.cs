using NanoVG;

namespace CytiaPrototype.Assets;

public class Font : IDisposable
{
    private readonly string _path;
    private readonly string _name;
    private int? _resId;
    private bool _isFailed;

    private NvgContext _ctx;

    public Font(string[] path) : this(Path.Combine(path))
    {
        
    }
    
    public Font(string path)
    {
        var name = Path.GetFileName(path);
        var subName = "Regular.ttf";

        var actualFontPath = Path.Combine(path, subName);
        _path = actualFontPath;
        _name = name;
    }

    public void Use(NvgContext ctx)
    {
        if(_isFailed)
            return;
        
        if (!_resId.HasValue)
        {
            var res = ctx.CreateFont(_name, _path);
            if (res == -1)
            {
                MarkAsFailedResource();
                return;
            }

            _resId = res;
        }

        if (!_resId.HasValue)
        {
            MarkAsFailedResource();
            return;
        }
        
        ctx.FontFaceId(_resId.Value);
    }

    private void MarkAsFailedResource()
    {
        Console.WriteLine("Failed");
        _isFailed = true;
    }

    public void Dispose()
    {
        // TODO release managed resources here
    }
}