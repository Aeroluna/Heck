using System.Collections.Generic;
using System.Text;

namespace Heck.Animation
{
    public interface IPointDefinition
    {
    }

    public class PointDefinition<T> : IPointDefinition
        where T : struct
    {
        private readonly List<PointData> _points;

        public PointDefinition()
            : this(new List<PointData>())
        {
        }

        internal PointDefinition(List<PointData> points)
        {
            _points = points;
        }

        internal delegate T InterpolationHandler(List<PointData> points, int l, int r, float time);

        public override string ToString()
        {
            StringBuilder stringBuilder = new("{ ");
            _points.ForEach(n => stringBuilder.Append($"{n.Point} "));

            stringBuilder.Append('}');
            return stringBuilder.ToString();
        }

        internal T Interpolate(float time, InterpolationHandler func, out bool last)
        {
            last = false;
            int count = _points.Count;
            if (count == 0)
            {
                return default;
            }

            PointData firstPoint = _points[0];
            if (firstPoint.Time >= time)
            {
                return firstPoint.Point;
            }

            PointData lastPoint = _points[count - 1];
            if (lastPoint.Time <= time)
            {
                last = true;
                return lastPoint.Point;
            }

            SearchIndex(time, out int l, out int r);
            PointData pointL = _points[l];
            PointData pointR = _points[r];

            float normalTime;
            float divisor = pointR.Time - pointL.Time;
            if (divisor != 0)
            {
                normalTime = (time - pointL.Time) / divisor;
            }
            else
            {
                normalTime = 0;
            }

            normalTime = Easings.Interpolate(normalTime, pointR.Easing);

            return func(_points, l, r, normalTime);
        }

        // Use binary search instead of linear search.
        private void SearchIndex(float time, out int l, out int r)
        {
            l = 0;
            r = _points.Count;

            while (l < r - 1)
            {
                int m = (l + r) / 2;
                float pointTime = _points[m].Time;

                if (pointTime < time)
                {
                    l = m;
                }
                else
                {
                    r = m;
                }
            }
        }

        internal class PointData
        {
            internal PointData(T point, float time, Functions easing, bool smooth = false, bool hsvLerp = false)
            {
                Point = point;
                Time = time;
                Easing = easing;
                Smooth = smooth;
                HsvLerp = hsvLerp;
            }

            internal T Point { get; }

            internal float Time { get; }

            internal Functions Easing { get; }

            internal bool Smooth { get; }

            internal bool HsvLerp { get; }
        }
    }
}
