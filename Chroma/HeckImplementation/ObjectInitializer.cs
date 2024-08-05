using Chroma.Colorizer;
using Heck.ObjectInitialize;
using JetBrains.Annotations;

namespace Chroma;

[UsedImplicitly]
internal class ObjectInitializer : IGameNoteInitializer, IBombNoteInitializer, IObstacleInitializer, ISliderInitializer
{
    private readonly BombColorizerManager _bombManager;
    private readonly NoteColorizerManager _noteManager;
    private readonly ObstacleColorizerManager _obstacleManager;
    private readonly SliderColorizerManager _sliderManager;

    private ObjectInitializer(
        NoteColorizerManager noteManager,
        BombColorizerManager bombManager,
        ObstacleColorizerManager obstacleManager,
        SliderColorizerManager sliderManager)
    {
        _noteManager = noteManager;
        _bombManager = bombManager;
        _obstacleManager = obstacleManager;
        _sliderManager = sliderManager;
    }

    public void InitializeBombNote(NoteControllerBase noteController)
    {
        _bombManager.Create(noteController);
    }

    public void InitializeGameNote(NoteControllerBase noteController)
    {
        _noteManager.Create(noteController);
    }

    public void InitializeObstacle(ObstacleControllerBase obstacleController)
    {
        _obstacleManager.Create(obstacleController);
    }

    public void InitializeSlider(SliderControllerBase sliderController)
    {
        if (sliderController is SliderController slider)
        {
            _sliderManager.Create(slider);
        }
    }
}
