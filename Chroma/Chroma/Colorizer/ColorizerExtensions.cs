namespace Chroma.Colorizer
{
    using System;
    using UnityEngine;

    public static class ColorizerExtensions
    {
        public static bool TryGetNoteColorizer(this NoteControllerBase noteController, out NoteColorizer colorizer)
        {
            if (NoteColorizer.Colorizers.TryGetValue(noteController, out colorizer))
            {
                return true;
            }
            else
            {
                throw new InvalidOperationException("Could not find NoteColorizer");
            }
        }

        public static bool TryGetBombColorizer(this NoteControllerBase noteController, out BombColorizer colorizer)
        {
            if (BombColorizer.Colorizers.TryGetValue(noteController, out colorizer))
            {
                return true;
            }
            else
            {
                throw new InvalidOperationException("Could not find BombColorizer");
            }
        }

        public static bool TryGetObstacleColorizer(this ObstacleControllerBase obstactleController, out ObstacleColorizer colorizer)
        {
            if (ObstacleColorizer.Colorizers.TryGetValue(obstactleController, out colorizer))
            {
                return true;
            }
            else
            {
                throw new InvalidOperationException("Could not find ObstacleColorizer");
            }
        }

        public static bool TryGetSaberColorizer(this SaberType saber, out SaberColorizer colorizer)
        {
            if (SaberColorizer.Colorizers.TryGetValue(saber, out colorizer))
            {
                return true;
            }
            else
            {
                throw new InvalidOperationException("Could not find SaberColorizer");
            }
        }

        public static bool TryGetLightColorizer(this BeatmapEventType eventType, out LightColorizer colorizer)
        {
            if (LightColorizer.Colorizers.TryGetValue(eventType, out colorizer))
            {
                return true;
            }
            else
            {
                throw new InvalidOperationException("Could not find LightColorizer");
            }
        }

        public static bool TryGetParticleColorizer(this BeatmapEventType eventType, out ParticleColorizer colorizer)
        {
            if (ParticleColorizer.Colorizers.TryGetValue(eventType, out colorizer))
            {
                return true;
            }
            else
            {
                throw new InvalidOperationException("Could not find ParticleColorizer");
            }
        }

        public static void ColorizeNote(this NoteControllerBase noteController, Color? color)
        {
            if (noteController.TryGetNoteColorizer(out NoteColorizer colorizer))
            {
                colorizer.Colorize(color);
            }
        }

        public static void ColorizeBomb(this NoteControllerBase noteController, Color? color)
        {
            if (noteController.TryGetBombColorizer(out BombColorizer colorizer))
            {
                colorizer.Colorize(color);
            }
        }

        public static void ColorizeObstacle(this ObstacleControllerBase obstactleController, Color? color)
        {
            if (obstactleController.TryGetObstacleColorizer(out ObstacleColorizer colorizer))
            {
                colorizer.Colorize(color);
            }
        }

        public static void ColorizeSaber(this SaberType saber, Color? color)
        {
            if (saber.TryGetSaberColorizer(out SaberColorizer colorizer))
            {
                colorizer.Colorize(color);
            }
        }

        public static void ColorizeLight(this BeatmapEventType eventType, bool refresh, params Color?[] colors)
        {
            if (eventType.TryGetLightColorizer(out LightColorizer colorizer))
            {
                colorizer.Colorize(refresh, colors);
            }
        }
    }
}
