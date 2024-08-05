using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;

namespace Heck.ObjectInitialize;

[UsedImplicitly]
internal class ObjectInitializerManager
{
    private readonly List<IBombNoteInitializer> _bombNoteInitializers;
    private readonly List<IGameNoteInitializer> _gameNoteInitializers;
    private readonly IInstantiator _instantiator;
    private readonly List<IObstacleInitializer> _obstacleInitializers;
    private readonly List<ISliderInitializer> _sliderInitializers;

    private ObjectInitializerManager(
        [InjectOptional] List<IGameNoteInitializer> gameNoteInitializers,
        [InjectOptional] List<IBombNoteInitializer> bombNoteInitializers,
        [InjectOptional] List<IObstacleInitializer> obstacleInitializers,
        [InjectOptional] List<ISliderInitializer> sliderInitializers,
        IInstantiator instantiator)
    {
        _gameNoteInitializers = gameNoteInitializers;
        _bombNoteInitializers = bombNoteInitializers;
        _obstacleInitializers = obstacleInitializers;
        _sliderInitializers = sliderInitializers;
        _instantiator = instantiator;
    }

    internal T CreateBombNoteController<T>(Object prefab)
        where T : NoteControllerBase
    {
        NoteControllerBase controller = _instantiator.InstantiatePrefabForComponent<NoteControllerBase>(prefab);
        _bombNoteInitializers.ForEach(n => n.InitializeBombNote(controller));
        return (T)controller;
    }

    internal T CreateGameNoteController<T>(Object prefab)
        where T : NoteControllerBase
    {
        NoteControllerBase controller = _instantiator.InstantiatePrefabForComponent<NoteControllerBase>(prefab);
        _gameNoteInitializers.ForEach(n => n.InitializeGameNote(controller));
        return (T)controller;
    }

    internal T CreateObstacleController<T>(Object prefab)
        where T : ObstacleControllerBase
    {
        ObstacleControllerBase controller = _instantiator.InstantiatePrefabForComponent<ObstacleControllerBase>(prefab);
        _obstacleInitializers.ForEach(n => n.InitializeObstacle(controller));
        return (T)controller;
    }

    internal T CreateSliderController<T>(Object prefab)
        where T : SliderControllerBase
    {
        SliderControllerBase controller = _instantiator.InstantiatePrefabForComponent<SliderControllerBase>(prefab);
        _sliderInitializers.ForEach(n => n.InitializeSlider(controller));
        return (T)controller;
    }
}
