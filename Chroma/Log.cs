using Heck;

namespace Chroma
{
    // its here cause its easier to type Log.Logger than HeckController.Logger
    internal static class Log
    {
        internal static HeckLogger Logger { get; set; } = null!;
    }
}
