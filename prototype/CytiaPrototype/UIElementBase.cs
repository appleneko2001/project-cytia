using System.Numerics;

namespace CytiaPrototype;

public abstract class UIElementBase
{
    private Vector2 _viewSize;

    internal Vector2 ViewSize => _viewSize;

    /// <summary>
    /// Update your own objects size by using this callback. It's useless to call base's implementation and can be removed safely.
    /// </summary>
    protected virtual void UpdateViewSizePrivate(float w, float h)
    {
    }
    
    public void UpdateViewSize(float w, float h)
    {
        UpdateViewSize(new Vector2(w, h));
    }
    
    public void UpdateViewSize(Vector2 v)
    {
        _viewSize = v;
        UpdateViewSizePrivate(v.X, v.Y);
    }
}