using static Heck.HeckController;

namespace Heck
{
    [Module(ID, 0, LoadType.Passive)]
    [ModulePatcher(HARMONY_ID + "Features", PatchType.Features)]
    [ModuleDataDeserializer(ID, typeof(CustomDataDeserializer))]
    internal class FeaturesModule : IModule
    {
        internal bool Active { get; private set; }

        [ModuleCallback]
        private void Callback(bool value)
        {
            Active = value;
        }
    }
}
