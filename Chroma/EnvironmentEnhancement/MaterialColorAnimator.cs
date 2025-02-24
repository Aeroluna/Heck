using System.Collections.Generic;
using Chroma.Animation;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;

namespace Chroma.EnvironmentEnhancement;

[UsedImplicitly]
internal class MaterialColorAnimator : ITickable
{
    private readonly HashSet<MaterialInfo> _activeMaterials = [];

    public void Tick()
    {
        foreach (MaterialInfo materialInfo in _activeMaterials)
        {
            AnimationHelper.GetColorOffset(null, materialInfo.Track, 0, out Color? color);
            if (!color.HasValue)
            {
                continue;
            }

#if !PRE_V1_39_1
            if (materialInfo.ShaderType is ShaderType.Standard or ShaderType.BTSPillar &&
                materialInfo.Material.shaderKeywords is { Length: 0 })
            {
                color = color.Value.ColorWithAlpha(0);
            }
#endif
            materialInfo.Material.color = color.Value;
        }
    }

    internal void Add(MaterialInfo materialInfo)
    {
        _activeMaterials.Add(materialInfo);
    }
}
