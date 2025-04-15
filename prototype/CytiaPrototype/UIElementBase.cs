using System.Numerics;

namespace CytiaPrototype;

public abstract class UIElementBase
{
    private Vector2 _viewSize;

    internal Vector2 ViewSize => _viewSize; 

    protected virtual void UpdateViewSizePrivate(float w, float h)
    {
        
    }
    
    public void UpdateViewSize(float w, float h)
    {
        _viewSize = new Vector2(w, h);

        UpdateViewSizePrivate(w, h);
    }
}