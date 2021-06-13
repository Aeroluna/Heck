namespace Chroma.Colorizer
{
    using System.Collections.Generic;
    using UnityEngine;

    public static class ColorizerExtensions
    {
        public static NoteColorizer GetNoteColorizer(this NoteControllerBase noteController) => NoteColorizer.Colorizers[noteController];

        public static BombColorizer GetBombColorizer(this NoteControllerBase noteController) => BombColorizer.Colorizers[noteController];

        public static ObstacleColorizer GetObstacleColorizer(this ObstacleControllerBase obstactleController) => ObstacleColorizer.Colorizers[obstactleController];

        public static List<SaberColorizer> GetSaberColorizers(this SaberType saber) => SaberColorizer.Colorizers[saber];

        public static LightColorizer GetLightColorizer(this BeatmapEventType eventType) => LightColorizer.Colorizers[eventType];

        public static List<ParticleColorizer> GetParticleColorizers(this BeatmapEventType eventType) => ParticleColorizer.Colorizers[eventType];

        public static void ColorizeNote(this NoteControllerBase noteController, Color? color) => noteController.GetNoteColorizer().Colorize(color);

        public static void ColorizeBomb(this NoteControllerBase noteController, Color? color) => noteController.GetBombColorizer().Colorize(color);

        public static void ColorizeObstacle(this ObstacleControllerBase obstactleController, Color? color) => obstactleController.GetObstacleColorizer().Colorize(color);

        public static void ColorizeSaber(this SaberType saber, Color? color) => saber.GetSaberColorizers().ForEach(n => n.Colorize(color));

        public static void ColorizeLight(this BeatmapEventType eventType, bool refresh, params Color?[] colors) => eventType.GetLightColorizer().Colorize(refresh, colors);
    }
}
