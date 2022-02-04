using Chroma.Colorizer;
using HarmonyLib;
using Heck;
using IPA.Utilities;
using JetBrains.Annotations;
using SiraUtil.Affinity;
using Zenject;

namespace Chroma.HarmonyPatches.Colorizer.Initialize
{
    [HeckPatch(PatchType.Colorizer)]
    internal class ObjectColorizerInitialize : IAffinity
    {
        private static readonly PropertyAccessor<MonoInstallerBase, DiContainer>.Getter _containerAccessor =
            PropertyAccessor<MonoInstallerBase, DiContainer>.GetGetter("Container");

        private readonly NoteColorizerManager _noteManager;
        private readonly BombColorizerManager _bombManager;
        private readonly ObstacleColorizerManager _obstacleManager;

        private ObjectColorizerInitialize(NoteColorizerManager noteManager, BombColorizerManager bombManager, ObstacleColorizerManager obstacleManager)
        {
            _noteManager = noteManager;
            _bombManager = bombManager;
            _obstacleManager = obstacleManager;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BeatmapObjectsInstaller), nameof(BeatmapObjectsInstaller.InstallBindings))]
        private static bool BindPrefabs(
            BeatmapObjectsInstaller __instance,
            GameplayCoreSceneSetupData ____sceneSetupData,
            GameNoteController ____normalBasicNotePrefab,
            GameNoteController ____proModeNotePrefab,
            BombNoteController ____bombNotePrefab,
            ObstacleController ____obstaclePrefab,
            NoteLineConnectionController ____noteLineConnectionControllerPrefab,
            BeatLine ____beatLinePrefab)
        {
            MonoInstallerBase installerBase = __instance;
            DiContainer container = _containerAccessor(ref installerBase);
            if (____sceneSetupData.gameplayModifiers.proMode)
            {
                container.Bind<GameNoteController>().FromInstance(____proModeNotePrefab).AsSingle();
            }
            else
            {
                container.Bind<GameNoteController>().FromInstance(____normalBasicNotePrefab).AsSingle();
            }

            container.Bind<BombNoteController>().FromInstance(____bombNotePrefab).AsSingle();
            container.Bind<ObstacleController>().FromInstance(____obstaclePrefab).AsSingle();

            container.BindMemoryPool<GameNoteController, GameNoteController.Pool>().WithInitialSize(25).ExpandByDoubling().FromFactory<GameNoteFactory>();
            container.BindMemoryPool<BombNoteController, BombNoteController.Pool>().WithInitialSize(35).ExpandByDoubling().FromFactory<BombNoteFactory>();
            container.BindMemoryPool<ObstacleController, ObstacleController.Pool>().WithInitialSize(25).ExpandByDoubling().FromFactory<ObstacleFactory>();

            container.BindMemoryPool<NoteLineConnectionController, NoteLineConnectionController.Pool>().WithInitialSize(10).FromComponentInNewPrefab(____noteLineConnectionControllerPrefab);
            container.BindMemoryPool<BeatLine, BeatLine.Pool>().WithInitialSize(16).FromComponentInNewPrefab(____beatLinePrefab);

            return false;
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(BaseNoteVisuals), nameof(BaseNoteVisuals.OnDestroy))]
        private void RemoveNoteColorizer(NoteControllerBase ____noteController)
        {
            switch (____noteController)
            {
                case BombNoteController:
                case MultiplayerConnectedPlayerBombNoteController:
                case MirroredBombNoteController:
                    _bombManager.Colorizers.Remove(____noteController);
                    break;
                default:
                    _noteManager.Colorizers.Remove(____noteController);
                    break;
            }
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(ObstacleDissolve), nameof(ObstacleDissolve.OnDestroy))]
        private void RemoveObstacleColorizer(ObstacleControllerBase ____obstacleController)
        {
            _obstacleManager.Colorizers.Remove(____obstacleController);
        }

        [UsedImplicitly]
        internal class GameNoteFactory : IFactory<GameNoteController>
        {
            private readonly IInstantiator _container;
            private readonly NoteColorizerManager _colorizerManager;
            private readonly GameNoteController _prefab;

            private GameNoteFactory(
                IInstantiator container,
                NoteColorizerManager colorizerManager,
                GameNoteController prefab)
            {
                _container = container;
                _colorizerManager = colorizerManager;
                _prefab = prefab;
            }

            public GameNoteController Create()
            {
                GameNoteController note = _container.InstantiatePrefabForComponent<GameNoteController>(_prefab);
                _colorizerManager.Create(note);
                return note;
            }
        }

        [UsedImplicitly]
        internal class BombNoteFactory : IFactory<BombNoteController>
        {
            private readonly IInstantiator _container;
            private readonly BombColorizerManager _colorizerManager;
            private readonly BombNoteController _prefab;

            private BombNoteFactory(
                IInstantiator container,
                BombColorizerManager colorizerManager,
                BombNoteController prefab)
            {
                _container = container;
                _colorizerManager = colorizerManager;
                _prefab = prefab;
            }

            public BombNoteController Create()
            {
                BombNoteController note = _container.InstantiatePrefabForComponent<BombNoteController>(_prefab);
                _colorizerManager.Create(note);
                return note;
            }
        }

        [UsedImplicitly]
        internal class ObstacleFactory : IFactory<ObstacleController>
        {
            private readonly IInstantiator _container;
            private readonly ObstacleColorizerManager _colorizerManager;
            private readonly ObstacleController _prefab;

            private ObstacleFactory(
                IInstantiator container,
                ObstacleColorizerManager colorizerManager,
                ObstacleController prefab)
            {
                _container = container;
                _colorizerManager = colorizerManager;
                _prefab = prefab;
            }

            public ObstacleController Create()
            {
                ObstacleController note = _container.InstantiatePrefabForComponent<ObstacleController>(_prefab);
                _colorizerManager.Create(note);
                return note;
            }
        }
    }
}
