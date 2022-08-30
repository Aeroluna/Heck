using System;
using System.Collections.Generic;
using BeatmapSaveDataVersion3;
using CustomJSONData.CustomBeatmap;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Chroma.EnvironmentEnhancement.Saved
{
    public class SavedEnvironment
    {
        [JsonConstructor]
        private SavedEnvironment(
            string name,
            Version version,
            string author,
            Features features,
            Version environmentVersion,
            string environmentName,
            List<CustomData> environment,
            CustomData? materials)
        {
            Name = name;
            Version = version;
            Author = author;
            Features = features;
            EnvironmentVersion = environmentVersion;
            EnvironmentName = environmentName;
            Environment = environment;
            Materials = materials;
        }

        [JsonProperty("version", Required = Required.Always)]
        [JsonConverter(typeof(VersionConverter))]
        internal Version Version { get; }

        [JsonProperty("name", Required = Required.Always)]
        internal string Name { get; }

        [JsonProperty("author", Required = Required.Always)]
        internal string Author { get; }

        [JsonProperty("features", Required = Required.Always)]
        internal Features Features { get; }

        [JsonProperty("environmentVersion", Required = Required.Always)]
        [JsonConverter(typeof(VersionConverter))]
        internal Version EnvironmentVersion { get; }

        [JsonProperty("environmentName", Required = Required.Always)]
        internal string EnvironmentName { get; }

        [JsonProperty("environment", Required = Required.Always)]
        internal List<CustomData> Environment { get; }

        [JsonProperty("materials", Required = Required.Default)]
        internal CustomData? Materials { get; }
    }
}
