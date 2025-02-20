namespace Chroma.EnvironmentEnhancement;

// ReSharper disable UnusedMember.Global
internal enum GeometryType
{
    Sphere,
    Capsule,
    Cylinder,
    Cube,
    Plane,
    Quad,
    Triangle
}

// ReSharper disable once InconsistentNaming
internal enum ShaderType
{
    Standard,
    OpaqueLight,
    TransparentLight,
    BaseWater,
    BillieWater,
    BTSPillar,
    InterscopeConcrete,
    InterscopeCar,
    Obstacle,
    WaterfallMirror,
    Glowing
}

internal static class ShaderTypeExtensions
{
    internal static bool IsLightType(this ShaderType shaderType)
    {
        return shaderType is ShaderType.OpaqueLight or ShaderType.TransparentLight or ShaderType.BillieWater;
    }
}
