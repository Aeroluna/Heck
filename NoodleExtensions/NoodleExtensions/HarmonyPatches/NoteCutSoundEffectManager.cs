namespace NoodleExtensions.HarmonyPatches
{
    using CustomJSONData;
    using CustomJSONData.CustomBeatmap;
    using static NoodleExtensions.Plugin;

    [NoodlePatch(typeof(NoteCutSoundEffectManager))]
    [NoodlePatch("BeatmapObjectCallback")]
    internal static class NoteCutSoundEffectManagerBeatmapObjectCallback
    {
        // Do not create a NoteCutSoundEffect for fake notes
#pragma warning disable SA1313
        private static bool Prefix(BeatmapObjectData beatmapObjectData)
#pragma warning restore SA1313
        {
            if (beatmapObjectData is CustomNoteData customNoteData)
            {
                dynamic dynData = customNoteData.customData;
                bool? fake = Trees.at(dynData, FAKENOTE);
                if (fake.HasValue && fake.Value)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
