namespace Chroma
{
    public interface IChromaBehaviourExtension
    {
        void PostInitialization(float songBPM, BeatmapData beatmapData, PlayerSpecificSettings playerSettings, ScoreController scoreController);

        void OnEnable();

        void OnDisable();
    }
}