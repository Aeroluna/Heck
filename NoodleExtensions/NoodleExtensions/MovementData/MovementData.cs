namespace NoodleExtensions
{
    internal abstract class MovementData
    {
        internal float time { get; }
        internal float duration { get; }

        protected MovementData(float time, float duration)
        {
            this.time = time;
            this.duration = duration;
        }
    }
}