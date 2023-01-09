using System;
using System.Collections.Generic;
using System.IO;
using IPA.Logging;
using IPA.Utilities;
using Newtonsoft.Json;

namespace Chroma.EnvironmentEnhancement.Saved
{
    // TODO: make config not static
    internal static class SavedEnvironmentLoader
    {
        private static readonly string _directory = Path.Combine(UnityGame.UserDataPath, ChromaController.ID, "Environments");
        private static readonly Version _currVer = new(1, 0, 0);
        private static readonly List<SavedEnvironment?> _environments = new() { null };

        public static IEnumerable<SavedEnvironment?> Environments => _environments;

        internal static void Init()
        {
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

                    savedEnvironment.FileName = Path.GetFileName(file);
                    Log.Logger.Log($"Loaded [{file}].", Logger.Level.Trace);

                    _environments.Add(savedEnvironment);
                }
                catch (Exception e)
                {
                    Log.Logger.Log($"Encountered error deserializing [{file}].", Logger.Level.Error);
                    Log.Logger.Log(e, Logger.Level.Error);
                }
            }
        }
    }
}
