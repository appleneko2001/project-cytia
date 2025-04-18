using System.Reflection;

namespace CytiaPrototype.Assets;

public class GameAssets
{
    private readonly Assembly _me;
    
    public GameAssets()
    {
        _me = typeof(GameAssets).Assembly;
        RetrieveUsableAssetsPrivate();
    }

    private void RetrieveUsableAssetsPrivate()
    {
        foreach (var entry in _me.GetManifestResourceNames())
            Console.WriteLine(entry);
    }

    public Stream? GetStream(string path) => _me.GetManifestResourceStream(path.Replace('\\', '/'));
}