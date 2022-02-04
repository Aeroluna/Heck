using System.Collections.Generic;
using Chroma.Colorizer;
using Chroma.Lighting;
using SiraUtil.Affinity;

namespace Chroma.HarmonyPatches.Colorizer.Initialize
{
    internal class LightWithIdRegisterer : IAffinity
    {
        private readonly Dictionary<ILightWithId, int> _requestedIds = new();
        private readonly HashSet<ILightWithId> _needToRegister = new();
        private readonly LightColorizerManager _colorizerManager;

        private LightWithIdRegisterer(LightColorizerManager colorizerManager)
        {
            _colorizerManager = colorizerManager;
        }

        internal void SetRequestedId(ILightWithId lightWithId, int id)
        {
            _requestedIds[lightWithId] = id;
        }

        internal void MarkForTableRegister(ILightWithId lightWithId)
        {
            _needToRegister.Add(lightWithId);
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(LightWithIdManager), nameof(LightWithIdManager.RegisterLight))]
        private bool Prefix(
            LightWithIdManager __instance,
            ILightWithId lightWithId,
            List<ILightWithId>?[] ____lights,
            List<ILightWithId> ____lightsToUnregister)
        {
            // TODO: figure this shit out
            // for some reason, despite being an affinity patch bound to player, this still runs in the menu scene
            // so quick and dirty fix
            if (__instance.gameObject.scene.name.Contains("Menu"))
            {
                return true;
            }

            // TODO: swap to transpiler to avoid overriding
            if (lightWithId.isRegistered)
            {
                return false;
            }

            int lightId = lightWithId.lightId;
            if (lightId == -1)
            {
                return false;
            }

            List<ILightWithId>? lights = ____lights[lightId];
            if (lights == null)
            {
                lights = new List<ILightWithId>(10);
                ____lights[lightId] = lights;
            }

            int type = lightId - 1;

            // TODO: find a better way to register "new" lights to table
            int index = lights.Count;
            if (_needToRegister.Contains(lightWithId))
            {
                int? tableId = _requestedIds.TryGetValue(lightWithId, out int value) ? value : null;
                LightIDTableManager.RegisterIndex(lightId - 1, index, tableId);
            }

            lightWithId.__SetIsRegistered();

            // this also colors the light
            _colorizerManager.CreateLightColorizerContract((BeatmapEventType)type, n => n.ChromaLightSwitchEventEffect.RegisterLight(lightWithId, index));

            lights.Add(lightWithId);
            ____lightsToUnregister.Remove(lightWithId);

            return false;
        }

        // TODO: add logic for unregistering
    }
}
