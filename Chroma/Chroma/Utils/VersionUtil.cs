using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Chroma.Utils {

    public class VersionUtil : MonoBehaviour {

        private static VersionUtil _instance;
        public static VersionUtil Instance {
            get {
                if (_instance == null) {
                    GameObject go = new GameObject("ChromaDownloader");
                    _instance = go.AddComponent<VersionUtil>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        public static void GetOnlineVersionInfo(Version compareVersion, string url, Action<Version, string, JSONNode, string> action) {
            Instance.StartCoroutine(GetOnlineVersionRoutine(compareVersion, url, action));
        }

        private static IEnumerator GetOnlineVersionRoutine(Version compareVersion, string url, Action<Version, string, JSONNode, string> action) {

            ChromaLogger.Log("Getting online version info from " + url);

            string errorMessage = null;
            string data = null;
            JSONNode node = null;

            using (UnityWebRequest www = UnityWebRequest.Get(url)) {
                yield return (www.SendWebRequest());

                ChromaLogger.Log("Downloaded online version info!");

                if (www.isHttpError || www.isNetworkError) {
                    ChromaLogger.Log(www.error, ChromaLogger.Level.ERROR, false);
                    errorMessage = "Failed to download version info " + Environment.NewLine + www.error;
                } else {
                    data = www.downloadHandler.text;

                    if (data != null) {
                        if (data == "Not Found") errorMessage = "Online version info \"not found\".";
                        else node = JSON.Parse(data);
                    }

                }

                ChromaLogger.Log("Passing along online info...");

                action(compareVersion, data, node, errorMessage);

            }

        }

    }

}
