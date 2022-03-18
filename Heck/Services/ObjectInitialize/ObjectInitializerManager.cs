using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;

namespace Heck
{
    [UsedImplicitly]
    internal class ObjectInitializerManager
    {
        private readonly List<IGameNoteInitializer> _gameNoteInitializers;
        private readonly List<IBombNoteInitializer> _bombNoteInitializers;
        private readonly List<IObstacleInitializer> _obstacleInitializers;
        private readonly IInstantiator _instantiator;

        private ObjectInitializerManager(
            [InjectOptional] List<IGameNoteInitializer> gameNoteInitializers,
            [InjectOptional] List<IBombNoteInitializer> bombNoteInitializers,
            [InjectOptional] List<IObstacleInitializer> obstacleInitializers,
            IInstantiator instantiator)
        {
            _gameNoteInitializers = gameNoteInitializers;
            _bombNoteInitializers = bombNoteInitializers;
            _obstacleInitializers = obstacleInitializers;
            _instantiator = instantiator;
        }

        internal T CreateGameNoteController<T>(Object prefab)
            where T : NoteControllerBase
        {
            NoteControllerBase controller = _instantiator.InstantiatePrefabForComponent<NoteControllerBase>(prefab);
            _gameNoteInitializers.ForEach(n => n.InitializeGameNote(controller));
            return (T)controller;
        }

        internal T CreateBombNoteController<T>(Object prefab)
            where T : NoteControllerBase
        {
            NoteControllerBase controller = _instantiator.InstantiatePrefabForComponent<NoteControllerBase>(prefab);
            _bombNoteInitializers.ForEach(n => n.InitializeBombNote(controller));
            return (T)controller;
        }

        internal T CreateObstacleController<T>(Object prefab)
            where T : ObstacleControllerBase
        {
            ObstacleControllerBase controller = _instantiator.InstantiatePrefabForComponent<ObstacleControllerBase>(prefab);
            _obstacleInitializers.ForEach(n => n.InitializeObstacle(controller));
            return (T)controller;
        }
    }
}
