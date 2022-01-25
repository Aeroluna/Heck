using static Heck.HeckController;

namespace Heck
{
    internal class ModuleCallbacks
    {
        [ModuleCallback]
        private static void Toggle(bool value)
        {
            FeaturesPatcher.Enabled = value;
            Deserializer.Enabled = value;
        }
    }
}
