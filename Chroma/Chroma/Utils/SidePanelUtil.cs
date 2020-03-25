using System.Collections.Generic;

namespace Chroma.Utils
{
    internal static class SidePanelUtil
    {
        private const string defaultKey = "default";
        private static Dictionary<string, string> screenText = new Dictionary<string, string>();

        private static TextPageScrollView textPageScrollView;

        private static string currentKey = defaultKey;

        /// <summary>
        /// Sets the panel to the text provided via RegisterTextPanel
        /// </summary>
        /// <param name="key">The key used in RegisterTextPanel</param>
        /// <returns>true if the key exists</returns>
        internal static bool SetPanel(string key)
        {
            if (key == null) return false;
            if (screenText.TryGetValue(key, out string message))
            {
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
        internal static void RegisterTextPanel(string key, string message)
        {
            if (screenText.ContainsKey(key))
            {
                screenText[key] = message;
            }
            else
            {
                screenText.Add(key, message);
            }
        }

        /// <summary>
        /// Sets the panel directly via message
        /// </summary>
        /// <param name="message">Message to be shown</param>
        internal static void SetPanelDirectly(string message, string key = null)
        {
            currentKey = key;
            textPageScrollView.SetText(message);
        }

        /// <summary>
        /// Reapplies the current key
        /// </summary>
        private static void Update()
        {
            SetPanel(currentKey);
        }

        internal static void ReleaseInfoEnabled(ReleaseInfoViewController instance, TextPageScrollView textPageScrollView, string message)
        {
            SidePanelUtil.textPageScrollView = textPageScrollView;
            if (!screenText.ContainsKey(defaultKey))
            {
                screenText.Add(defaultKey, message);
                currentKey = defaultKey;
            }
            ReleaseInfoEnabledEvent?.Invoke();
        }

        internal delegate void ReleaseInfoEnabledDelegate();

        internal static event ReleaseInfoEnabledDelegate ReleaseInfoEnabledEvent;
    }
}