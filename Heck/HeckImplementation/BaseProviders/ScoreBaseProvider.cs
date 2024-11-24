using System;
using Heck.BaseProvider;
using JetBrains.Annotations;
using Zenject;

namespace Heck.BaseProviders;

// ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal class ScoreBaseProvider : IBaseProvider
{
    internal float Combo { get; set; }

    internal float MultipliedScore { get; set; }

    internal float ImmediateMaxPossibleMultipliedScore { get; set; }

    internal float ModifiedScore { get; set; }

    internal float ImmediateMaxPossibleModifiedScore { get; set; }

    internal float RelativeScore { get; set; }

    internal float Multiplier { get; set; }

    internal float Energy { get; set; }

    internal float SongTime { get; set; }

    internal float SongLength { get; set; }
}

internal class ScoreGetter : ITickable, IDisposable
{
    private readonly ScoreBaseProvider _scoreBaseProvider;
    private readonly IScoreController _scoreController;
    private readonly ComboController _comboController;
    private readonly RelativeScoreAndImmediateRankCounter _relativeScoreAndImmediateRankCounter;
    private readonly IGameEnergyCounter _gameEnergyCounter;
    private readonly SongController _songController;
    private readonly BeatmapCallbacksController _beatmapCallbacksController;
    private bool _songDidFinish;

    [UsedImplicitly]
    private ScoreGetter(
        ScoreBaseProvider scoreBaseProvider,
        IScoreController scoreController,
        ComboController comboController,
        RelativeScoreAndImmediateRankCounter relativeScoreAndImmediateRankCounter,
        IGameEnergyCounter gameEnergyCounter,
        SongController songController,
        BeatmapCallbacksController beatmapCallbacksController,
        IAudioTimeSource audioTimeSource)
    {
        _scoreBaseProvider = scoreBaseProvider;
        _scoreController = scoreController;
        scoreController.scoreDidChangeEvent += HandleScoreDidChange;
        scoreController.multiplierDidChangeEvent += HandleMultiplierDidChange;
        _comboController = comboController;
        comboController.comboDidChangeEvent += HandleComboDidChange;
        _relativeScoreAndImmediateRankCounter = relativeScoreAndImmediateRankCounter;
        relativeScoreAndImmediateRankCounter.relativeScoreOrImmediateRankDidChangeEvent +=
            HandleRelativeScoreAndImmediateRankCounterRelativeScoreOrImmediateRankDidChange;
        _gameEnergyCounter = gameEnergyCounter;
        gameEnergyCounter.gameEnergyDidChangeEvent += HandleGameEnergyDidChange;
        _songController = songController;
        songController.songDidFinishEvent += HandleSongDidFinish;
        _beatmapCallbacksController = beatmapCallbacksController;
        scoreBaseProvider.SongLength = audioTimeSource.songLength;
        _scoreBaseProvider.Multiplier = 1;
    }

    public void Tick()
    {
        if (_songDidFinish)
        {
            return;
        }

        // IAudioTimeSource.songTime should not be trusted!
        _scoreBaseProvider.SongTime = _beatmapCallbacksController.songTime;
    }

    public void Dispose()
    {
        if (_scoreController != null)
        {
            _scoreController.scoreDidChangeEvent -= HandleScoreDidChange;
            _scoreController.multiplierDidChangeEvent -= HandleMultiplierDidChange;
        }

        if (_comboController != null)
        {
            _comboController.comboDidChangeEvent -= HandleComboDidChange;
        }

        if (_relativeScoreAndImmediateRankCounter != null)
        {
            _relativeScoreAndImmediateRankCounter.relativeScoreOrImmediateRankDidChangeEvent -=
                HandleRelativeScoreAndImmediateRankCounterRelativeScoreOrImmediateRankDidChange;
        }

        if (_songController != null)
        {
            _songController.songDidFinishEvent -= HandleSongDidFinish;
        }

        if (_gameEnergyCounter != null)
        {
            _gameEnergyCounter.gameEnergyDidChangeEvent -= HandleGameEnergyDidChange;
        }
    }

    private void HandleScoreDidChange(int multipliedScore, int modifiedScore)
    {
        _scoreBaseProvider.MultipliedScore = multipliedScore;
        _scoreBaseProvider.ModifiedScore = modifiedScore;
        _scoreBaseProvider.ImmediateMaxPossibleMultipliedScore = _scoreController.immediateMaxPossibleMultipliedScore;
        _scoreBaseProvider.ImmediateMaxPossibleModifiedScore = _scoreController.immediateMaxPossibleModifiedScore;
    }

    private void HandleMultiplierDidChange(int multiplier, float normalizedProgress)
    {
        _scoreBaseProvider.Multiplier = multiplier;
    }

    private void HandleComboDidChange(int combo)
    {
        _scoreBaseProvider.Combo = combo;
    }

    // wtf is this method name
    private void HandleRelativeScoreAndImmediateRankCounterRelativeScoreOrImmediateRankDidChange()
    {
        _scoreBaseProvider.RelativeScore = _relativeScoreAndImmediateRankCounter.relativeScore;
    }

    private void HandleGameEnergyDidChange(float energy)
    {
        _scoreBaseProvider.Energy = energy;
    }

    private void HandleSongDidFinish()
    {
        _songDidFinish = true;
    }
}
