using System.Numerics;
using NanoVG;

namespace CytiaPrototype.Graphics;

// TODO: merge it to NanoVG.Bindings
public static class MatrixStateExtensions
{
    public static MatrixState PushPostTransform(this NvgContext ctx, Matrix3x2 mat)
    {
        return new MatrixState(ctx)
            .SaveState()
            .PushPostTransformPrivate(mat);
    }
    
    public static MatrixState PushPreTransform(this NvgContext ctx, Matrix3x2 mat)
    {
        return new MatrixState(ctx)
            .SaveState()
            .PushPreTransformPrivate(mat);
    }
}

// TODO: merge it to NanoVG.Bindings
public class MatrixState : IDisposable
{
    private readonly NvgContext _ctx;

    private Matrix3x2 _previousState;
    private static Matrix3x2 _current = Matrix3x2.Identity;

    internal MatrixState(NvgContext ctx)
    {
        _ctx = ctx;
    }

    // TODO: nvgCurrentTransform doubled matrix somehow, requires fix Bindings or the graphics library.
    internal MatrixState SaveState()
    {
        // This is my private patched dll
        // source code is not shared yet.
        //_ctx.GetTransform(out var mat);
        _previousState = _current;

        _ctx.Save();
        return this;
    }

    internal MatrixState PushPostTransformPrivate(Matrix3x2 mat)
    {
        SetTransformPrivate(_previousState * mat);
        return this;
    }
    
    internal MatrixState PushPreTransformPrivate(Matrix3x2 mat)
    {
        SetTransformPrivate(mat * _previousState);
        return this;
    }

    private void SetTransformPrivate(Matrix3x2 mat)
    {
        // Replace current transform and trying to handle by our own instead
        _ctx.ResetTransform();
        _ctx.Transform(mat);
        _current = mat;
    }
    
    public void Dispose()
    {
        SetTransformPrivate(_previousState);
        _ctx.Restore();
    }
}