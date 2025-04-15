using CytiaPrototype.Levels;

namespace CytiaPrototype.Extensions;

public static class PageExtensions
{
    public static PageDirection Reverse(this PageDirection dir)
    {
        if (dir == PageDirection.Idle)
            return PageDirection.Idle;
        
        return dir == PageDirection.Up ? PageDirection.Down : PageDirection.Up;
    }
}