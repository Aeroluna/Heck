namespace Heck.SettingsSetter
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using Newtonsoft.Json;

    internal static class SettingSetterSettableSettingsManager
    {
        private static Dictionary<string, List<Dictionary<string, object>>>? _settingsTable;

        public static Dictionary<string, List<Dictionary<string, object>>> SettingsTable => _settingsTable ?? throw new InvalidOperationException($"[{nameof(_settingsTable)}] was not created.");

        internal static void SetupSettingsTable()
        {
            using JsonReader reader = new JsonTextReader(new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Heck.SettingsSetter.SettingsSetterSettableSettings.json")));
            Dictionary<int, int>[] typeTable = new Dictionary<int, int>[8];

            _settingsTable = new JsonSerializer().Deserialize<Dictionary<string, List<Dictionary<string, object>>>>(reader) ?? throw new InvalidOperationException($"Failed to deserialize settings table.");
        }
    }
}
