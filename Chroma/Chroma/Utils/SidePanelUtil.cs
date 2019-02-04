using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Chroma.Utils {
    
    public static class SidePanelUtil {

        private const string defaultKey = "default";
        private static Dictionary<string, string> screenText = new Dictionary<string, string>();

        private static TextPageScrollView textPageScrollView;

        private static string currentKey = defaultKey;
        private static string currentMessage = "";

        /*private static ReleaseInfoViewController infoView;
        private static TextMeshProUGUI tmText;
        
        private static bool Initialize() {
            try {
                if (infoView != null) return true;
                infoView = GameObject.FindObjectOfType<ReleaseInfoViewController>();
                if (infoView == null) infoView = Resources.FindObjectsOfTypeAll<ReleaseInfoViewController>().FirstOrDefault();
                if (infoView != null) {
                    textPageScrollView = infoView.GetField<TextPageScrollView>("_textPageScrollView");
                    if (textPageScrollView != null) {
                        tmText = textPageScrollView.GetField<TextMeshProUGUI>("_text");
                        if (tmText != null) {
                            if (!screenText.ContainsKey(defaultKey)) screenText.Add(defaultKey, tmText.text);
                            ChromaLogger.Log(tmText.text);
                            return true;
                        } else {
                            ChromaLogger.Log("Failure to get TextMeshProUGUI", ChromaLogger.Level.ERROR);
                        }
                    } else {
                        ChromaLogger.Log("Failure to get TextPageScrollView", ChromaLogger.Level.ERROR);
                    }
                } else {
                    ChromaLogger.Log("Failure to get InfoView", ChromaLogger.Level.ERROR);
                }
            } catch (Exception e) {
                ChromaLogger.Log(e);
            }
            return false;
        }*/

        /// <summary>
        /// Revert the panel to default
        /// </summary>
        public static void ResetPanel() {
            SetPanel(defaultKey);
        }

        /// <summary>
        /// Sets the panel to the text provided via RegisterTextPanel
        /// </summary>
        /// <param name="key">The key used in RegisterTextPanel</param>
        /// <returns>true if the key exists</returns>
        public static bool SetPanel(string key) {
            if (key == null) return false;
            if (screenText.TryGetValue(key, out string message)) {
                SetPanelDirectly(message, key);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Registers a key/message for the text panel
        /// </summary>
        /// <param name="key">key to use for EnablePanel</param>
        /// <param name="message">Message to be shown</param>
        public static void RegisterTextPanel(string key, string message) {
            if (screenText.ContainsKey(key)) {
                screenText[key] = message;
            } else {
                screenText.Add(key, message);
            }
        }

        /// <summary>
        /// Sets the panel directly via message
        /// </summary>
        /// <param name="message">Message to be shown</param>
        public static void SetPanelDirectly(string message, string key = null) {
            currentKey = key;
            currentMessage = message;
            textPageScrollView.SetText(message);
        }

        /// <summary>
        /// Reapplies the current key
        /// </summary>
        public static void Update() {
            SetPanel(currentKey);
        }


        internal static void ReleaseInfoEnabled(ReleaseInfoViewController instance, TextPageScrollView textPageScrollView, string message) {
            SidePanelUtil.textPageScrollView = textPageScrollView;
            if (!screenText.ContainsKey(defaultKey)) {
                screenText.Add(defaultKey, message);
                currentKey = defaultKey;
                currentMessage = message;
            }
            ReleaseInfoEnabledEvent?.Invoke();
        }

        public delegate void ReleaseInfoEnabledDelegate();
        public static event ReleaseInfoEnabledDelegate ReleaseInfoEnabledEvent;
    }

}
