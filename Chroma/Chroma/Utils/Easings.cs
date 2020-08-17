namespace Chroma.Utils
{
    using Math = UnityEngine.Mathf;

    /// <summary>
    /// Easing Functions enumeration
    /// </summary>
#pragma warning disable SA1300 // Element should begin with upper-case letter
    internal enum Functions
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
        easeInOutBounce,
    }
#pragma warning restore SA1300 // Element should begin with upper-case letter

    internal static class Easings
    {
        /// <summary>
        /// Constant Pi.
        /// </summary>
        private const float PI = Math.PI;

        /// <summary>
        /// Constant Pi / 2.
        /// </summary>
        private const float HALFPI = Math.PI / 2.0f;

        /// <summary>
        /// Interpolate using the specified function.
        /// </summary>
        internal static float Interpolate(float p, Functions function)
        {
            switch (function)
            {
                default:
                case Functions.easeLinear: return EaseLinear(p);
                case Functions.easeStep: return EaseStep(p);
                case Functions.easeOutQuad: return EaseOutQuad(p);
                case Functions.easeInQuad: return EaseInQuad(p);
                case Functions.easeInOutQuad: return EaseInOutQuad(p);
                case Functions.easeInCubic: return EaseInCubic(p);
                case Functions.easeOutCubic: return EaseOutCubic(p);
                case Functions.easeInOutCubic: return EaseInOutCubic(p);
                case Functions.easeInQuart: return EaseInQuart(p);
                case Functions.easeOutQuart: return EaseOutQuart(p);
                case Functions.easeInOutQuart: return EaseInOutQuart(p);
                case Functions.easeInQuint: return EaseInQuint(p);
                case Functions.easeOutQuint: return EaseOutQuint(p);
                case Functions.easeInOutQuint: return EaseInOutQuint(p);
                case Functions.easeInSine: return EaseInSine(p);
                case Functions.easeOutSine: return EaseOutSine(p);
                case Functions.easeInOutSine: return EaseInOutSine(p);
                case Functions.easeInCirc: return EaseInCirc(p);
                case Functions.easeOutCirc: return EaseOutCirc(p);
                case Functions.easeInOutCirc: return EaseInOutCirc(p);
                case Functions.easeInExpo: return EaseInExpo(p);
                case Functions.easeOutExpo: return EaseOutExpo(p);
                case Functions.easeInOutExpo: return EaseInOutExpo(p);
                case Functions.easeInElastic: return EaseInElastic(p);
                case Functions.easeOutElastic: return EaseOutElastic(p);
                case Functions.easeInOutElastic: return EaseInOutElastic(p);
                case Functions.easeInBack: return EaseInBack(p);
                case Functions.easeOutBack: return EaseOutBack(p);
                case Functions.easeInOutBack: return EaseInOutBack(p);
                case Functions.easeInBounce: return EaseInBounce(p);
                case Functions.easeOutBounce: return EaseOutBounce(p);
                case Functions.easeInOutBounce: return EaseInOutBounce(p);
            }
        }

        /// <summary>
        /// Modeled after the line y = x
        /// </summary>
        internal static float EaseLinear(float p)
        {
            return p;
        }

        /// <summary>
        /// It's either 1, or it's not
        /// </summary>
        internal static float EaseStep(float p)
        {
            return Math.Floor(p);
        }

        /// <summary>
        /// Modeled after the parabola y = x^2
        /// </summary>
        internal static float EaseInQuad(float p)
        {
            return p * p;
        }

        /// <summary>
        /// Modeled after the parabola y = -x^2 + 2x
        /// </summary>
        internal static float EaseOutQuad(float p)
        {
            return -(p * (p - 2));
        }

        /// <summary>
        /// Modeled after the piecewise quad
        /// y = (1/2)((2x)^2)             ; [0, 0.5)
        /// y = -(1/2)((2x-1)*(2x-3) - 1) ; [0.5, 1]
        /// </summary>
        internal static float EaseInOutQuad(float p)
        {
            if (p < 0.5f)
            {
                return 2 * p * p;
            }
            else
            {
                return (-2 * p * p) + (4 * p) - 1;
            }
        }

        /// <summary>
        /// Modeled after the cubic y = x^3
        /// </summary>
        internal static float EaseInCubic(float p)
        {
            return p * p * p;
        }

        /// <summary>
        /// Modeled after the cubic y = (x - 1)^3 + 1
        /// </summary>
        internal static float EaseOutCubic(float p)
        {
            float f = p - 1;
            return (f * f * f) + 1;
        }

        /// <summary>
        /// Modeled after the piecewise cubic
        /// y = (1/2)((2x)^3)       ; [0, 0.5)
        /// y = (1/2)((2x-2)^3 + 2) ; [0.5, 1]
        /// </summary>
        internal static float EaseInOutCubic(float p)
        {
            if (p < 0.5f)
            {
                return 4 * p * p * p;
            }
            else
            {
                float f = (2 * p) - 2;
                return (0.5f * f * f * f) + 1;
            }
        }

        /// <summary>
        /// Modeled after the quart x^4
        /// </summary>
        internal static float EaseInQuart(float p)
        {
            return p * p * p * p;
        }

        /// <summary>
        /// Modeled after the quart y = 1 - (x - 1)^4
        /// </summary>
        internal static float EaseOutQuart(float p)
        {
            float f = p - 1;
            return (f * f * f * (1 - p)) + 1;
        }

        /// <summary>
        /// Modeled after the piecewise quart
        /// y = (1/2)((2x)^4)        ; [0, 0.5)
        /// y = -(1/2)((2x-2)^4 - 2) ; [0.5, 1]
        /// </summary>
        internal static float EaseInOutQuart(float p)
        {
            if (p < 0.5f)
            {
                return 8 * p * p * p * p;
            }
            else
            {
                float f = p - 1;
                return (-8 * f * f * f * f) + 1;
            }
        }

        /// <summary>
        /// Modeled after the quint y = x^5
        /// </summary>
        internal static float EaseInQuint(float p)
        {
            return p * p * p * p * p;
        }

        /// <summary>
        /// Modeled after the quint y = (x - 1)^5 + 1
        /// </summary>
        internal static float EaseOutQuint(float p)
        {
            float f = p - 1;
            return (f * f * f * f * f) + 1;
        }

        /// <summary>
        /// Modeled after the piecewise quint
        /// y = (1/2)((2x)^5)       ; [0, 0.5)
        /// y = (1/2)((2x-2)^5 + 2) ; [0.5, 1]
        /// </summary>
        internal static float EaseInOutQuint(float p)
        {
            if (p < 0.5f)
            {
                return 16 * p * p * p * p * p;
            }
            else
            {
                float f = (2 * p) - 2;
                return (0.5f * f * f * f * f * f) + 1;
            }
        }

        /// <summary>
        /// Modeled after quarter-cycle of sine wave
        /// </summary>
        internal static float EaseInSine(float p)
        {
            return Math.Sin((p - 1) * HALFPI) + 1;
        }

        /// <summary>
        /// Modeled after quarter-cycle of sine wave (different phase)
        /// </summary>
        internal static float EaseOutSine(float p)
        {
            return Math.Sin(p * HALFPI);
        }

        /// <summary>
        /// Modeled after half sine wave
        /// </summary>
        internal static float EaseInOutSine(float p)
        {
            return 0.5f * (1 - Math.Cos(p * PI));
        }

        /// <summary>
        /// Modeled after shifted quadrant IV of unit circle
        /// </summary>
        internal static float EaseInCirc(float p)
        {
            return 1 - Math.Sqrt(1 - (p * p));
        }

        /// <summary>
        /// Modeled after shifted quadrant II of unit circle
        /// </summary>
        internal static float EaseOutCirc(float p)
        {
            return Math.Sqrt((2 - p) * p);
        }

        /// <summary>
        /// Modeled after the piecewise circ function
        /// y = (1/2)(1 - Math.Sqrt(1 - 4x^2))           ; [0, 0.5)
        /// y = (1/2)(Math.Sqrt(-(2x - 3)*(2x - 1)) + 1) ; [0.5, 1]
        /// </summary>
        internal static float EaseInOutCirc(float p)
        {
            if (p < 0.5f)
            {
                return 0.5f * (1 - Math.Sqrt(1 - (4 * (p * p))));
            }
            else
            {
                return 0.5f * (Math.Sqrt(-((2 * p) - 3) * ((2 * p) - 1)) + 1);
            }
        }

        /// <summary>
        /// Modeled after the expo function y = 2^(10(x - 1))
        /// </summary>
        internal static float EaseInExpo(float p)
        {
            return (p == 0.0f) ? p : Math.Pow(2, 10 * (p - 1));
        }

        /// <summary>
        /// Modeled after the expo function y = -2^(-10x) + 1
        /// </summary>
        internal static float EaseOutExpo(float p)
        {
            return (p == 1.0f) ? p : 1 - Math.Pow(2, -10 * p);
        }

        /// <summary>
        /// Modeled after the piecewise expo
        /// y = (1/2)2^(10(2x - 1))         ; [0,0.5)
        /// y = -(1/2)*2^(-10(2x - 1))) + 1 ; [0.5,1]
        /// </summary>
        internal static float EaseInOutExpo(float p)
        {
            if (p == 0.0 || p == 1.0)
            {
                return p;
            }

            if (p < 0.5f)
            {
                return 0.5f * Math.Pow(2, (20 * p) - 10);
            }
            else
            {
                return (-0.5f * Math.Pow(2, (-20 * p) + 10)) + 1;
            }
        }

        /// <summary>
        /// Modeled after the damped sine wave y = sin(13pi/2*x)*Math.Pow(2, 10 * (x - 1))
        /// </summary>
        internal static float EaseInElastic(float p)
        {
            return Math.Sin(13 * HALFPI * p) * Math.Pow(2, 10 * (p - 1));
        }

        /// <summary>
        /// Modeled after the damped sine wave y = sin(-13pi/2*(x + 1))*Math.Pow(2, -10x) + 1
        /// </summary>
        internal static float EaseOutElastic(float p)
        {
            return (Math.Sin(-13 * HALFPI * (p + 1)) * Math.Pow(2, -10 * p)) + 1;
        }

        /// <summary>
        /// Modeled after the piecewise expoly-damped sine wave:
        /// y = (1/2)*sin(13pi/2*(2*x))*Math.Pow(2, 10 * ((2*x) - 1))      ; [0,0.5)
        /// y = (1/2)*(sin(-13pi/2*((2x-1)+1))*Math.Pow(2,-10(2*x-1)) + 2) ; [0.5, 1]
        /// </summary>
        internal static float EaseInOutElastic(float p)
        {
            if (p < 0.5f)
            {
                return 0.5f * Math.Sin(13 * HALFPI * (2 * p)) * Math.Pow(2, 10 * ((2 * p) - 1));
            }
            else
            {
                return 0.5f * ((Math.Sin(-13 * HALFPI * (2 * p)) * Math.Pow(2, -10 * ((2 * p) - 1))) + 2);
            }
        }

        /// <summary>
        /// Modeled after the overshooting cubic y = x^3-x*sin(x*pi)
        /// </summary>
        internal static float EaseInBack(float p)
        {
            return (p * p * p) - (p * Math.Sin(p * PI));
        }

        /// <summary>
        /// Modeled after overshooting cubic y = 1-((1-x)^3-(1-x)*sin((1-x)*pi))
        /// </summary>
        internal static float EaseOutBack(float p)
        {
            float f = 1 - p;
            return 1 - ((f * f * f) - (f * Math.Sin(f * PI)));
        }

        /// <summary>
        /// Modeled after the piecewise overshooting cubic function:
        /// y = (1/2)*((2x)^3-(2x)*sin(2*x*pi))           ; [0, 0.5)
        /// y = (1/2)*(1-((1-x)^3-(1-x)*sin((1-x)*pi))+1) ; [0.5, 1]
        /// </summary>
        internal static float EaseInOutBack(float p)
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

        internal static float EaseInBounce(float p)
        {
            return 1 - EaseOutBounce(1 - p);
        }

        internal static float EaseOutBounce(float p)
        {
            if (p < 4 / 11.0f)
            {
                return 121 * p * p / 16.0f;
            }
            else if (p < 8 / 11.0f)
            {
                return (363 / 40.0f * p * p) - (99 / 10.0f * p) + (17 / 5.0f);
            }
            else if (p < 9 / 10.0f)
            {
                return (4356 / 361.0f * p * p) - (35442 / 1805.0f * p) + (16061 / 1805.0f);
            }
            else
            {
                return (54 / 5.0f * p * p) - (513 / 25.0f * p) + (268 / 25.0f);
            }
        }

        internal static float EaseInOutBounce(float p)
        {
            if (p < 0.5f)
            {
                return 0.5f * EaseInBounce(p * 2);
            }
            else
            {
                return (0.5f * EaseOutBounce((p * 2) - 1)) + 0.5f;
            }
        }
    }
}
