using System;
using System.Collections.Generic;
using System.IO;
using IPA.Logging;
using IPA.Utilities;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Chroma.EnvironmentEnhancement.Saved
{
    internal class Loader
    {
        private static readonly string _directory = Path.Combine(UnityGame.UserDataPath, ChromaController.ID, "Environments");
        private static readonly Version _currVer = new(1, 0, 0);

        [UsedImplicitly]
        private Loader()
        {
            List<SavedEnvironment?> environments = new() { null };
            Environments = environments;
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

                    Log.Logger.Log($"Loaded [{file}].", Logger.Level.Trace);
                    environments.Add(savedEnvironment);
                }
                catch (Exception e)
                {
                    Log.Logger.Log($"Encountered error deserializing [{file}].", Logger.Level.Error);
                    Log.Logger.Log(e, Logger.Level.Error);
                }
            }
        }

        public List<SavedEnvironment?> Environments { get; }
    }
}
