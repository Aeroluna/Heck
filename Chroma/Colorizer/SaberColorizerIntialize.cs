using JetBrains.Annotations;

namespace Chroma.Colorizer
{
    [UsedImplicitly]
    internal class SaberColorizerIntialize
    {
        private SaberColorizerIntialize(SaberColorizerManager manager, SaberManager saberManager)
        {
            manager.Create(saberManager.leftSaber);
            manager.Create(saberManager.rightSaber);
        }
    }
}
