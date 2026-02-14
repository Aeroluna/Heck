using Heck.BaseProvider;
using JetBrains.Annotations;
using Zenject;

namespace Heck.BaseProviders;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal class MovementDataBaseProvider : IBaseProvider
{
    internal float[] NoteJumpMovementSpeed { get; set; } = new float[1];

    internal float[] NoteJumpStartBeatOffset { get; set; } = new float[1];

    internal float[] JumpDistance { get; set; } = new float[1];

    internal float[] PlayerHeight { get; set; } = new float[1];
}

internal class MovementDataGetter : IInitializable, ITickable
{
    private readonly MovementDataBaseProvider _movementDataBaseProvider;
    private readonly PlayerSpecificSettings _playerSpecificSettings;
    private readonly PlayerHeightDetector? _playerHeightDetector;
    private readonly bool _automaticPlayerHeight;

#if !PRE_V1_40_8
    private readonly IVariableMovementDataProvider _variableMovementDataProvider;
    private readonly BeatmapObjectSpawnController.InitData _initData;
#else
    private readonly BeatmapObjectSpawnMovementData _beatmapObjectSpawnMovementData;
#endif

    [UsedImplicitly]
    private MovementDataGetter(
        MovementDataBaseProvider movementDataBaseProvider,
        GameplayCoreSceneSetupData gameplayCoreSceneSetupData,
        [InjectOptional] PlayerHeightDetector? playerHeightDetector,
#if !PRE_V1_40_8
        IVariableMovementDataProvider variableMovementDataProvider,
        BeatmapObjectSpawnController.InitData initData)
#else
        IBeatmapObjectSpawnController beatmapObjectSpawnController)
#endif
    {
        _movementDataBaseProvider = movementDataBaseProvider;
        _playerSpecificSettings = gameplayCoreSceneSetupData.playerSpecificSettings;
        _playerHeightDetector = playerHeightDetector;
        _automaticPlayerHeight = _playerSpecificSettings.automaticPlayerHeight;
#if !PRE_V1_40_8
        _variableMovementDataProvider = variableMovementDataProvider;
        _initData = initData;
#else
        _beatmapObjectSpawnMovementData = beatmapObjectSpawnController.beatmapObjectSpawnMovementData;
#endif
    }

    public void Initialize()
    {
        if (!_automaticPlayerHeight)
        {
            _movementDataBaseProvider.PlayerHeight[0] = _playerSpecificSettings.playerHeight;
        }
    }

    public void Tick()
    {
#if !PRE_V1_40_8
        _movementDataBaseProvider.NoteJumpMovementSpeed[0] = _variableMovementDataProvider.noteJumpSpeed;
        _movementDataBaseProvider.NoteJumpStartBeatOffset[0] = _initData.noteJumpValue;
        _movementDataBaseProvider.JumpDistance[0] = _variableMovementDataProvider.jumpDistance;
#else
        _movementDataBaseProvider.NoteJumpMovementSpeed[0] = _beatmapObjectSpawnMovementData.noteJumpMovementSpeed;
        _movementDataBaseProvider.NoteJumpStartBeatOffset[0] = _beatmapObjectSpawnMovementData._noteJumpStartBeatOffset;
        _movementDataBaseProvider.JumpDistance[0] = _beatmapObjectSpawnMovementData.jumpDistance;
#endif

        if (_automaticPlayerHeight)
        {
            _movementDataBaseProvider.PlayerHeight[0] = _playerHeightDetector!.playerHeight;
        }
    }
}
