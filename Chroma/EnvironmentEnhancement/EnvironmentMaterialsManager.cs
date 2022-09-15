using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Logger = IPA.Logging.Logger;

namespace Chroma.EnvironmentEnhancement
{
    internal class EnvironmentMaterialsManager : MonoBehaviour
    {
        internal Dictionary<ShaderType, Material> EnvironmentMaterials { get; } = new();

        private void Start()
        {
            StartCoroutine(Activate());
        }

        private IEnumerator Activate()
        {
            AsyncOperation Load(string environmentName)
            {
                Log.Logger.Log($"Loading environment [{environmentName}].", Logger.Level.Trace);
                return SceneManager.LoadSceneAsync(environmentName, LoadSceneMode.Additive);
            }

            string[] environments = { "BTSEnvironment", "BillieEnvironment", "InterscopeEnvironment" };
            AsyncOperation[] loads = environments.Select(Load).ToArray();

            while (loads.Any(n => !n.isDone))
            {
                yield return null;
            }

            Material[] environmentMaterials = Resources.FindObjectsOfTypeAll<Material>();

            void Save(ShaderType key, string matName)
            {
                Material? material = environmentMaterials.FirstOrDefault(e => e.name == matName);
                if (material != null)
                {
                    EnvironmentMaterials[key] = material;
                    Log.Logger.Log($"Saving [{matName}] to [{key}].", Logger.Level.Trace);
                }
                else
                {
                    Log.Logger.Log($"Could not find [{matName}].", Logger.Level.Error);
                }
            }

            Save(ShaderType.BTSPillar, "BTSDarkEnvironmentWithHeightFog");
            Save(ShaderType.BillieWater, "WaterfallFalling");
            Save(ShaderType.WaterfallMirror, "WaterfallMirror");
            Save(ShaderType.InterscopeConcrete, "Concrete2");
            Save(ShaderType.InterscopeCar, "Car");

            foreach (string environment in environments)
            {
                SceneManager.UnloadSceneAsync(environment);
            }
        }
    }
}
