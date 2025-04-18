using NanoVG;

namespace CytiaPrototype.Assets;

public class Font : IDisposable
{
    private readonly string _path;
    private readonly string _name;
    private int? _resId;
    private bool _isFailed;

    private NvgContext _ctx;
    private GameAssets _assets;

    public Font(GameAssets assets, string[] path) : this(Path.Combine(path))
    {
        _assets = assets;
    }
    
    private Font(string path)
    {
        var name = Path.GetFileName(path);
        var subName = "Regular.ttf";

        var actualFontPath = Path.Combine(path, subName);
        _path = actualFontPath;
        _name = name;
    }

    public unsafe void Use(NvgContext ctx)
    {
        if(_isFailed)
            return;
        
        if (!_resId.HasValue)
        {
            using var mem = new MemoryStream();
            using (var source = _assets.GetStream(_path) )
            {
                source?.CopyTo(mem);
            }

            fixed (void* ptr = mem.ToArray())
            {
                var res = ctx.CreateFontMem(_name, (IntPtr)ptr, (int)mem.Length, 0);
                if (res == -1)
                {
                    MarkAsFailedResource();
                    return;
                }

                _resId = res;
            }
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