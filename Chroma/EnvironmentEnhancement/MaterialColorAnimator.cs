using System.Collections.Generic;
using Chroma.Animation;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;
using static Chroma.EnvironmentEnhancement.MaterialsManager;

namespace Chroma.EnvironmentEnhancement
{
    [UsedImplicitly]
    internal class MaterialColorAnimator : ITickable
    {
        private readonly HashSet<MaterialInfo> _activeMaterials = new();

        public void Tick()
        {
            foreach (MaterialInfo materialInfo in _activeMaterials)
            {
                AnimationHelper.GetColorOffset(null, materialInfo.Track, 0, out Color? color);
                if (color.HasValue)
                {
                    materialInfo.Material.color = color.Value;
                }
            }
        }

        internal void Add(MaterialInfo materialInfo)
        {
            _activeMaterials.Add(materialInfo);
        }
    }
}
