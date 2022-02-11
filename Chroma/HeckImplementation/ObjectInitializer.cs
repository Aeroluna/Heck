using Chroma.Colorizer;
using Heck;
using JetBrains.Annotations;

namespace Chroma
{
    [UsedImplicitly]
    internal class ObjectInitializer : IGameNoteInitializer, IBombNoteInitializer, IObstacleInitializer
    {
        private readonly NoteColorizerManager _noteManager;
        private readonly BombColorizerManager _bombManager;
        private readonly ObstacleColorizerManager _obstacleManager;

        private ObjectInitializer(NoteColorizerManager noteManager, BombColorizerManager bombManager, ObstacleColorizerManager obstacleManager)
        {
            _noteManager = noteManager;
            _bombManager = bombManager;
            _obstacleManager = obstacleManager;
        }

        public void InitializeGameNote(NoteControllerBase noteController)
        {
            _noteManager.Create(noteController);
        }

        public void InitializeBombNote(NoteControllerBase noteController)
        {
            _bombManager.Create(noteController);
        }

        public void InitializeObstacle(ObstacleControllerBase obstacleController)
        {
            _obstacleManager.Create(obstacleController);
        }
    }
}
