using JetBrains.Annotations;
using NoodleExtensions.Animation;
using NoodleExtensions.HarmonyPatches.FakeNotes;
using NoodleExtensions.HarmonyPatches.Mirror;
using NoodleExtensions.HarmonyPatches.ObjectProcessing;
using NoodleExtensions.HarmonyPatches.Objects;
using NoodleExtensions.HarmonyPatches.SmallFixes;
using NoodleExtensions.Managers;
using Zenject;

namespace NoodleExtensions.Installers
{
    [UsedImplicitly]
    internal class PlayerInstaller : Installer
    {
        public override void InstallBindings()
        {
            if (!NoodleController.FeaturesPatcher.Enabled)
            {
                return;
            }

            Container.Bind<SpawnDataManager>().AsSingle();
            Container.Bind<CutoutManager>().AsSingle();
            Container.Bind<AnimationHelper>().AsSingle();

            // Events
            Container.Bind<EventController>().FromNewComponentOnRoot().AsSingle().NonLazy();
            Container.Bind<ParentController>().AsSingle();
            Container.Bind<PlayerTrack>().FromFactory<PlayerTrack.PlayerTrackFactory>().AsSingle();

            // Cutout
            Container.BindInterfacesTo<ObjectInitializer>().AsSingle();

            // FakeNotes
            Container.BindInterfacesTo<FakeNotePatches>().AsSingle();
            Container.BindInterfacesTo<MiscFakePatches>().AsSingle();
            Container.BindInterfacesAndSelfTo<FakePatchesManager>().AsSingle();

            // Mirror
            ////Container.BindInterfacesTo<MirroredNoteNoodleTracker>().AsSingle(); https://github.com/Auros/SiraUtil/issues/36
            Container.BindInterfacesTo<MirroredObstacleNoodleTracker>().AsSingle();

            // ObjectProcessing
            Container.BindInterfacesAndSelfTo<ManagedActiveObstacleTracker>().AsSingle();
            Container.BindInterfacesTo<NoodledSpawnMovementData>().AsSingle();
            Container.BindInterfacesTo<ObjectCallbackAheadTimeReorder>().AsSingle();
            Container.BindInterfacesTo<RemoveFromTrackObjects>().AsSingle();

            // Objects
            Container.BindInterfacesTo<NoteFloorMovementNoodlifier>().AsSingle();
            Container.BindInterfacesTo<NoteInitNoodlifier>().AsSingle();
            Container.BindInterfacesAndSelfTo<NoteJumpNoodlifier>().AsSingle();
            Container.BindInterfacesAndSelfTo<NoteUpdateNoodlifier>().AsSingle();
            Container.BindInterfacesTo<ObstacleInitNoodlifier>().AsSingle();
            Container.BindInterfacesTo<ObstacleUpdateNoodlifier>().AsSingle();

            // SmallFixes
            Container.BindInterfacesTo<NoteCutSoundLimiter>().AsSingle();
            Container.BindInterfacesTo<PreventObstacleFlickerOnSpawn>().AsSingle();
            Container.Bind<InitializedSpawnMovementData>().AsSingle().NonLazy();
        }
    }
}
