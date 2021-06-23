namespace Chroma
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Newtonsoft.Json;

    internal static class LightIDTableManager
    {
        private static readonly Dictionary<string, Dictionary<int, int>[]> _lightIDTable = new Dictionary<string, Dictionary<int, int>[]>();

        private static Dictionary<int, int>[] _activeTable;

        internal static int? GetActiveTableValue(int type, int id)
        {
            if (_activeTable != null)
            {
                if (_activeTable[type].TryGetValue(id, out int newId))
                {
                    return newId;
                }
                else
                {
                    Plugin.Logger.Log($"Unable to find value for type [{type}] and id [{id}].", IPA.Logging.Logger.Level.Error);
                }
            }

            return null;
        }

        internal static void SetEnvironment(string environmentName)
        {
            if (_lightIDTable.TryGetValue(environmentName, out Dictionary<int, int>[] activeTable))
            {
                _activeTable = activeTable.Select(n => new Dictionary<int, int>(n)).ToArray();
            }
            else
            {
                _activeTable = null;
                Plugin.Logger.Log($"Table not found for: {environmentName}", IPA.Logging.Logger.Level.Warning);
            }
        }

        internal static void RegisterIndex(int type, int index, int? requestedKey)
        {
            Dictionary<int, int> dictioanry = _activeTable[type];
            int key;

            if (requestedKey.HasValue)
            {
                key = requestedKey.Value;
                while (dictioanry.ContainsKey(key))
                {
                    key++;
                }
            }
            else
            {
                key = dictioanry.Keys.Max() + 1;
            }

            dictioanry.Add(key, index);
            if (Settings.ChromaConfig.Instance.PrintEnvironmentEnhancementDebug)
            {
                Plugin.Logger.Log($"Registered key [{key}] to type [{type}]");
            }
        }

        internal static void InitTable()
        {
            string tableNamespace = "Chroma.LightIDTables.";
            Assembly assembly = Assembly.GetExecutingAssembly();
            IEnumerable<string> tableNames = assembly.GetManifestResourceNames().Where(n => n.StartsWith(tableNamespace));
            foreach (string tableName in tableNames)
            {
                using (JsonReader reader = new JsonTextReader(new StreamReader(assembly.GetManifestResourceStream(tableName))))
                {
                    Dictionary<int, int>[] typeTable = new Dictionary<int, int>[8];

                    JsonSerializer serializer = new JsonSerializer();
                    Dictionary<string, Dictionary<string, int>> rawDict = serializer.Deserialize<Dictionary<string, Dictionary<string, int>>>(reader);

                    foreach (KeyValuePair<string, Dictionary<string, int>> typePair in rawDict)
                    {
                        typeTable[int.Parse(typePair.Key)] = typePair.Value.ToDictionary(n => int.Parse(n.Key), n => n.Value);
                    }

                    string tableNameWithoutExtension = Path.GetFileNameWithoutExtension(tableName.Remove(tableName.IndexOf(tableNamespace), tableNamespace.Length));
                    _lightIDTable.Add(tableNameWithoutExtension, typeTable);
                }
            }
        }
    }
}
