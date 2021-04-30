namespace Chroma.HarmonyPatches
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using Chroma.Settings;
    using CustomJSONData;
    using CustomJSONData.CustomBeatmap;
    using HarmonyLib;

    internal static class SceneTransitionHelper
    {
        private static readonly MethodInfo _setEnvironmentTable = SymbolExtensions.GetMethodInfo(() => SetEnvironmentTable(null));

        internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();
            bool foundReturn = false;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (!foundReturn &&
                    instructionList[i].opcode == OpCodes.Ret)
                {
                    foundReturn = true;

                    instructionList.Insert(i, new CodeInstruction(OpCodes.Ldloc_0));
                    instructionList.Insert(i + 1, new CodeInstruction(OpCodes.Call, _setEnvironmentTable));
                }
            }

            if (!foundReturn)
            {
                Plugin.Logger.Log("Failed to find ret!", IPA.Logging.Logger.Level.Error);
            }

            return instructionList.AsEnumerable();
        }

        internal static void Patch(IDifficultyBeatmap difficultyBeatmap)
        {
            if (difficultyBeatmap.beatmapData is CustomBeatmapData customBeatmapData)
            {
                BasicPatch(customBeatmapData);
            }
        }

        internal static void Patch(IDifficultyBeatmap difficultyBeatmap, ref OverrideEnvironmentSettings overrideEnvironmentSettings)
        {
            if (difficultyBeatmap.beatmapData is CustomBeatmapData customBeatmapData)
            {
                bool chromaRequirement = BasicPatch(customBeatmapData);
                if (chromaRequirement &&
                    ChromaConfig.Instance.EnvironmentEnhancementsEnabled &&
                    (Trees.at(customBeatmapData.beatmapCustomData, Chroma.Plugin.ENVIRONMENTREMOVAL) != null || Trees.at(customBeatmapData.customData, Chroma.Plugin.ENVIRONMENT) != null))
                {
                    overrideEnvironmentSettings = null;
                }
            }
        }

        private static void SetEnvironmentTable(EnvironmentInfoSO environmentInfo)
        {
            LightIDTableManager.SetEnvironment(environmentInfo.serializedName);
        }

        private static bool BasicPatch(CustomBeatmapData customBeatmapData)
        {
            IEnumerable<string> requirements = ((List<object>)Trees.at(customBeatmapData.beatmapCustomData, "_requirements"))?.Cast<string>();
            IEnumerable<string> suggestions = ((List<object>)Trees.at(customBeatmapData.beatmapCustomData, "_suggestions"))?.Cast<string>();
            bool chromaRequirement = (requirements?.Contains(Chroma.Plugin.REQUIREMENTNAME) ?? false) || (suggestions?.Contains(Chroma.Plugin.REQUIREMENTNAME) ?? false);

            // please let me remove this shit
            bool legacyOverride = customBeatmapData.beatmapEventsData.Any(n => n.value >= LegacyLightHelper.RGB_INT_OFFSET);
            if (legacyOverride)
            {
                Plugin.Logger.Log("Legacy Chroma Detected...", IPA.Logging.Logger.Level.Warning);
                Plugin.Logger.Log("Please do not use Legacy Chroma for new maps as it is deprecated and its functionality in future versions of Chroma cannot be guaranteed", IPA.Logging.Logger.Level.Warning);
            }

            ChromaController.ToggleChromaPatches((chromaRequirement || legacyOverride) && ChromaConfig.Instance.CustomColorEventsEnabled);
            ChromaController.DoColorizerSabers = chromaRequirement && ChromaConfig.Instance.CustomColorEventsEnabled;

            return chromaRequirement;
        }
    }
}
