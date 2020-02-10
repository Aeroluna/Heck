namespace Chroma.Beatmap
{
    public class CustomBeatmapBarrier : CustomBeatmapObject
    {
        private ObstacleData _obstacle;

        public BeatmapObjectData Obstacle
        {
            get { return _obstacle; }
        }

        public CustomBeatmapBarrier(ObstacleData obstacle) : base(obstacle)
        {
            this._obstacle = obstacle;
        }
    }
}