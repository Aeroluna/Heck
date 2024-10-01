using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using SiraUtil.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;
#if !PRE_V1_37_1
using UnityEngine.AddressableAssets;
using _AsyncOperation =
    UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<
        UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>;
#else
using _AsyncOperation = UnityEngine.AsyncOperation;
#endif

namespace Chroma.EnvironmentEnhancement;

internal class EnvironmentMaterialsManager : MonoBehaviour
{
    private Dictionary<ShaderType, Material>? _environmentMaterials;
    private SiraLog _log = null!;

#if !PRE_V1_37_1
    private Shader? _waterLit;
#endif

    internal Dictionary<ShaderType, Material> EnvironmentMaterials
    {
        get
        {
            if (_environmentMaterials == null)
            {
                throw new InvalidOperationException("Environment materials not yet fetched!");
            }

            return _environmentMaterials;
        }
    }

#if !PRE_V1_37_1
    internal Shader WaterLit
    {
        get
        {
            if (_waterLit == null)
            {
                throw new InvalidOperationException("Environment materials not yet fetched!");
            }

            return _waterLit;
        }
    }
#endif

    private IEnumerator Activate()
    {
        if (_environmentMaterials != null)
        {
            yield break;
        }

        string[] environments = ["BTSEnvironment", "BillieEnvironment", "InterscopeEnvironment"];
        IEnumerable<_AsyncOperation> loads = environments.Select(Load).ToArray();
        foreach (_AsyncOperation asyncOperationHandle in loads)
        {
            yield return asyncOperationHandle;
        }

        Material[] environmentMaterials = Resources.FindObjectsOfTypeAll<Material>();

        _environmentMaterials = new Dictionary<ShaderType, Material>();
        Save(ShaderType.BTSPillar, "BTSDarkEnvironmentWithHeightFog");
        Save(ShaderType.BillieWater, "WaterfallFalling");
        Save(ShaderType.WaterfallMirror, "WaterfallMirror");
        Save(ShaderType.InterscopeConcrete, "Concrete2");
        Save(ShaderType.InterscopeCar, "Car");
#if !PRE_V1_37_1
        _waterLit = Resources.FindObjectsOfTypeAll<Shader>().First(n => n.name == "Custom/WaterLit");
#endif

        foreach (string environment in environments)
        {
            SceneManager.UnloadSceneAsync(environment);
        }

        yield break;

        void Save(ShaderType key, string matName)
        {
            Material? material = environmentMaterials.FirstOrDefault(e => e.name == matName);
            if (material != null)
            {
#if !PRE_V1_37_1
                // must be copied because the original gets unloaded.
                material = new Material(material);
#endif
                _environmentMaterials[key] = material;
                _log.Trace($"Saving [{matName}] to [{key}]");
            }
            else
            {
                _log.Error($"Could not find [{matName}]");
            }
        }

        _AsyncOperation Load(string environmentName)
        {
            _log.Trace($"Loading environment [{environmentName}]");
#if !PRE_V1_37_1
            return Addressables.LoadSceneAsync(environmentName, LoadSceneMode.Additive, true, int.MaxValue);
#else
            return SceneManager.LoadSceneAsync(environmentName, LoadSceneMode.Additive) ??
                   throw new InvalidOperationException($"Failed to load scene {environmentName}");
#endif
        }
    }

    [UsedImplicitly]
    [Inject]
    private void Construct(SiraLog log)
    {
        _log = log;
    }

    private void Initialize()
    {
        StartCoroutine(Activate());
    }

    // Exists because AppInit.GetAppStartType check scenecount for some reason and we cannot load extra scenes during appinit
    // credit to meivyn for figuring out this bug
    internal class EnvironmentMaterialsManagerInitializer : IInitializable
    {
        private readonly EnvironmentMaterialsManager _environmentMaterialsManager;

        [UsedImplicitly]
        private EnvironmentMaterialsManagerInitializer(EnvironmentMaterialsManager environmentMaterialsManager)
        {
            _environmentMaterialsManager = environmentMaterialsManager;
        }

        public void Initialize()
        {
            _environmentMaterialsManager.Initialize();
        }
    }
}
