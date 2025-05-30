using Heck.BaseProvider;
using JetBrains.Annotations;
using Zenject;

namespace Heck.BaseProviders;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal class SettingsBaseProvider : IBaseProvider
{
    internal float[] NoteJumpMovementSpeed { get; set; } = new float[1];

    internal float[] NoteJumpStartBeatOffset { get; set; } = new float[1];

    internal float[] PlayerHeight { get; set; } = new float[1];
}

internal class SettingsGetter : IInitializable, ITickable
{
    private readonly SettingsBaseProvider _settingsBaseProvider;
    private readonly PlayerSpecificSettings _playerSpecificSettings;
    private readonly PlayerHeightDetector? _playerHeightDetector;
    private readonly bool _automaticPlayerHeight;
    private readonly BeatmapObjectSpawnMovementData _beatmapObjectSpawnMovementData;

    [UsedImplicitly]
    private SettingsGetter(
        SettingsBaseProvider settingsBaseProvider,
        PlayerSpecificSettings playerSpecificSettings,
        [InjectOptional] PlayerHeightDetector? playerHeightDetector,
        BeatmapObjectSpawnMovementData beatmapObjectSpawnMovementData)
    {
        _settingsBaseProvider = settingsBaseProvider;
        _playerSpecificSettings = playerSpecificSettings;
        _playerHeightDetector = playerHeightDetector;
        _automaticPlayerHeight = playerSpecificSettings.automaticPlayerHeight;
        _beatmapObjectSpawnMovementData = beatmapObjectSpawnMovementData;
    }

    public void Initialize()
    {
        if (!_automaticPlayerHeight)
        {
            _settingsBaseProvider.PlayerHeight[0] = _playerSpecificSettings.playerHeight;
        }
    }

    public void Tick()
    {
        _settingsBaseProvider.NoteJumpMovementSpeed[0] = _beatmapObjectSpawnMovementData.noteJumpMovementSpeed;
        _settingsBaseProvider.NoteJumpStartBeatOffset[0] = _beatmapObjectSpawnMovementData._noteJumpStartBeatOffset;

        if (_automaticPlayerHeight)
        {
            _settingsBaseProvider.PlayerHeight[0] = _playerHeightDetector!.playerHeight;
        }
    }
}
