namespace Heck
{
    public interface IGameNoteInitializer
    {
        public void InitializeGameNote(NoteControllerBase gameNoteController);
    }

    public interface IBombNoteInitializer
    {
        public void InitializeBombNote(NoteControllerBase bombNoteController);
    }

    public interface IObstacleInitializer
    {
        public void InitializeObstacle(ObstacleControllerBase obstacleController);
    }

    public interface ISliderInitializer
    {
        public void InitializeSlider(SliderControllerBase sliderController);
    }
}
