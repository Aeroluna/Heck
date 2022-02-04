using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Chroma.Colorizer;
using HarmonyLib;
using Heck;
using IPA.Utilities;
using JetBrains.Annotations;
using Zenject;

namespace Chroma.HarmonyPatches.Colorizer.Initialize
{
    [HeckPatch(PatchType.Colorizer)]
    [HarmonyPatch(typeof(FakeMirrorObjectsInstaller))]
    internal static class MirrorColorizerInitialize
    {
        private static readonly PropertyAccessor<MonoInstallerBase, DiContainer>.Getter _containerAccessor =
            PropertyAccessor<MonoInstallerBase, DiContainer>.GetGetter("Container");

        private static readonly FieldInfo _mirroredGameNoteControllerPrefab =
            AccessTools.Field(typeof(FakeMirrorObjectsInstaller), "_mirroredGameNoteControllerPrefab");

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(FakeMirrorObjectsInstaller.InstallBindings))]
        private static IEnumerable<CodeInstruction> SkipOriginalBind(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Ldfld, _mirroredGameNoteControllerPrefab))
                .Advance(-6)
                .RemoveInstructions(27)
                .InstructionEnumeration();
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(FakeMirrorObjectsInstaller.InstallBindings))]
        private static void BindMirrorPrefabs(
            FakeMirrorObjectsInstaller __instance,
            MirroredCubeNoteController ____mirroredGameNoteControllerPrefab,
            MirroredBombNoteController ____mirroredBombNoteControllerPrefab,
            MirroredObstacleController ____mirroredObstacleControllerPrefab)
        {
            MonoInstallerBase installerBase = __instance;
            DiContainer container = _containerAccessor(ref installerBase);

            container.Bind<MirroredCubeNoteController>().FromInstance(____mirroredGameNoteControllerPrefab).AsSingle();
            container.Bind<MirroredBombNoteController>().FromInstance(____mirroredBombNoteControllerPrefab).AsSingle();
            container.Bind<MirroredObstacleController>().FromInstance(____mirroredObstacleControllerPrefab).AsSingle();

            container.BindMemoryPool<MirroredCubeNoteController, MirroredCubeNoteController.Pool>()
                .WithInitialSize(25).ExpandByDoubling().FromFactory<MirroredCubeNoteFactory>();
            container.BindMemoryPool<MirroredBombNoteController, MirroredBombNoteController.Pool>()
                .WithInitialSize(35).ExpandByDoubling().FromFactory<MirroredBombNoteFactory>();
            container.BindMemoryPool<MirroredObstacleController, MirroredObstacleController.Pool>()
                .WithInitialSize(25).ExpandByDoubling().FromFactory<MirroredObstacleFactory>();
        }

        [UsedImplicitly]
        internal class MirroredCubeNoteFactory : IFactory<MirroredCubeNoteController>
        {
            private readonly IInstantiator _container;
            private readonly NoteColorizerManager _colorizerManager;
            private readonly MirroredCubeNoteController _prefab;

            private MirroredCubeNoteFactory(
                IInstantiator container,
                NoteColorizerManager colorizerManager,
                MirroredCubeNoteController prefab)
            {
                _container = container;
                _colorizerManager = colorizerManager;
                _prefab = prefab;
            }

            public MirroredCubeNoteController Create()
            {
                MirroredCubeNoteController note = _container.InstantiatePrefabForComponent<MirroredCubeNoteController>(_prefab);
                _colorizerManager.Create(note);
                return note;
            }
        }

        [UsedImplicitly]
        internal class MirroredBombNoteFactory : IFactory<MirroredBombNoteController>
        {
            private readonly IInstantiator _container;
            private readonly BombColorizerManager _colorizerManager;
            private readonly MirroredBombNoteController _prefab;

            private MirroredBombNoteFactory(
                IInstantiator container,
                BombColorizerManager colorizerManager,
                MirroredBombNoteController prefab)
            {
                _container = container;
                _colorizerManager = colorizerManager;
                _prefab = prefab;
            }

            public MirroredBombNoteController Create()
            {
                MirroredBombNoteController note = _container.InstantiatePrefabForComponent<MirroredBombNoteController>(_prefab);
                _colorizerManager.Create(note);
                return note;
            }
        }

        [UsedImplicitly]
        internal class MirroredObstacleFactory : IFactory<MirroredObstacleController>
        {
            private readonly IInstantiator _container;
            private readonly ObstacleColorizerManager _colorizerManager;
            private readonly MirroredObstacleController _prefab;

            private MirroredObstacleFactory(
                IInstantiator container,
                ObstacleColorizerManager colorizerManager,
                MirroredObstacleController prefab)
            {
                _container = container;
                _colorizerManager = colorizerManager;
                _prefab = prefab;
            }

            public MirroredObstacleController Create()
            {
                MirroredObstacleController note = _container.InstantiatePrefabForComponent<MirroredObstacleController>(_prefab);
                _colorizerManager.Create(note);
                return note;
            }
        }
    }
}
