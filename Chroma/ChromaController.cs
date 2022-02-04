using Heck;

namespace Chroma
{
    internal enum PatchType
    {
        Colorizer,
        Features
    }

    internal static class ChromaController
    {
        internal const string ANIMATION = "_animation";
        internal const string COLOR = "_color";
        internal const string COUNTER_SPIN = "_counterSpin";
        internal const string DIRECTION = "_direction";
        internal const string DISABLE_SPAWN_EFFECT = "_disableSpawnEffect";
        internal const string DURATION = "_duration";
        internal const string EASING = "_easing";
        internal const string END_COLOR = "_endColor";
        internal const string ENVIRONMENT_REMOVAL = "_environmentRemoval";
        internal const string LERP_TYPE = "_lerpType";
        internal const string LIGHT_GRADIENT = "_lightGradient";
        internal const string LIGHT_ID = "_lightID";
        internal const string LOCK_POSITION = "_lockPosition";
        internal const string NAME_FILTER = "_nameFilter";
        internal const string PRECISE_SPEED = "_preciseSpeed";
        internal const string PROP = "_prop";
        internal const string PROPAGATION_ID = "_propID";
        internal const string PROP_MULT = "_propMult";
        internal const string RESET = "_reset";
        internal const string SPEED = "_speed";
        internal const string SPEED_MULT = "_speedMult";
        internal const string START_COLOR = "_startColor";
        internal const string STEP = "_step";
        internal const string STEP_MULT = "_stepMult";
        internal const string ROTATION = "_rotation";

        internal const string ENVIRONMENT = "_environment";
        internal const string GAMEOBJECT_ID = "_id";
        internal const string LOOKUP_METHOD = "_lookupMethod";
        internal const string DUPLICATION_AMOUNT = "_duplicate";
        internal const string ACTIVE = "_active";
        internal const string SCALE = "_scale";
        internal const string POSITION = "_position";
        internal const string LOCAL_POSITION = "_localPosition";
        internal const string OBJECT_ROTATION = "_rotation";
        internal const string LOCAL_ROTATION = "_localRotation";

        internal const string ASSIGN_FOG_TRACK = "AssignFogTrack";
        internal const string ATTENUATION = "_attenuation";
        internal const string OFFSET = "_offset";
        internal const string HEIGHT_FOG_STARTY = "_startY";
        internal const string HEIGHT_FOG_HEIGHT = "_height";

        internal const string CAPABILITY = "Chroma";
        internal const string ID = "Chroma";
        internal const string HARMONY_ID = "com.aeroluna.Chroma";

        internal static HeckPatcher CorePatcher { get; } = new(HARMONY_ID + "Core");

        internal static HeckPatcher ColorizerPatcher { get; } = new(HARMONY_ID + "Colorizer", PatchType.Colorizer);

        internal static HeckPatcher FeaturesPatcher { get; } = new(HARMONY_ID + "Features", PatchType.Features);

        internal static CustomDataDeserializer Deserializer { get; } = DeserializerManager.RegisterDeserialize<CustomDataManager>(ID);

        internal static Module ColorizerModule { get; } = ModuleManager.RegisterModule<ModuleCallbacks>(
            "ChromaColorizer",
            0,
            RequirementType.None,
            PatchType.Colorizer);

        internal static Module FeaturesModule { get; } = ModuleManager.RegisterModule<ModuleCallbacks>(
            "Chroma",
            2,
            RequirementType.Condition,
            PatchType.Features,
            new[] { "ChromaColorizer", "Heck" });
    }
}
