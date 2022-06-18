using System;
using System.Collections.Generic;
using System.Linq;
using Chroma.Extras;
using CustomJSONData.CustomBeatmap;
using IPA.Utilities;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Rendering;
using Zenject;
using static Chroma.ChromaController;
using Object = UnityEngine.Object;

namespace Chroma.EnvironmentEnhancement
{
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

    internal enum ShaderPreset
    {
        Standard,
        None,
        Light
    }

    internal class GeometryFactory
    {
        private static readonly FieldAccessor<TubeBloomPrePassLight, float>.Accessor _colorAlphaMultiplierAccessor = FieldAccessor<TubeBloomPrePassLight, float>.GetAccessor("_colorAlphaMultiplier");
        private static readonly FieldAccessor<TubeBloomPrePassLight, BoolSO>.Accessor _mainEffectPostProcessEnabledAccessor = FieldAccessor<TubeBloomPrePassLight, BoolSO>.GetAccessor("_mainEffectPostProcessEnabled");
        private static readonly FieldAccessor<TubeBloomPrePassLight, bool>.Accessor _forceUseBakedGlowAccessor = FieldAccessor<TubeBloomPrePassLight, bool>.GetAccessor("_forceUseBakedGlow");
        private static readonly FieldAccessor<TubeBloomPrePassLight, Parametric3SliceSpriteController>.Accessor _dynamic3SliceSpriteAccessor = FieldAccessor<TubeBloomPrePassLight, Parametric3SliceSpriteController>.GetAccessor("_dynamic3SliceSprite");
        private static readonly FieldAccessor<BloomPrePassLight, BloomPrePassLightTypeSO>.Accessor _lightTypeAccessor = FieldAccessor<BloomPrePassLight, BloomPrePassLightTypeSO>.GetAccessor("_lightType");
        private static readonly FieldAccessor<BloomPrePassLight, BloomPrePassLightTypeSO>.Accessor _registeredWithLightTypeAccessor = FieldAccessor<BloomPrePassLight, BloomPrePassLightTypeSO>.GetAccessor("_registeredWithLightType");
        private static readonly FieldAccessor<TubeBloomPrePassLightWithId, TubeBloomPrePassLight>.Accessor _tubeBloomPrePassLightAccessor = FieldAccessor<TubeBloomPrePassLightWithId, TubeBloomPrePassLight>.GetAccessor("_tubeBloomPrePassLight");

        // Specular and Standard are built in Unity
        // TODO: Make a material programatically instead of relying on this
        // This is the shader BTS Cube uses
        private static readonly Material _standardMaterial = new(Shader.Find("Custom/SimpleLit"));
        private static readonly Material _lightMaterial = new(Shader.Find("Custom/OpaqueNeonLight"));

        private static readonly Dictionary<(Color, ShaderPreset), Material> _cachedMaterials = new();
        private static TubeBloomPrePassLight? _originalTubeBloomPrePassLight = Resources.FindObjectsOfTypeAll<TubeBloomPrePassLight>().FirstOrDefault();

        private readonly IInstantiator _instantiator;

        [UsedImplicitly]
        private GeometryFactory(IInstantiator instantiator)
        {
            _instantiator = instantiator;
        }

        internal GameObject Create(CustomData customData)
        {
            Color color = CustomDataManager.GetColorFromData(customData) ?? Color.cyan;
            GeometryType geometryType = customData.GetStringToEnumRequired<GeometryType>(GEOMETRY_TYPE);
            ShaderPreset shaderPreset = customData.GetStringToEnum<ShaderPreset?>(SHADER_PRESET) ?? ShaderPreset.Standard;
            IEnumerable<string>? shaderKeywords = customData.Get<List<object>?>(SHADER_KEYWORDS)?.Cast<string>();

            // Omitted in Quest
            bool collision = customData.Get<bool?>(COLLISION) ?? false;

            PrimitiveType primitiveType = geometryType switch
            {
                GeometryType.Sphere => PrimitiveType.Sphere,
                GeometryType.Capsule => PrimitiveType.Capsule,
                GeometryType.Cylinder => PrimitiveType.Cylinder,
                GeometryType.Cube => PrimitiveType.Cube,
                GeometryType.Plane => PrimitiveType.Plane,
                GeometryType.Quad => PrimitiveType.Quad,
                GeometryType.Triangle => PrimitiveType.Quad,
                _ => throw new ArgumentOutOfRangeException($"Geometry type {geometryType} does not match a primitive!", nameof(geometryType))
            };

            GameObject gameObject = GameObject.CreatePrimitive(primitiveType);
            gameObject.name = $"{geometryType}{shaderPreset}";
            MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();

            // Disable expensive shadows
            meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
            meshRenderer.receiveShadows = false;

            // Shared material is usually better performance as far as I know
            Material material = GetMaterial(color, shaderPreset);
            meshRenderer.sharedMaterial = material;

            if (geometryType == GeometryType.Triangle)
            {
                Mesh mesh = ChromaUtils.CreateTriangleMesh();
                gameObject.GetComponent<MeshFilter>().sharedMesh = mesh;
                if (collision)
                {
                    MeshCollider meshCollider = gameObject.GetComponent<MeshCollider>();
                    if (meshCollider != null)
                    {
                        meshCollider.sharedMesh = mesh;
                    }
                }
            }

            if (!collision)
            {
                // destroy colliders
                Object.Destroy(gameObject.GetComponent<Collider>());
            }

            // THIS REDUCES PERFORMANCE FOR BULK RENDERING
            if (shaderKeywords != null)
            {
                meshRenderer.sharedMaterial = material = Object.Instantiate(meshRenderer.sharedMaterial);
                meshRenderer.sharedMaterial.shaderKeywords = shaderKeywords.ToArray();
            }

            // Handle light preset
            if (shaderPreset is not ShaderPreset.Light)
            {
                return gameObject;
            }

            // Stop TubeBloomPrePassLight from running OnEnable before I can set the fields
            gameObject.SetActive(false);

            // I have no clue how this works
            TubeBloomPrePassLight tubeBloomPrePassLight = gameObject.AddComponent<TubeBloomPrePassLight>();

            if (_originalTubeBloomPrePassLight != null)
            {
                _colorAlphaMultiplierAccessor(ref tubeBloomPrePassLight) = 10;
                _mainEffectPostProcessEnabledAccessor(ref tubeBloomPrePassLight) =
                    _mainEffectPostProcessEnabledAccessor(ref _originalTubeBloomPrePassLight);
                _forceUseBakedGlowAccessor(ref tubeBloomPrePassLight) =
                    _forceUseBakedGlowAccessor(ref _originalTubeBloomPrePassLight);
                _dynamic3SliceSpriteAccessor(ref tubeBloomPrePassLight) =
                    _dynamic3SliceSpriteAccessor(ref _originalTubeBloomPrePassLight);

                BloomPrePassLight bloomPrePassLight = tubeBloomPrePassLight;
                BloomPrePassLight originalBloomPrePassLight = _originalTubeBloomPrePassLight;

                _lightTypeAccessor(ref bloomPrePassLight) = _lightTypeAccessor(ref originalBloomPrePassLight);
                _registeredWithLightTypeAccessor(ref bloomPrePassLight) =
                    _registeredWithLightTypeAccessor(ref originalBloomPrePassLight);

                TubeBloomPrePassLightWithId lightWithId = _instantiator.InstantiateComponent<TubeBloomPrePassLightWithId>(gameObject);
                _tubeBloomPrePassLightAccessor(ref lightWithId) = tubeBloomPrePassLight;
                lightWithId.SetLightId(0);
            }
            else
            {
                throw new InvalidOperationException($"[{nameof(_originalTubeBloomPrePassLight)}] was null.");
            }

            gameObject.SetActive(true);

            return gameObject;
        }

        // Cache materials to improve bulk rendering performance
        // TODO: Cache shader keywords
        private static Material GetMaterial(Color color, ShaderPreset shaderPreset)
        {
            if (_cachedMaterials.TryGetValue((color, shaderPreset), out Material material))
            {
                return material;
            }

            Material originalMaterial = shaderPreset switch
            {
                ShaderPreset.Light => _lightMaterial,
                _ => _standardMaterial
            };

            _cachedMaterials[(color, shaderPreset)] = material = Object.Instantiate(originalMaterial);
            material.color = color;
            material.globalIlluminationFlags = shaderPreset switch
            {
                ShaderPreset.Light => MaterialGlobalIlluminationFlags.EmissiveIsBlack,
                _ => MaterialGlobalIlluminationFlags.RealtimeEmissive
            };

            material.enableInstancing = true;
            material.shaderKeywords = shaderPreset switch
            {
                // Keywords found in RUE PC in BS 1.23
                ShaderPreset.Standard => new[]
                {
                    "DIFFUSE", "ENABLE_DIFFUSE", "ENABLE_FOG", "ENABLE_HEIGHT_FOG", "ENABLE_SPECULAR", "FOG",
                    "HEIGHT_FOG", "REFLECTION_PROBE_BOX_PROJECTION", "SPECULAR", "_EMISSION",
                    "_ENABLE_FOG_TINT", "_RIMLIGHT_NONE", "_ZWRITE_ON", "REFLECTION_PROBE", "LIGHT_FALLOFF"
                },
                ShaderPreset.Light => new[]
                {
                    "DIFFUSE", "ENABLE_BLUE_NOISE", "ENABLE_DIFFUSE", "ENABLE_HEIGHT_FOG", "ENABLE_LIGHTNING", "USE_COLOR_FOG"
                },
                _ => material.shaderKeywords
            };

            return material;
        }
    }
}
