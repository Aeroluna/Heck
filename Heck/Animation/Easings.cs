using Math = UnityEngine.Mathf;

namespace Heck.Animation;

/// <summary>
///     Easing Functions enumeration
/// </summary>
// ReSharper disable InconsistentNaming
#pragma warning disable SA1300 // Element should begin with upper-case letter
public enum Functions
{
    easeLinear,
    easeStep,
    easeInQuad,
    easeOutQuad,
    easeInOutQuad,
    easeInCubic,
    easeOutCubic,
    easeInOutCubic,
    easeInQuart,
    easeOutQuart,
    easeInOutQuart,
    easeInQuint,
    easeOutQuint,
    easeInOutQuint,
    easeInSine,
    easeOutSine,
    easeInOutSine,
    easeInCirc,
    easeOutCirc,
    easeInOutCirc,
    easeInExpo,
    easeOutExpo,
    easeInOutExpo,
    easeInElastic,
    easeOutElastic,
    easeInOutElastic,
    easeInBack,
    easeOutBack,
    easeInOutBack,
    easeInBounce,
    easeOutBounce,
    easeInOutBounce
}

#pragma warning restore SA1300 // Element should begin with upper-case letter

public static class Easings
{
    /// <summary>
    ///     Constant Pi / 2.
    /// </summary>
    private const float HALFPI = Math.PI / 2.0f;

    /// <summary>
    ///     Constant Pi.
    /// </summary>
    private const float PI = Math.PI;

    /// <summary>
    ///     Dips down to (x:0.53, y:-0.379), shooting quickly up to 1. Like jumping up from a trampoline
    /// </summary>
    public static float EaseInBack(float p)
    {
        return (p * p * p) - (p * Math.Sin(p * PI));
    }

    /// <summary>
    ///     Bouncing from 0 up to 1. Like the Solitaire bouncing cards effect in reverse.
    /// </summary>
    public static float EaseInBounce(float p)
    {
        return 1 - EaseOutBounce(1 - p);
    }

    /// <summary>
    ///     Bottom half of a unit circle centered around (x:0, y:1). Like riding the shape of a perfect bowl
    /// </summary>
    public static float EaseInCirc(float p)
    {
        return 1f - Math.Sqrt(Math.Max(Math.Epsilon, 1f - p * p));
    }

    /// <summary>
    ///     Modeled after the cubic y = x^3
    /// </summary>
    public static float EaseInCubic(float p)
    {
        return p * p * p;
    }

    /// <summary>
    ///     Modeled after the damped sine wave y = sin(13pi/2*x)*Math.Pow(2, 10 * (x - 1))
    /// </summary>
    public static float EaseInElastic(float p)
    {
        return Math.Sin(13 * HALFPI * p) * Math.Pow(2, 10 * (p - 1));
    }

    /// <summary>
    ///     Very slow climb from 0 to 1, exponentially accelerating at the end. Like magnets flying together
    /// </summary>
    public static float EaseInExpo(float p)
    {
        const float S = 1f / 1023f;
        return p <= 0.0f ? p : (Math.Pow(2f, 10f * p) * S - S);
    }

    /// <summary>
    ///     Drops down to (x:0.265 y:-0.190), overshoots to (x:0.735 y:1.190), smoothly landing to 1. Like throwing someone upward aiming for 1
    /// </summary>
    public static float EaseInOutBack(float p)
    {
        if (p < 0.5f)
        {
            float f = 2 * p;
            return 0.5f * ((f * f * f) - (f * Math.Sin(f * PI)));
        }
        else
        {
            float f = 1 - ((2 * p) - 1);
            return (0.5f * (1 - ((f * f * f) - (f * Math.Sin(f * PI))))) + 0.5f;
        }
    }

    /// <summary>
    ///     Bouncing from the 0 line to the 1 line. Like if you bounced from a trampoline on a planet to another trampoline on another planet in mario galaxy
    /// </summary>
    public static float EaseInOutBounce(float p)
    {
        if (p < 0.5f)
        {
            return 0.5f * EaseInBounce(p * 2);
        }

        return (0.5f * EaseOutBounce((p * 2) - 1)) + 0.5f;
    }

    /// <summary>
    ///     Place a circle at (x:0.5 y:0.5) with radius 0.5, and shift the top half 1 unit to the right. Thats this function.
    /// </summary>
    public static float EaseInOutCirc(float p)
    {
        if(p < 0.5f)
        {
            return 0.5f - Math.Sqrt(Math.Max(Math.Epsilon, 0.25f - p*p));
        }

        p -= 1f;
        return 0.5f + Math.Sqrt(Math.Max(Math.Epsilon, 0.25f - p*p));
    }

    /// <summary>
    ///     Modeled after the piecewise cubic
    ///     y = (1/2)((2x)^3)       ; [0, 0.5)
    ///     y = (1/2)((2x-2)^3 + 2) ; [0.5, 1]
    /// </summary>
    public static float EaseInOutCubic(float p)
    {
        float f = p - 0.5f;
        float x = Math.Abs(f);
        return ((4f * x - 6f) * x + 3f) * f + 0.5f;
    }

    /// <summary>
    ///     Modeled after the piecewise expoly-damped sine wave:
    ///     y = (1/2)*sin(13pi/2*(2*x))*Math.Pow(2, 10 * ((2*x) - 1))      ; [0,0.5)
    ///     y = (1/2)*(sin(-13pi/2*((2x-1)+1))*Math.Pow(2,-10(2*x-1)) + 2) ; [0.5, 1]
    /// </summary>
    public static float EaseInOutElastic(float p)
    {
        if (p < 0.5f)
        {
            return 0.5f * Math.Sin(13 * HALFPI * (2 * p)) * Math.Pow(2, 10 * ((2 * p) - 1));
        }

        return 0.5f * ((Math.Sin(-13 * HALFPI * (2 * p)) * Math.Pow(2, -10 * ((2 * p) - 1))) + 2);
    }

    /// <summary>
    ///     Modeled after the piecewise expo
    ///     y = (1/2)2^(10(2x - 1))         ; [0,0.5)
    ///     y = -(1/2)*2^(-10(2x - 1))) + 1 ; [0.5,1]
    /// </summary>
    public static float EaseInOutExpo(float p)
    {
        //p = Math.Clamp01(p);
        if(p > 1f)
        {
            return p;
        }

        p = p * 20f - 10f;
        const float S = 512f / 1023f;

        // Left half
        if(p < 0f)
        {
            return 0.5f - (S - S * Math.Pow(2f, p));
        }

        // Right half
        return 0.5f + (S - S * Math.Pow(2f, -p));
    }

    /// <summary>
    ///     A slightly faster smoothstep
    /// </summary>
    public static float EaseInOutQuad(float p)
    {
        float x = p - 0.5f;
        return (x - x * Math.Abs(x)) * 2f + 0.5f;
    }

    /// <summary>
    ///     Like a half pipe from 0 - 0.5, leading to an upside down half pipe from 0.5 - 1
    /// </summary>
    public static float EaseInOutQuart(float p)
    {
        float t = 0f;
        float f = p - 0.5f;
        float x = Math.Abs(f);
        t = x * -8f + 16f;
        t = t * x - 12f;
        t = t * x + 4f;
        return t * f + 0.5f;
    }

    /// <summary>
    ///     Modeled after the piecewise quint
    ///     y = (1/2)((2x)^5)       ; [0, 0.5)
    ///     y = (1/2)((2x-2)^5 + 2) ; [0.5, 1]
    /// </summary>
    public static float EaseInOutQuint(float p)
    {
        float t = 0f;
        float f = p - 0.5f;
        float x = Math.Abs(f);
        t = x * 16f - 40f;
        t = t * x + 40f;
        t = t * x - 20f;
        t = t * x + 5f;
        return t * f + 0.5f;
    }

    /// <summary>
    ///     Modeled after half sine wave
    /// </summary>
    public static float EaseInOutSine(float p)
    {
        float f = Math.Sin(HALFPI * p);
        return f * f;
    }

    /// <summary>
    ///     Modeled after the parabola y = x^2
    /// </summary>
    public static float EaseInQuad(float p)
    {
        return p * p;
    }

    /// <summary>
    ///     Modeled after the quart x^4
    /// </summary>
    public static float EaseInQuart(float p)
    {
        return p * p * p * p;
    }

    /// <summary>
    ///     Modeled after the quint y = x^5
    /// </summary>
    public static float EaseInQuint(float p)
    {
        return p * p * p * p * p;
    }

    /// <summary>
    ///     An upside-down cosine wave starting at 0, hitting 1, and peaking at 2
    /// </summary>
    public static float EaseInSine(float p)
    {
        return 1f - Math.Cos(HALFPI * p);
    }

    /// <summary>
    ///     Modeled after a straight line y = x
    /// </summary>
    public static float EaseLinear(float p)
    {
        return p;
    }

    /// <summary>
    ///     Modeled after overshooting cubic y = 1-((1-x)^3-(1-x)*sin((1-x)*pi))
    /// </summary>
    public static float EaseOutBack(float p)
    {
        float f = 1 - p;
        return 1 - ((f * f * f) - (f * Math.Sin(f * PI)));
    }

    /// <summary>
    ///     Bouncing from y=0 up to y=1. Like the Solitaire card bouncing when you win
    ///     Collision points: 4/11, 8/11, 9/11, 1.0
    /// </summary>
    public static float EaseOutBounce(float p)
    {
        // First wave
        float a = (121f / 16f) * p * p;
        float x = a;

        // Second wave
        float q1 = p - (6f/11f);
        float b = (363f / 40f) * q1 * q1 + (7f / 10f);
        x = (b < x) ? b : x;

        // Third wave
        float q2 = p - (179f/220f);
        float c = (4356f / 361f) * q2 * q2 + (91f / 100f);
        x = (c < x) ? c : x;

        // Fourth wave
        float q3 = p - (19f/20f);
        float d = (54f / 5f) * q3 * q3 + (973f / 1000f);
        x = (d < x) ? d : x;

        return x;
    }

    /// <summary>
    ///     Modeled after the top half of a unit circle centered at (x:1, y:0)
    /// </summary>
    public static float EaseOutCirc(float p)
    {
        return Math.Sqrt(Math.Max(Math.Epsilon, (2f - p) * p));
    }

    /// <summary>
    ///     Modeled after the cubic y = 1 - (1 - x)^3
    /// </summary>
    public static float EaseOutCubic(float p)
    {
        float f = 1f - p;
        return 1f - (f * f * f);
    }

    /// <summary>
    ///     Modeled after the damped sine wave y = sin(-13pi/2*(x + 1))*Math.Pow(2, -10x) + 1
    /// </summary>
    public static float EaseOutElastic(float p)
    {
        return (Math.Sin(-13 * HALFPI * (p + 1)) * Math.Pow(2, -10 * p)) + 1;
    }

    /// <summary>
    ///     Very sharp climb from 0 to 1, with an exponential slow down. Like a rocket landing on y=1
    /// </summary>
    public static float EaseOutExpo(float p)
    {
        const float S = 1024f / 1023f;
        return p > 1.0f ? p : (S - S * Math.Pow(2f, -10f * p));
    }

    /// <summary>
    ///     Modeled after the parabola y = 2x - x^2
    /// </summary>
    public static float EaseOutQuad(float p)
    {
        return (2f - p) * p;
    }

    /// <summary>
    ///     Modeled after the quart y = 1 - (1 - x)^4
    /// </summary>
    public static float EaseOutQuart(float p)
    {
        float f = 1f - p;
        return 1f - (f * f * f * f);
    }

    /// <summary>
    ///     Modeled after the quint y = 1 - (1 - x)^5
    /// </summary>
    public static float EaseOutQuint(float p)
    {
        float f = 1f - p;
        return 1f - (f * f * f * f * f);
    }

    /// <summary>
    ///     Modeled after a sine wave, starting at 0, peaking at 1
    /// </summary>
    public static float EaseOutSine(float p)
    {
        return Math.Sin(p * HALFPI);
    }

    /// <summary>
    ///     Always rounds down to a whole number
    /// </summary>
    public static float EaseStep(float p)
    {
        return Math.Floor(p);
    }

    /// <summary>
    ///     Interpolate using the specified function.
    /// </summary>
    public static float Interpolate(float p, Functions function)
    {
        return function switch
        {
            Functions.easeStep => EaseStep(p),
            Functions.easeOutQuad => EaseOutQuad(p),
            Functions.easeInQuad => EaseInQuad(p),
            Functions.easeInOutQuad => EaseInOutQuad(p),
            Functions.easeInCubic => EaseInCubic(p),
            Functions.easeOutCubic => EaseOutCubic(p),
            Functions.easeInOutCubic => EaseInOutCubic(p),
            Functions.easeInQuart => EaseInQuart(p),
            Functions.easeOutQuart => EaseOutQuart(p),
            Functions.easeInOutQuart => EaseInOutQuart(p),
            Functions.easeInQuint => EaseInQuint(p),
            Functions.easeOutQuint => EaseOutQuint(p),
            Functions.easeInOutQuint => EaseInOutQuint(p),
            Functions.easeInSine => EaseInSine(p),
            Functions.easeOutSine => EaseOutSine(p),
            Functions.easeInOutSine => EaseInOutSine(p),
            Functions.easeInCirc => EaseInCirc(p),
            Functions.easeOutCirc => EaseOutCirc(p),
            Functions.easeInOutCirc => EaseInOutCirc(p),
            Functions.easeInExpo => EaseInExpo(p),
            Functions.easeOutExpo => EaseOutExpo(p),
            Functions.easeInOutExpo => EaseInOutExpo(p),
            Functions.easeInElastic => EaseInElastic(p),
            Functions.easeOutElastic => EaseOutElastic(p),
            Functions.easeInOutElastic => EaseInOutElastic(p),
            Functions.easeInBack => EaseInBack(p),
            Functions.easeOutBack => EaseOutBack(p),
            Functions.easeInOutBack => EaseInOutBack(p),
            Functions.easeInBounce => EaseInBounce(p),
            Functions.easeOutBounce => EaseOutBounce(p),
            Functions.easeInOutBounce => EaseInOutBounce(p),
            _ => EaseLinear(p)
        };
    }
}
