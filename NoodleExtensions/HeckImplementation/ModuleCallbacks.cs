using System.Linq;
using Heck;
using static NoodleExtensions.NoodleController;

namespace NoodleExtensions
{
    internal class ModuleCallbacks
    {
        [ModuleCondition]
        private static bool Condition(Capabilities capabilities)
        {
            return capabilities.Requirements.Contains(CAPABILITY);
        }

        [ModuleCallback]
        private static void Toggle(bool value)
        {
            FeaturesPatcher.Enabled = value;
            Deserializer.Enabled = value;
        }
    }
}
