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

        // patching generics is weird
        private void MirrorColorize<T>(MirroredNoteController<T> instance, INoteMirrorable followedNote)
            where T : INoteMirrorable
        {
            if (followedNote is IGameNoteMirrorable)
            {
                _noteManager.Colorize(instance, _noteManager.GetColorizer((NoteControllerBase)followedNote).Color);
            }
            else
            {
                _bombManager.Colorize(instance, _bombManager.GetColorizer((NoteControllerBase)followedNote).Color);
            }
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(MirroredNoteController<INoteMirrorable>), "UpdatePositionAndRotation")]
        private void BombMirrorColorize(MirroredNoteController<INoteMirrorable> __instance, INoteMirrorable ___followedNote)
        {
            MirrorColorize(__instance, ___followedNote);
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(MirroredNoteController<IGameNoteMirrorable>), "UpdatePositionAndRotation")]
        private void NoteMirrorColorize(MirroredNoteController<IGameNoteMirrorable> __instance, IGameNoteMirrorable ___followedNote)
        {
            MirrorColorize(__instance, ___followedNote);
        }
    }
}
