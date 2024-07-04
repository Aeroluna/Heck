using Heck.Animation;
using Heck.Patcher;
using IPA;
using JetBrains.Annotations;
using NoodleExtensions.Installers;
using SiraUtil.Zenject;
using UnityEngine;
using static Heck.HeckController;
using static NoodleExtensions.NoodleController;
using Logger = IPA.Logging.Logger;

namespace NoodleExtensions
{
    [Plugin(RuntimeOptions.DynamicInit)]
    internal class Plugin
    {
        [UsedImplicitly]
        [Init]
        public Plugin(Logger pluginLogger, Zenjector zenjector)
        {
            Log = pluginLogger;

            zenjector.Install<NoodleAppInstaller>(Location.App);
            zenjector.Install<NoodlePlayerInstaller>(Location.Player);
            zenjector.Expose<NoteCutCoreEffectsSpawner>("Gameplay");
            zenjector.UseLogger(pluginLogger);

            HeckPatchManager.Register(HARMONY_ID);

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

        internal static Logger Log { get; private set; } = null!;

#pragma warning disable CA1822
        [UsedImplicitly]
        [OnEnable]
        public void OnEnable()
        {
            Capability.Register();
            JSONDeserializer.Enabled = true;
        }

        [UsedImplicitly]
        [OnDisable]
        public void OnDisable()
        {
            Capability.Deregister();
            JSONDeserializer.Enabled = false;
        }
#pragma warning restore CA1822
    }
}
