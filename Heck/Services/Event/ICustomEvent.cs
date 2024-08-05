using CustomJSONData.CustomBeatmap;

namespace Heck.Event;

public interface ICustomEvent
{
    public void Callback(CustomEventData customEventData);
}
