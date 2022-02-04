using Chroma.Colorizer;
using SiraUtil.Affinity;

namespace Chroma.HarmonyPatches.Mirror
{
    internal class MirroredNoteChromaTracker : IAffinity
    {
        private readonly NoteColorizerManager _noteManager;
        private readonly BombColorizerManager _bombManager;

        private MirroredNoteChromaTracker(NoteColorizerManager noteManager, BombColorizerManager bombManager)
        {
            _noteManager = noteManager;
            _bombManager = bombManager;
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(MirroredNoteController<INoteMirrorable>), "UpdatePositionAndRotation")]
        private void Postfix(MirroredNoteController<INoteMirrorable> __instance, INoteMirrorable ___followedNote)
        {
            _bombManager.Colorize(__instance, _bombManager.GetColorizer((NoteControllerBase)___followedNote).Color);
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(MirroredNoteController<ICubeNoteMirrorable>), "UpdatePositionAndRotation")]
        private void Postfix(MirroredNoteController<ICubeNoteMirrorable> __instance, ICubeNoteMirrorable ___followedNote)
        {
            _noteManager.Colorize(__instance, _noteManager.GetColorizer((NoteControllerBase)___followedNote).Color);
        }
    }
}
