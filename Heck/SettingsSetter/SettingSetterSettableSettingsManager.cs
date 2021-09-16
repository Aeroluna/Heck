namespace Heck.SettingsSetter
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using Newtonsoft.Json;

    public static class SettingSetterSettableSettingsManager
    {
        private static Dictionary<string, List<Dictionary<string, object>>>? _settingsTable;

        internal static Dictionary<string, List<Dictionary<string, object>>> SettingsTable => _settingsTable ?? throw new InvalidOperationException($"[{nameof(_settingsTable)}] was not created.");

        internal static Dictionary<string, Dictionary<string, ISettableSetting>> SettableSettings { get; } = new Dictionary<string, Dictionary<string, ISettableSetting>>();

        public static void RegisterSettableSetting(string group, string fieldName, ISettableSetting settableSetting)
        {
            if (!SettableSettings.TryGetValue(group, out Dictionary<string, ISettableSetting> groupSettings))
            {
                groupSettings = new Dictionary<string, ISettableSetting>();
                SettableSettings[group] = groupSettings;
            }

            if (groupSettings.ContainsKey(fieldName))
            {
                throw new ArgumentException($"Group [{group}] already contains [{fieldName}].", nameof(fieldName));
            }

            groupSettings[fieldName] = settableSetting;
        }

        internal static void SetupSettingsTable()
        {
            using JsonReader reader = new JsonTextReader(new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Heck.SettingsSetter.SettingsSetterSettableSettings.json")));
            Dictionary<int, int>[] typeTable = new Dictionary<int, int>[8];

            _settingsTable = new JsonSerializer().Deserialize<Dictionary<string, List<Dictionary<string, object>>>>(reader) ?? throw new InvalidOperationException($"Failed to deserialize settings table.");
        }
    }
}
