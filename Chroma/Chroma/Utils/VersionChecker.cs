using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using SimpleJSON;

namespace Chroma.Utils {

    /*
     * VersionUtil improved by Caeden
     */
    class VersionChecker : MonoBehaviour {

        public static bool upToDate = false;
        public static string webVersion = null;

        private static VersionChecker _instance;
        public static VersionChecker Instance {
            get {
                if (_instance != null) return _instance;
                _instance = new GameObject("Counters+ | Version Checker").AddComponent<VersionChecker>();
                DontDestroyOnLoad(_instance.gameObject);
                return _instance;
            }
        }

        public static void GetOnlineVersion(string url, Action<bool, string, string> action = null) {
            Instance.StartCoroutine(GetOnlineVersionRoutine(url, action));
        }

        private static IEnumerator GetOnlineVersionRoutine(string url, Action<bool, string, string> action) {
            ChromaLogger.Log("Obtaining latest version information...");
            using (UnityWebRequest www = UnityWebRequest.Get(url)) {
                yield return (www.SendWebRequest());
                if (www.isHttpError || www.isNetworkError) {
                    action?.Invoke(false, null, "Failed to download version info");
                } else {
                    ChromaLogger.Log("Obtained latest version info!");
                    JSONNode node = JSON.Parse(www.downloadHandler.text);
                    foreach (JSONNode child in node.Children) {
                        try {
                            if (child["approval"]["status"] != "approved") continue;
                            string version = child["version"].Value;
                            upToDate = isLatestVersion(version);
                            webVersion = version;
                            action?.Invoke(upToDate, webVersion, null);
                            break;
                        } catch {
                            action?.Invoke(false, null, "Failed to download version info");
                        }
                    }
                }
            }
            ChromaLogger.Log("Finished hunting for version");
        }

        private static bool isLatestVersion(string downloadedVersion) {
            List<int> pluginVersion = new List<int>();
            List<int> webVersion = new List<int>();
            foreach (string num in ChromaPlugin.Instance.plugin.Version.Split('.')) {
                string parse = num;
                if (num.Contains("-")) parse = num.Split('-').First();
                pluginVersion.Add(int.Parse(parse));
            }
            foreach (string num in downloadedVersion.Split('.')) {
                webVersion.Add(int.Parse(num));
            }
            for (int i = 0; i < pluginVersion.Count(); i++) {
                if (pluginVersion[i] > webVersion[i]) return true;
            }
            if (ChromaPlugin.Instance.plugin.Version == downloadedVersion) return true;
            return false;
        }
    }


}
