using System.Linq;
using Heck;
using static NoodleExtensions.NoodleController;

namespace NoodleExtensions
{
    [Module(ID, 2, LoadType.Active, new[] { "Heck" })]
    [ModulePatcher(HARMONY_ID + "Features", PatchType.Features)]
    [ModuleDataDeserializer(ID, typeof(CustomDataManager))]
    internal class FeaturesModule : IModule
    {
        internal bool Active { get; private set; }

        [ModuleCondition]
        private static bool Condition(Capabilities capabilities)
        {
            return capabilities.Requirements.Contains(CAPABILITY);
        }

        [ModuleCallback]
        private void Callback(bool value)
        {
            Active = value;
        }
    }
}
