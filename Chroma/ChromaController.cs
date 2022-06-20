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
        internal const string V2_COLOR = "_color";
        internal const string V2_COUNTER_SPIN = "_counterSpin";
        internal const string V2_DIRECTION = "_direction";
        internal const string V2_DISABLE_SPAWN_EFFECT = "_disableSpawnEffect";
        internal const string V2_END_COLOR = "_endColor";
        internal const string V2_ENVIRONMENT_REMOVAL = "_environmentRemoval";
        internal const string V2_LERP_TYPE = "_lerpType";
        internal const string V2_LIGHT_GRADIENT = "_lightGradient";
        internal const string V2_LIGHT_ID = "_lightID";
        internal const string V2_LOCK_POSITION = "_lockPosition";
        internal const string V2_NAME_FILTER = "_nameFilter";
        internal const string V2_PRECISE_SPEED = "_preciseSpeed";
        internal const string V2_PROP = "_prop";
        internal const string V2_PROPAGATION_ID = "_propID";
        internal const string V2_PROP_MULT = "_propMult";
        internal const string V2_RESET = "_reset";
        internal const string V2_SPEED = "_speed";
        internal const string V2_SPEED_MULT = "_speedMult";
        internal const string V2_START_COLOR = "_startColor";
        internal const string V2_STEP = "_step";
        internal const string V2_STEP_MULT = "_stepMult";

        internal const string V2_ENVIRONMENT = "_environment";
        internal const string V2_GAMEOBJECT_ID = "_id";
        internal const string V2_LOOKUP_METHOD = "_lookupMethod";
        internal const string V2_DUPLICATION_AMOUNT = "_duplicate";
        internal const string V2_ACTIVE = "_active";

        internal const string V2_ATTENUATION = "_attenuation";
        internal const string V2_OFFSET = "_offset";
        internal const string V2_HEIGHT_FOG_STARTY = "_startY";
        internal const string V2_HEIGHT_FOG_HEIGHT = "_height";

        internal const string COLOR = "color";
        internal const string DIRECTION = "direction";
        internal const string NOTE_SPAWN_EFFECT = "spawnEffect";
        internal const string LERP_TYPE = "lerpType";
        internal const string LIGHT_ID = "lightID";
        internal const string LOCK_POSITION = "lockRotation";
        internal const string NAME_FILTER = "nameFilter";
        internal const string PROP = "prop";
        internal const string SPEED = "speed";
        internal const string STEP = "step";
        internal const string RING_ROTATION = "rotation";

        internal const string ENVIRONMENT = "environment";
        internal const string GEOMETRY = "geometry";
        internal const string GEOMETRY_TYPE = "type";
        internal const string SHADER_PRESET = "shader";
        internal const string SHADER_KEYWORDS = "shaderKeywords";
        internal const string COLLISION = "collision";
        internal const string GAMEOBJECT_ID = "id";
        internal const string LOOKUP_METHOD = "lookupMethod";
        internal const string DUPLICATION_AMOUNT = "duplicate";
        internal const string ACTIVE = "active";
        internal const string ASSIGN_FOG_TRACK = "AssignFogTrack";

        internal const string CAPABILITY = "Chroma";
        internal const string ID = "Chroma";
        internal const string HARMONY_ID = "aeroluna.Chroma";

        internal static HeckPatcher CorePatcher { get; } = new(HARMONY_ID + "Core");

        internal static HeckPatcher ColorizerPatcher { get; } = new(HARMONY_ID + "Colorizer", PatchType.Colorizer);

        internal static HeckPatcher FeaturesPatcher { get; } = new(HARMONY_ID + "Features", PatchType.Features);

        internal static DataDeserializer Deserializer { get; } = DeserializerManager.Register<CustomDataManager>(ID);

        internal static Module ColorizerModule { get; } = ModuleManager.RegisterModule<ModuleCallbacks>(
            "ChromaColorizer",
            0,
            RequirementType.None,
            PatchType.Colorizer,
            new[] { "Heck" });

        internal static Module FeaturesModule { get; } = ModuleManager.RegisterModule<ModuleCallbacks>(
            "Chroma",
            2,
            RequirementType.Condition,
            PatchType.Features,
            new[] { "ChromaColorizer" });
    }
}
