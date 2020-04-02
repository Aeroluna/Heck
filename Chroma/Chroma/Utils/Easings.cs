using Math = UnityEngine.Mathf;

namespace Chroma.Utils
{
    static internal class Easings
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
        /// Easing Functions enumeration
        /// </summary>
        internal enum Functions
        {
            easeLinear,
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

        /// <summary>
        /// Interpolate using the specified function.
        /// </summary>
        static internal float Interpolate(float p, Functions function)
        {
            switch (function)
            {
                default:
                case Functions.easeLinear: return easeLinear(p);
                case Functions.easeOutQuad: return easeOutQuad(p);
                case Functions.easeInQuad: return easeInQuad(p);
                case Functions.easeInOutQuad: return easeInOutQuad(p);
                case Functions.easeInCubic: return easeInCubic(p);
                case Functions.easeOutCubic: return easeOutCubic(p);
                case Functions.easeInOutCubic: return easeInOutCubic(p);
                case Functions.easeInQuart: return easeInQuart(p);
                case Functions.easeOutQuart: return easeOutQuart(p);
                case Functions.easeInOutQuart: return easeInOutQuart(p);
                case Functions.easeInQuint: return easeInQuint(p);
                case Functions.easeOutQuint: return easeOutQuint(p);
                case Functions.easeInOutQuint: return easeInOutQuint(p);
                case Functions.easeInSine: return easeInSine(p);
                case Functions.easeOutSine: return easeOutSine(p);
                case Functions.easeInOutSine: return easeInOutSine(p);
                case Functions.easeInCirc: return easeInCirc(p);
                case Functions.easeOutCirc: return easeOutCirc(p);
                case Functions.easeInOutCirc: return easeInOutCirc(p);
                case Functions.easeInExpo: return easeInExpo(p);
                case Functions.easeOutExpo: return easeOutExpo(p);
                case Functions.easeInOutExpo: return easeInOutExpo(p);
                case Functions.easeInElastic: return easeInElastic(p);
                case Functions.easeOutElastic: return easeOutElastic(p);
                case Functions.easeInOutElastic: return easeInOutElastic(p);
                case Functions.easeInBack: return easeInBack(p);
                case Functions.easeOutBack: return easeOutBack(p);
                case Functions.easeInOutBack: return easeInOutBack(p);
                case Functions.easeInBounce: return easeInBounce(p);
                case Functions.easeOutBounce: return easeOutBounce(p);
                case Functions.easeInOutBounce: return easeInOutBounce(p);
            }
        }

        /// <summary>
        /// Modeled after the line y = x
        /// </summary>
        static internal float easeLinear(float p)
        {
            return p;
        }

        /// <summary>
        /// Modeled after the parabola y = x^2
        /// </summary>
        static internal float easeInQuad(float p)
        {
            return p * p;
        }

        /// <summary>
        /// Modeled after the parabola y = -x^2 + 2x
        /// </summary>
        static internal float easeOutQuad(float p)
        {
            return -(p * (p - 2));
        }

        /// <summary>
        /// Modeled after the piecewise quad
        /// y = (1/2)((2x)^2)             ; [0, 0.5)
        /// y = -(1/2)((2x-1)*(2x-3) - 1) ; [0.5, 1]
        /// </summary>
        static internal float easeInOutQuad(float p)
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
        static internal float easeInCubic(float p)
        {
            return p * p * p;
        }

        /// <summary>
        /// Modeled after the cubic y = (x - 1)^3 + 1
        /// </summary>
        static internal float easeOutCubic(float p)
        {
            float f = (p - 1);
            return f * f * f + 1;
        }

        /// <summary>
        /// Modeled after the piecewise cubic
        /// y = (1/2)((2x)^3)       ; [0, 0.5)
        /// y = (1/2)((2x-2)^3 + 2) ; [0.5, 1]
        /// </summary>
        static internal float easeInOutCubic(float p)
        {
            if (p < 0.5f)
            {
                return 4 * p * p * p;
            }
            else
            {
                float f = ((2 * p) - 2);
                return 0.5f * f * f * f + 1;
            }
        }

        /// <summary>
        /// Modeled after the quart x^4
        /// </summary>
        static internal float easeInQuart(float p)
        {
            return p * p * p * p;
        }

        /// <summary>
        /// Modeled after the quart y = 1 - (x - 1)^4
        /// </summary>
        static internal float easeOutQuart(float p)
        {
            float f = (p - 1);
            return f * f * f * (1 - p) + 1;
        }

        /// <summary>
        // Modeled after the piecewise quart
        // y = (1/2)((2x)^4)        ; [0, 0.5)
        // y = -(1/2)((2x-2)^4 - 2) ; [0.5, 1]
        /// </summary>
        static internal float easeInOutQuart(float p)
        {
            if (p < 0.5f)
            {
                return 8 * p * p * p * p;
            }
            else
            {
                float f = (p - 1);
                return -8 * f * f * f * f + 1;
            }
        }

        /// <summary>
        /// Modeled after the quint y = x^5
        /// </summary>
        static internal float easeInQuint(float p)
        {
            return p * p * p * p * p;
        }

        /// <summary>
        /// Modeled after the quint y = (x - 1)^5 + 1
        /// </summary>
        static internal float easeOutQuint(float p)
        {
            float f = (p - 1);
            return f * f * f * f * f + 1;
        }

        /// <summary>
        /// Modeled after the piecewise quint
        /// y = (1/2)((2x)^5)       ; [0, 0.5)
        /// y = (1/2)((2x-2)^5 + 2) ; [0.5, 1]
        /// </summary>
        static internal float easeInOutQuint(float p)
        {
            if (p < 0.5f)
            {
                return 16 * p * p * p * p * p;
            }
            else
            {
                float f = ((2 * p) - 2);
                return 0.5f * f * f * f * f * f + 1;
            }
        }

        /// <summary>
        /// Modeled after quarter-cycle of sine wave
        /// </summary>
        static internal float easeInSine(float p)
        {
            return Math.Sin((p - 1) * HALFPI) + 1;
        }

        /// <summary>
        /// Modeled after quarter-cycle of sine wave (different phase)
        /// </summary>
        static internal float easeOutSine(float p)
        {
            return Math.Sin(p * HALFPI);
        }

        /// <summary>
        /// Modeled after half sine wave
        /// </summary>
        static internal float easeInOutSine(float p)
        {
            return 0.5f * (1 - Math.Cos(p * PI));
        }

        /// <summary>
        /// Modeled after shifted quadrant IV of unit circle
        /// </summary>
        static internal float easeInCirc(float p)
        {
            return 1 - Math.Sqrt(1 - (p * p));
        }

        /// <summary>
        /// Modeled after shifted quadrant II of unit circle
        /// </summary>
        static internal float easeOutCirc(float p)
        {
            return Math.Sqrt((2 - p) * p);
        }

        /// <summary>
        /// Modeled after the piecewise circ function
        /// y = (1/2)(1 - Math.Sqrt(1 - 4x^2))           ; [0, 0.5)
        /// y = (1/2)(Math.Sqrt(-(2x - 3)*(2x - 1)) + 1) ; [0.5, 1]
        /// </summary>
        static internal float easeInOutCirc(float p)
        {
            if (p < 0.5f)
            {
                return 0.5f * (1 - Math.Sqrt(1 - 4 * (p * p)));
            }
            else
            {
                return 0.5f * (Math.Sqrt(-((2 * p) - 3) * ((2 * p) - 1)) + 1);
            }
        }

        /// <summary>
        /// Modeled after the expo function y = 2^(10(x - 1))
        /// </summary>
        static internal float easeInExpo(float p)
        {
            return (p == 0.0f) ? p : Math.Pow(2, 10 * (p - 1));
        }

        /// <summary>
        /// Modeled after the expo function y = -2^(-10x) + 1
        /// </summary>
        static internal float easeOutExpo(float p)
        {
            return (p == 1.0f) ? p : 1 - Math.Pow(2, -10 * p);
        }

        /// <summary>
        /// Modeled after the piecewise expo
        /// y = (1/2)2^(10(2x - 1))         ; [0,0.5)
        /// y = -(1/2)*2^(-10(2x - 1))) + 1 ; [0.5,1]
        /// </summary>
        static internal float easeInOutExpo(float p)
        {
            if (p == 0.0 || p == 1.0) return p;

            if (p < 0.5f)
            {
                return 0.5f * Math.Pow(2, (20 * p) - 10);
            }
            else
            {
                return -0.5f * Math.Pow(2, (-20 * p) + 10) + 1;
            }
        }

        /// <summary>
        /// Modeled after the damped sine wave y = sin(13pi/2*x)*Math.Pow(2, 10 * (x - 1))
        /// </summary>
        static internal float easeInElastic(float p)
        {
            return Math.Sin(13 * HALFPI * p) * Math.Pow(2, 10 * (p - 1));
        }

        /// <summary>
        /// Modeled after the damped sine wave y = sin(-13pi/2*(x + 1))*Math.Pow(2, -10x) + 1
        /// </summary>
        static internal float easeOutElastic(float p)
        {
            return Math.Sin(-13 * HALFPI * (p + 1)) * Math.Pow(2, -10 * p) + 1;
        }

        /// <summary>
        /// Modeled after the piecewise expoly-damped sine wave:
        /// y = (1/2)*sin(13pi/2*(2*x))*Math.Pow(2, 10 * ((2*x) - 1))      ; [0,0.5)
        /// y = (1/2)*(sin(-13pi/2*((2x-1)+1))*Math.Pow(2,-10(2*x-1)) + 2) ; [0.5, 1]
        /// </summary>
        static internal float easeInOutElastic(float p)
        {
            if (p < 0.5f)
            {
                return 0.5f * Math.Sin(13 * HALFPI * (2 * p)) * Math.Pow(2, 10 * ((2 * p) - 1));
            }
            else
            {
                return 0.5f * (Math.Sin(-13 * HALFPI * ((2 * p - 1) + 1)) * Math.Pow(2, -10 * (2 * p - 1)) + 2);
            }
        }

        /// <summary>
        /// Modeled after the overshooting cubic y = x^3-x*sin(x*pi)
        /// </summary>
        static internal float easeInBack(float p)
        {
            return p * p * p - p * Math.Sin(p * PI);
        }

        /// <summary>
        /// Modeled after overshooting cubic y = 1-((1-x)^3-(1-x)*sin((1-x)*pi))
        /// </summary>
        static internal float easeOutBack(float p)
        {
            float f = (1 - p);
            return 1 - (f * f * f - f * Math.Sin(f * PI));
        }

        /// <summary>
        /// Modeled after the piecewise overshooting cubic function:
        /// y = (1/2)*((2x)^3-(2x)*sin(2*x*pi))           ; [0, 0.5)
        /// y = (1/2)*(1-((1-x)^3-(1-x)*sin((1-x)*pi))+1) ; [0.5, 1]
        /// </summary>
        static internal float easeInOutBack(float p)
        {
            if (p < 0.5f)
            {
                float f = 2 * p;
                return 0.5f * (f * f * f - f * Math.Sin(f * PI));
            }
            else
            {
                float f = (1 - (2 * p - 1));
                return 0.5f * (1 - (f * f * f - f * Math.Sin(f * PI))) + 0.5f;
            }
        }

        /// <summary>
        /// </summary>
        static internal float easeInBounce(float p)
        {
            return 1 - easeOutBounce(1 - p);
        }

        /// <summary>
        /// </summary>
        static internal float easeOutBounce(float p)
        {
            if (p < 4 / 11.0f)
            {
                return (121 * p * p) / 16.0f;
            }
            else if (p < 8 / 11.0f)
            {
                return (363 / 40.0f * p * p) - (99 / 10.0f * p) + 17 / 5.0f;
            }
            else if (p < 9 / 10.0f)
            {
                return (4356 / 361.0f * p * p) - (35442 / 1805.0f * p) + 16061 / 1805.0f;
            }
            else
            {
                return (54 / 5.0f * p * p) - (513 / 25.0f * p) + 268 / 25.0f;
            }
        }

        /// <summary>
        /// </summary>
        static internal float easeInOutBounce(float p)
        {
            if (p < 0.5f)
            {
                return 0.5f * easeInBounce(p * 2);
            }
            else
            {
                return 0.5f * easeOutBounce(p * 2 - 1) + 0.5f;
            }
        }
    }
}