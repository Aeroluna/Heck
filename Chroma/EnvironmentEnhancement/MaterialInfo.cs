using System.Collections.Generic;
using Heck.Animation;
using UnityEngine;

namespace Chroma.EnvironmentEnhancement;

internal readonly struct MaterialInfo
{
    internal MaterialInfo(
        ShaderType shaderType,
        Material material,
        List<Track>? track)
    {
        ShaderType = shaderType;
        Material = material;
        Track = track;
    }

    internal ShaderType ShaderType { get; }

    internal Material Material { get; }

    internal List<Track>? Track { get; }
}
