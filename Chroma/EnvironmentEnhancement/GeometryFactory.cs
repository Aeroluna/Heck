using System;
using System.Collections.Generic;
using System.Linq;
using Chroma.Colorizer;
using Chroma.Extras;
using Chroma.HarmonyPatches.Colorizer.Initialize;
using Chroma.HarmonyPatches.EnvironmentComponent;
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

    internal enum ShaderType
    {
        Standard,
        OpaqueLight,
        TransparentLight
    }

    internal class GeometryFactory
    {
        private static readonly FieldAccessor<TubeBloomPrePassLight, BoolSO>.Accessor _mainEffectPostProcessEnabledAccessor = FieldAccessor<TubeBloomPrePassLight, BoolSO>.GetAccessor("_mainEffectPostProcessEnabled");
        private static readonly FieldAccessor<TubeBloomPrePassLight, ParametricBoxController>.Accessor _parametricBoxControllerAccessor = FieldAccessor<TubeBloomPrePassLight, ParametricBoxController>.GetAccessor("_parametricBoxController");
        private static readonly FieldAccessor<BloomPrePassLight, BloomPrePassLightTypeSO>.Accessor _lightTypeAccessor = FieldAccessor<BloomPrePassLight, BloomPrePassLightTypeSO>.GetAccessor("_lightType");
        private static readonly FieldAccessor<BloomPrePassLight, BloomPrePassLightTypeSO>.Accessor _registeredWithLightTypeAccessor = FieldAccessor<BloomPrePassLight, BloomPrePassLightTypeSO>.GetAccessor("_registeredWithLightType");
        private static readonly FieldAccessor<TubeBloomPrePassLightWithId, TubeBloomPrePassLight>.Accessor _tubeBloomPrePassLightAccessor = FieldAccessor<TubeBloomPrePassLightWithId, TubeBloomPrePassLight>.GetAccessor("_tubeBloomPrePassLight");
        private static readonly FieldAccessor<ParametricBoxController, MeshRenderer>.Accessor _meshRendererAccessor = FieldAccessor<ParametricBoxController, MeshRenderer>.GetAccessor("_meshRenderer");

        private static readonly Material _standardMaterial = InstantiateSharedMaterial(ShaderType.Standard);
        private static readonly Material _opaqueLightMaterial = InstantiateSharedMaterial(ShaderType.OpaqueLight);
        private static readonly Material _transparentLightMaterial = InstantiateSharedMaterial(ShaderType.TransparentLight);

        private static readonly Dictionary<(Color, ShaderType), HashSet<(string[]?, Material)>> _cachedMaterials = new();

        private readonly IInstantiator _instantiator;
        private readonly LightWithIdRegisterer _lightWithIdRegisterer;
        private readonly ParametricBoxControllerTransformOverride _parametricBoxControllerTransformOverride;
        private TubeBloomPrePassLight? _originalTubeBloomPrePassLight = Resources.FindObjectsOfTypeAll<TubeBloomPrePassLight>().FirstOrDefault();

        [UsedImplicitly]
        private GeometryFactory(
            IInstantiator instantiator,
            LightColorizerManager lightColorizerManager,
            LightWithIdRegisterer lightWithIdRegisterer,
            ParametricBoxControllerTransformOverride parametricBoxControllerTransformOverride)
        {
            _instantiator = instantiator;
            _lightWithIdRegisterer = lightWithIdRegisterer;
            _parametricBoxControllerTransformOverride = parametricBoxControllerTransformOverride;
        }

        internal GameObject Create(CustomData customData)
        {
            Color color = CustomDataManager.GetColorFromData(customData) ?? new Color(0, 0, 0, 0);
            GeometryType geometryType = customData.GetStringToEnumRequired<GeometryType>(GEOMETRY_TYPE);
            ShaderType shaderType = customData.GetStringToEnum<ShaderType?>(SHADER_PRESET) ?? ShaderType.Standard;
            string[]? shaderKeywords = customData.Get<List<object>?>(SHADER_KEYWORDS)?.Cast<string>().ToArray();
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
            gameObject.name = $"{geometryType}{shaderType}";
            MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();

            // Disable expensive shadows
            meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
            meshRenderer.receiveShadows = false;

            // Shared material is usually better performance as far as I know
            Material material = GetMaterial(color, shaderType, shaderKeywords);
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

            // Handle light preset
            if (!IsLightType(shaderType))
            {
                return gameObject;
            }

            // Stop TubeBloomPrePassLight from running OnEnable before I can set the fields
            gameObject.SetActive(false);

            TubeBloomPrePassLight tubeBloomPrePassLight = gameObject.AddComponent<TubeBloomPrePassLight>();
            ParametricBoxController parametricBoxController = gameObject.AddComponent<ParametricBoxController>();
            _parametricBoxControllerTransformOverride.UpdatePosition(parametricBoxController);
            _parametricBoxControllerTransformOverride.UpdateScale(parametricBoxController);
            _meshRendererAccessor(ref parametricBoxController) = meshRenderer;

            if (_originalTubeBloomPrePassLight != null)
            {
                _mainEffectPostProcessEnabledAccessor(ref tubeBloomPrePassLight) =
                    _mainEffectPostProcessEnabledAccessor(ref _originalTubeBloomPrePassLight);

                BloomPrePassLight bloomPrePassLight = tubeBloomPrePassLight;
                BloomPrePassLight originalBloomPrePassLight = _originalTubeBloomPrePassLight;

                _lightTypeAccessor(ref bloomPrePassLight) = _lightTypeAccessor(ref originalBloomPrePassLight);
                _registeredWithLightTypeAccessor(ref bloomPrePassLight) =
                    _registeredWithLightTypeAccessor(ref originalBloomPrePassLight);
            }
            else
            {
                throw new InvalidOperationException($"[{nameof(_originalTubeBloomPrePassLight)}] was null.");
            }

            _parametricBoxControllerAccessor(ref tubeBloomPrePassLight) = parametricBoxController;
            TubeBloomPrePassLightWithId lightWithId = _instantiator.InstantiateComponent<TubeBloomPrePassLightWithId>(gameObject);
            _tubeBloomPrePassLightAccessor(ref lightWithId) = tubeBloomPrePassLight;
            _lightWithIdRegisterer.MarkForTableRegister(lightWithId);

            gameObject.SetActive(true);

            return gameObject;
        }

        private static Material InstantiateSharedMaterial(ShaderType shaderType)
        {
            return new Material(Shader.Find(shaderType switch
            {
                ShaderType.OpaqueLight => "Custom/OpaqueNeonLight",
                ShaderType.TransparentLight => "Custom/TransparentNeonLight",
                _ => "Custom/SimpleLit"
            }))
            {
                globalIlluminationFlags = IsLightType(shaderType)
                    ? MaterialGlobalIlluminationFlags.EmissiveIsBlack
                    : MaterialGlobalIlluminationFlags.RealtimeEmissive,
                enableInstancing = true,
                shaderKeywords = shaderType switch
                {
                    // Keywords found in RUE PC in BS 1.23
                    ShaderType.Standard => new[]
                    {
                        "DIFFUSE", "ENABLE_DIFFUSE", "ENABLE_FOG", "ENABLE_HEIGHT_FOG", "ENABLE_SPECULAR", "FOG",
                        "HEIGHT_FOG", "REFLECTION_PROBE_BOX_PROJECTION", "SPECULAR", "_EMISSION",
                        "_ENABLE_FOG_TINT", "_RIMLIGHT_NONE", "_ZWRITE_ON", "REFLECTION_PROBE", "LIGHT_FALLOFF"
                    },
                    ShaderType.OpaqueLight => new[]
                    {
                        "DIFFUSE", "ENABLE_BLUE_NOISE", "ENABLE_DIFFUSE", "ENABLE_HEIGHT_FOG", "ENABLE_LIGHTNING", "USE_COLOR_FOG"
                    },
                    ShaderType.TransparentLight => new[]
                    {
                        "ENABLE_HEIGHT_FOG", "MULTIPLY_COLOR_WITH_ALPHA", "_ENABLE_MAIN_EFFECT_WHITE_BOOST"
                    },
                    _ => Array.Empty<string>()
                }
            };
        }

        // Cache materials to improve bulk rendering performance
        private static Material GetMaterial(Color color, ShaderType shaderType, string[]? keywords)
        {
            if (_cachedMaterials.TryGetValue((color, shaderType), out HashSet<(string[]?, Material)> materials))
            {
                IEnumerable<(string[]?, Material)> cachedMaterials = materials.Where(n =>
                {
                    (string[]? strings, _) = n;
                    if (keywords == null && strings == null)
                    {
                        return true;
                    }

                    if (keywords != null && strings != null)
                    {
                        return strings.SequenceEqual(keywords);
                    }

                    return false;
                });

                foreach ((string[]?, Material) current in cachedMaterials)
                {
                    return current.Item2;
                }
            }
            else
            {
                materials = new HashSet<(string[]?, Material)>();
                _cachedMaterials[(color, shaderType)] = materials;
            }

            Material originalMaterial = shaderType switch
            {
                ShaderType.OpaqueLight => _opaqueLightMaterial,
                ShaderType.TransparentLight => _transparentLightMaterial,
                _ => _standardMaterial
            };
            Material material = Object.Instantiate(originalMaterial);
            material.color = color;
            if (keywords != null)
            {
                material.shaderKeywords = keywords;
            }

            materials.Add((keywords, material));
            return material;
        }

        private static bool IsLightType(ShaderType shaderType)
        {
            return shaderType is ShaderType.OpaqueLight or ShaderType.TransparentLight;
        }
    }
}
