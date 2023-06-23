using BepInEx;
using BepInEx.Logging;
using Heck.Animation;
using NoodleExtensions.Installers;
using SiraUtil.Zenject;
using SongCore;
using UnityEngine;
using static Heck.HeckController;
using static NoodleExtensions.NoodleController;

namespace NoodleExtensions
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency("SongCore")]
    [BepInDependency("CustomJSONData")]
    [BepInDependency("Heck")]
    [BepInDependency("SiraUtil")]
    [BepInProcess("Beat Saber.exe")]
    internal class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log { get; private set; } = null!;

        private void Awake()
        {
            Log = Logger;

            Zenjector zenjector = Zenjector.ConstructZenjector(Info);
            zenjector.Install<NoodleAppInstaller>(Location.App);
            zenjector.Install<NoodlePlayerInstaller>(Location.Player);
            zenjector.Expose<NoteCutCoreEffectsSpawner>("Gameplay");

            Track.RegisterProperty<Vector3>(OFFSET_POSITION, V2_POSITION);
            Track.RegisterProperty<Quaternion>(OFFSET_ROTATION, V2_ROTATION);
            Track.RegisterProperty<float>(DISSOLVE, V2_DISSOLVE);
            Track.RegisterProperty<float>(DISSOLVE_ARROW, V2_DISSOLVE_ARROW);
            Track.RegisterProperty<float>(TIME, V2_TIME);
            Track.RegisterProperty<float>(INTERACTABLE, V2_CUTTABLE);

            Track.RegisterPathProperty<Vector3>(OFFSET_POSITION, V2_POSITION);
            Track.RegisterPathProperty<Quaternion>(OFFSET_ROTATION, V2_ROTATION);
            Track.RegisterPathProperty<Vector3>(SCALE, V2_SCALE);
            Track.RegisterPathProperty<Quaternion>(LOCAL_ROTATION, V2_LOCAL_ROTATION);
            Track.RegisterPathProperty<Vector3>(DEFINITE_POSITION, V2_DEFINITE_POSITION);
            Track.RegisterPathProperty<float>(DISSOLVE, V2_DISSOLVE);
            Track.RegisterPathProperty<float>(DISSOLVE_ARROW, V2_DISSOLVE_ARROW);
            Track.RegisterPathProperty<float>(INTERACTABLE, V2_CUTTABLE);
        }

#pragma warning disable CA1822
        private void OnEnable()
        {
            Collections.RegisterCapability(CAPABILITY);
            CorePatcher.Enabled = true;
            FeaturesModule.Enabled = true;
            JSONDeserializer.Enabled = true;
        }

        private void OnDisable()
        {
            Collections.DeregisterizeCapability(CAPABILITY);
            CorePatcher.Enabled = false;
            FeaturesPatcher.Enabled = false;
            FeaturesModule.Enabled = false;
            Deserializer.Enabled = false;
            JSONDeserializer.Enabled = false;
        }
#pragma warning restore CA1822
    }
}
