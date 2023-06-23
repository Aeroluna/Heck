using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using Chroma.Settings;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Chroma.EnvironmentEnhancement.Saved
{
    internal class SavedEnvironmentLoader
    {
        private static readonly string _directory = Path.Combine(Paths.ConfigPath, ChromaController.ID, "Environments");
        private static readonly Version _currVer = new(1, 0, 0);

        // TODO: change modules to use instanced
        private static SavedEnvironmentLoader? _instance;

        private readonly Config _config;

        [UsedImplicitly]
        private SavedEnvironmentLoader(Config config)
        {
            _instance = this;
            _config = config;
            Init();
        }

        public static SavedEnvironmentLoader Instance => _instance ?? throw new InvalidOperationException("SavedEnvironmentLoader instance not yet created.");

        public Dictionary<string?, SavedEnvironment?> Environments { get; private set; } = new();

        public SavedEnvironment? SavedEnvironment
        {
            get
            {
                string? name = _config.CustomEnvironment.Value;
                if (name == null)
                {
                    return null;
                }

                Environments.TryGetValue(name, out SavedEnvironment? result);
                return result;
            }
        }

        internal void Init()
        {
            Environments = new Dictionary<string?, SavedEnvironment?>();

            if (!Directory.Exists(_directory))
            {
                Directory.CreateDirectory(_directory);
            }

            JsonSerializerSettings serializerSettings = new()
            {
                MissingMemberHandling = MissingMemberHandling.Error
            };

            foreach (string file in Directory.EnumerateFiles(_directory, "*.dat"))
            {
                try
                {
                    using StreamReader streamReader = new(file);
                    using JsonReader reader = new JsonTextReader(streamReader);
                    JsonSerializer serializer = JsonSerializer.Create(serializerSettings);
                    SavedEnvironment savedEnvironment = serializer.Deserialize<SavedEnvironment>(reader)
                                                        ?? throw new InvalidOperationException("Deserializing returned null.");
                    if (savedEnvironment.Version != _currVer)
                    {
                        throw new InvalidOperationException($"Unhandled version: [{savedEnvironment.Version}], must be [{_currVer}].");
                    }

                    string fileName = Path.GetFileName(file);
                    Plugin.Log.LogDebug($"Loaded [{file}].");

                    Environments.Add(fileName, savedEnvironment);
                }
                catch (Exception e)
                {
                    Plugin.Log.LogError($"Encountered error deserializing [{file}].");
                    Plugin.Log.LogError(e);
                }
            }
        }
    }
}
