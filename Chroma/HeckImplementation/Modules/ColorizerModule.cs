using Heck;
using Heck.Module;
using static Chroma.ChromaController;

namespace Chroma.Modules
{
    [Module("ChromaColorizer", 0, LoadType.Passive, new[] { "Heck" })]
    [ModulePatcher(HARMONY_ID + "Colorizer", PatchType.Colorizer)]
    [ModuleDataDeserializer(ID, typeof(CustomDataDeserializer))]
    internal class ColorizerModule : IModule
    {
        internal bool Active { get; private set; }

        [ModuleCallback]
        private void Callback(bool value)
        {
            Active = value;
        }
    }
}
