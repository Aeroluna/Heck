#if !LATEST
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using SiraUtil.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace Chroma.EnvironmentEnhancement
{
    internal class EnvironmentMaterialsManager : MonoBehaviour
    {
        private SiraLog _log = null!;

        private Dictionary<ShaderType, Material>? _environmentMaterials;

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

        private IEnumerator Activate()
        {
            if (_environmentMaterials != null)
            {
                yield break;
            }

            string[] environments = { "BTSEnvironment", "BillieEnvironment", "InterscopeEnvironment" };
            AsyncOperation[] loads = environments.Select(Load).ToArray();

            while (loads.Any(n => !n.isDone))
            {
                yield return null;
            }

            Material[] environmentMaterials = Resources.FindObjectsOfTypeAll<Material>();

            _environmentMaterials = new Dictionary<ShaderType, Material>();
            Save(ShaderType.BTSPillar, "BTSDarkEnvironmentWithHeightFog");
            Save(ShaderType.BillieWater, "WaterfallFalling");
            Save(ShaderType.WaterfallMirror, "WaterfallMirror");
            Save(ShaderType.InterscopeConcrete, "Concrete2");
            Save(ShaderType.InterscopeCar, "Car");

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
                    _environmentMaterials[key] = material;
                    _log.Trace($"Saving [{matName}] to [{key}]");
                }
                else
                {
                    _log.Trace($"Could not find [{matName}]");
                }
            }

            AsyncOperation Load(string environmentName)
            {
                _log.Trace($"Loading environment [{environmentName}]");
                return SceneManager.LoadSceneAsync(environmentName, LoadSceneMode.Additive);
            }
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
}
#endif
