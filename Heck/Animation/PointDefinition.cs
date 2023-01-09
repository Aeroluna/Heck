using System.Collections.Generic;
using System.Text;

namespace Heck.Animation
{
    public interface IPointDefinition
    {
        public int Count { get; }
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
            Count = points.Count;
        }

        internal delegate T InterpolationHandler(List<PointData> points, int l, int r, float time);

        public int Count { get; }

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
            if (Count == 0)
            {
                return default;
            }

            PointData lastPoint = _points[Count - 1];
            if (lastPoint.Time <= time)
            {
                last = true;
                return lastPoint.Point;
            }

            PointData firstPoint = _points[0];
            if (firstPoint.Time >= time)
            {
                return firstPoint.Point;
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
            r = Count;

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
