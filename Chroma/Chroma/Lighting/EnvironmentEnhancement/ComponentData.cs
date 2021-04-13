namespace Chroma
{
    internal interface IComponentData
    {
    }

    internal class TrackLaneRingsManagerComponentData : IComponentData
    {
        internal TrackLaneRingsManager OldTrackLaneRingsManager { get; set; }

        internal TrackLaneRingsManager NewTrackLaneRingsManager { get; set; }
    }
}
