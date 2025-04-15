namespace CytiaPrototype.Extensions;

public static class NumberExtensions
{
    public static double Clamp(this double v, double min, double max) => 
        v > max ? Math.Min(v, max) : v < min ? Math.Max(v, min) : v;

    public static double LinearLerp(this double v, double a, double b)
    {
        return (b - a) * v + a;
    }
    
    // IM LAZY TO THINK HOW TO IMPLEMENT, ASKED GEMINI LOL
    // Helper function to perform the normalized linear interpolation
    // Calculates (value - start) / (end - start)
    public static double InverseLerp(this double value, double start, double end)
    {
        if (Math.Abs(start - end) < 1e-9) // Avoid division by zero or near-zero
            return 0.0;
        return (value - start) / (end - start);
    }

    /// <summary>
    /// Creates a trapezoidal shape: fades in from 0 to 1 between a0 and a1,
    /// stays at 1 between a1 and b0, and fades out from 1 to 0 between b0 and b1.
    /// </summary>
    /// <param name="input">The input value (like x-coordinate).</param>
    /// <param name="a0">Start of fade-in.</param>
    /// <param name="a1">End of fade-in / Start of plateau.</param>
    /// <param name="b0">End of plateau / Start of fade-out.</param>
    /// <param name="b1">End of fade-out.</param>
    /// <returns>Value between 0.0 and 1.0 based on the trapezoidal shape.</returns>
    public static double LinearFadeEdge(this double input, double a0, double a1, double b0, double b1)
    {
        if (input < a0)
        {
            // Before fade-in
            return 0.0; 
        }
        else if (input >= a0 && input < a1)
        {
            // Fade-in: Linear interpolation from 0 to 1
            // Clamp ensures result stays within [0, 1] even with potential floating point inaccuracies
            return Clamp(input.InverseLerp(a0, a1), 0.0, 1.0);
        }
        else if (input >= a1 && input < b0)
        {
            // Plateau
            return 1.0; 
        }
        else if (input >= b0 && input < b1)
        {
            // Fade-out: Linear interpolation from 1 down to 0
            // Achieved by 1.0 - (interpolation factor from 0 to 1)
            var fadeFactor = input.InverseLerp(b0, b1);
            return Clamp(1.0 - fadeFactor, 0.0, 1.0);
        }
        else 
        {
            // After fade-out
            return 0.0; 
        }
    }
}