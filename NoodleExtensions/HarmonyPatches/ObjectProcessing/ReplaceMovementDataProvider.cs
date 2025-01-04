#if LATEST
using IPA.Utilities;
using SiraUtil.Affinity;

namespace NoodleExtensions.HarmonyPatches.ObjectProcessing;

internal class ReplaceMovementDataProvider : IAffinity
{
    private static readonly FieldAccessor<ObstacleControllerBase, IVariableMovementDataProvider>.Accessor
        _obstacleMovementDataProvider =
            FieldAccessor<ObstacleControllerBase, IVariableMovementDataProvider>.GetAccessor(
                nameof(ObstacleControllerBase._variableMovementDataProvider));

    private static readonly FieldAccessor<NoteMovement, IVariableMovementDataProvider>.Accessor
        _noteMovementMovementDataProvider =
            FieldAccessor<NoteMovement, IVariableMovementDataProvider>.GetAccessor(
                nameof(NoteMovement._variableMovementDataProvider));

    private static readonly FieldAccessor<NoteFloorMovement, IVariableMovementDataProvider>.Accessor
        _noteFloorMovementMovementDataProvider =
            FieldAccessor<NoteFloorMovement, IVariableMovementDataProvider>.GetAccessor(
                nameof(NoteFloorMovement._variableMovementDataProvider));

    private static readonly FieldAccessor<NoteJump, IVariableMovementDataProvider>.Accessor
        _noteJumpMovementDataProvider =
            FieldAccessor<NoteJump, IVariableMovementDataProvider>.GetAccessor(
                nameof(NoteJump._variableMovementDataProvider));

    private static readonly FieldAccessor<NoteWaiting, IVariableMovementDataProvider>.Accessor
        _noteWaitingMovementDataProvider =
            FieldAccessor<NoteWaiting, IVariableMovementDataProvider>.GetAccessor(
                nameof(NoteWaiting._variableMovementDataProvider));

    private static readonly FieldAccessor<SliderController, IVariableMovementDataProvider>.Accessor
        _sliderControllerMovementDataProvider =
            FieldAccessor<SliderController, IVariableMovementDataProvider>.GetAccessor(
                nameof(SliderController._variableMovementDataProvider));

    private static readonly FieldAccessor<SliderMovement, IVariableMovementDataProvider>.Accessor
        _sliderMovementMovementDataProvider =
            FieldAccessor<SliderMovement, IVariableMovementDataProvider>.GetAccessor(
                nameof(SliderMovement._variableMovementDataProvider));

    private readonly NoodleMovementDataProvider.Pool _noodleProviderPool;

    private ReplaceMovementDataProvider(NoodleMovementDataProvider.Pool noodleProviderPool)
    {
        _noodleProviderPool = noodleProviderPool;
    }

    [AffinityPrefix]
    [AffinityPatch(typeof(ObstacleController), nameof(ObstacleController.Init))]
    private void ReplaceObstacleMovement(ObstacleController __instance, ObstacleData obstacleData)
    {
        ObstacleControllerBase obstacleControllerBase = __instance;
        _obstacleMovementDataProvider(ref obstacleControllerBase) = _noodleProviderPool.Spawn(obstacleData);
    }

    [AffinityPrefix]
    [AffinityPatch(typeof(NoteController), nameof(NoteController.Init))]
    private void ReplaceNoteMovement(NoteController __instance, NoteData noteData)
    {
        NoteMovement noteMovement = __instance._noteMovement;
        NoteFloorMovement noteFloorMovement = noteMovement._floorMovement;
        NoteJump noteJump = noteMovement._jump;
        NoteWaiting noteWaiting = noteMovement._waiting;
        NoodleMovementDataProvider newProvider = _noodleProviderPool.Spawn(noteData);
        _noteMovementMovementDataProvider(ref noteMovement) = newProvider;
        _noteFloorMovementMovementDataProvider(ref noteFloorMovement) = newProvider;
        _noteJumpMovementDataProvider(ref noteJump) = newProvider;
        _noteWaitingMovementDataProvider(ref noteWaiting) = newProvider;
    }

    [AffinityPrefix]
    [AffinityPatch(typeof(SliderController), nameof(SliderController.Init))]
    private void ReplaceSliderMovement(SliderController __instance, SliderData sliderData)
    {
        SliderMovement sliderMovement = __instance.sliderMovement;
        NoodleMovementDataProvider newProvider = _noodleProviderPool.Spawn(sliderData);
        _sliderControllerMovementDataProvider(ref __instance) = newProvider;
        _sliderMovementMovementDataProvider(ref sliderMovement) = newProvider;
    }

    [AffinityPrefix]
    [AffinityPatch(
        typeof(BasicBeatmapObjectManager),
        nameof(BasicBeatmapObjectManager.DespawnInternal),
        AffinityMethodType.Normal,
        null,
        typeof(ObstacleController))]
    private void DespawnObstacleMovement(ObstacleController obstacleController)
    {
        if (obstacleController._variableMovementDataProvider is NoodleMovementDataProvider noodleMovementDataProvider)
        {
            _noodleProviderPool.Despawn(noodleMovementDataProvider);
        }
    }

    [AffinityPrefix]
    [AffinityPatch(
        typeof(BasicBeatmapObjectManager),
        nameof(BasicBeatmapObjectManager.DespawnInternal),
        AffinityMethodType.Normal,
        null,
        typeof(NoteController))]
    private void DespawnNoteMovement(NoteController noteController)
    {
        if (noteController._noteMovement._variableMovementDataProvider is NoodleMovementDataProvider noodleMovementDataProvider)
        {
            _noodleProviderPool.Despawn(noodleMovementDataProvider);
        }
    }

    [AffinityPrefix]
    [AffinityPatch(
        typeof(BasicBeatmapObjectManager),
        nameof(BasicBeatmapObjectManager.DespawnInternal),
        AffinityMethodType.Normal,
        null,
        typeof(SliderController))]
    private void DespawnSliderMovement(SliderController sliderNoteController)
    {
        if (sliderNoteController._sliderMovement._variableMovementDataProvider is NoodleMovementDataProvider noodleMovementDataProvider)
        {
            _noodleProviderPool.Despawn(noodleMovementDataProvider);
        }
    }
}
#endif
