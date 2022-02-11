using System.Collections.Generic;
using JetBrains.Annotations;
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
        private readonly GameNoteController _gameNotePrefab;
        private readonly BombNoteController _bombNotePrefab;
        private readonly ObstacleController _obstaclePrefab;
        private readonly MirroredCubeNoteController _mirroredCubeNotePrefab;
        private readonly MirroredBombNoteController _mirroredBombNotePrefab;
        private readonly MirroredObstacleController _mirroredObstaclePrefab;
        private readonly MultiplayerConnectedPlayerGameNoteController _multiplayerGameNoteControllerPrefab;
        private readonly MultiplayerConnectedPlayerBombNoteController _multiplayerBombNoteControllerPrefab;
        private readonly MultiplayerConnectedPlayerObstacleController _multiplayerObstacleControllerPrefab;

        private ObjectInitializerManager(
            [InjectOptional] List<IGameNoteInitializer> gameNoteInitializers,
            [InjectOptional] List<IBombNoteInitializer> bombNoteInitializers,
            [InjectOptional] List<IObstacleInitializer> obstacleInitializers,
            IInstantiator instantiator,
            GameNoteController gameNotePrefab,
            BombNoteController bombNotePrefab,
            ObstacleController obstaclePrefab,
            [InjectOptional] MirroredCubeNoteController mirroredCubeNotePrefab,
            [InjectOptional] MirroredBombNoteController mirroredBombNotePrefab,
            [InjectOptional] MirroredObstacleController mirroredObstaclePrefab,
            [InjectOptional] MultiplayerConnectedPlayerGameNoteController multiplayerGameNoteControllerPrefab,
            [InjectOptional] MultiplayerConnectedPlayerBombNoteController multiplayerBombNoteControllerPrefab,
            [InjectOptional] MultiplayerConnectedPlayerObstacleController multiplayerObstacleControllerPrefab)
        {
            _gameNoteInitializers = gameNoteInitializers;
            _bombNoteInitializers = bombNoteInitializers;
            _obstacleInitializers = obstacleInitializers;
            _instantiator = instantiator;
            _gameNotePrefab = gameNotePrefab;
            _bombNotePrefab = bombNotePrefab;
            _obstaclePrefab = obstaclePrefab;
            _mirroredCubeNotePrefab = mirroredCubeNotePrefab;
            _mirroredBombNotePrefab = mirroredBombNotePrefab;
            _mirroredObstaclePrefab = mirroredObstaclePrefab;
            _multiplayerGameNoteControllerPrefab = multiplayerGameNoteControllerPrefab;
            _multiplayerBombNoteControllerPrefab = multiplayerBombNoteControllerPrefab;
            _multiplayerObstacleControllerPrefab = multiplayerObstacleControllerPrefab;
        }

        internal GameNoteController CreateGameNoteController()
        {
            GameNoteController controller = _instantiator.InstantiatePrefabForComponent<GameNoteController>(_gameNotePrefab);
            _gameNoteInitializers.ForEach(n => n.InitializeGameNote(controller));
            return controller;
        }

        internal BombNoteController CreateBombNoteController()
        {
            BombNoteController controller = _instantiator.InstantiatePrefabForComponent<BombNoteController>(_bombNotePrefab);
            _bombNoteInitializers.ForEach(n => n.InitializeBombNote(controller));
            return controller;
        }

        internal ObstacleController CreateObstacleController()
        {
            ObstacleController controller = _instantiator.InstantiatePrefabForComponent<ObstacleController>(_obstaclePrefab);
            _obstacleInitializers.ForEach(n => n.InitializeObstacle(controller));
            return controller;
        }

        internal MirroredCubeNoteController CreateMirroredCubeNoteController()
        {
            MirroredCubeNoteController controller = _instantiator.InstantiatePrefabForComponent<MirroredCubeNoteController>(_mirroredCubeNotePrefab);
            _gameNoteInitializers.ForEach(n => n.InitializeGameNote(controller));
            return controller;
        }

        internal MirroredBombNoteController CreateMirroredBombNoteController()
        {
            MirroredBombNoteController controller = _instantiator.InstantiatePrefabForComponent<MirroredBombNoteController>(_mirroredBombNotePrefab);
            _bombNoteInitializers.ForEach(n => n.InitializeBombNote(controller));
            return controller;
        }

        internal MirroredObstacleController CreateMirroredObstacleController()
        {
            MirroredObstacleController controller = _instantiator.InstantiatePrefabForComponent<MirroredObstacleController>(_mirroredObstaclePrefab);
            _obstacleInitializers.ForEach(n => n.InitializeObstacle(controller));
            return controller;
        }

        internal MultiplayerConnectedPlayerGameNoteController CreateMultiplayerConnectedPlayerGameNoteController()
        {
            MultiplayerConnectedPlayerGameNoteController controller =
                _instantiator.InstantiatePrefabForComponent<MultiplayerConnectedPlayerGameNoteController>(_multiplayerGameNoteControllerPrefab);
            _gameNoteInitializers.ForEach(n => n.InitializeGameNote(controller));
            return controller;
        }

        internal MultiplayerConnectedPlayerBombNoteController CreateMultiplayerConnectedPlayerBombNoteController()
        {
            MultiplayerConnectedPlayerBombNoteController controller =
                _instantiator.InstantiatePrefabForComponent<MultiplayerConnectedPlayerBombNoteController>(_multiplayerBombNoteControllerPrefab);
            _bombNoteInitializers.ForEach(n => n.InitializeBombNote(controller));
            return controller;
        }

        internal MultiplayerConnectedPlayerObstacleController CreateMultiplayerConnectedPlayerObstacleController()
        {
            MultiplayerConnectedPlayerObstacleController controller =
                _instantiator.InstantiatePrefabForComponent<MultiplayerConnectedPlayerObstacleController>(_multiplayerObstacleControllerPrefab);
            _obstacleInitializers.ForEach(n => n.InitializeObstacle(controller));
            return controller;
        }
    }
}
