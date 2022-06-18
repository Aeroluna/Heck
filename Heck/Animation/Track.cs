using System;
using System.Collections.Generic;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using JetBrains.Annotations;
using UnityEngine;
using static Heck.HeckController;

namespace Heck.Animation
{
    public class TrackBuilder
    {
        private readonly bool _v2;

        public TrackBuilder(bool v2)
        {
            _v2 = v2;
        }

        public static event Action<Track>? TrackCreated;

        public Dictionary<string, Track> Tracks { get; } = new();

        public void AddTrack(string trackName)
        {
            if (Tracks.ContainsKey(trackName))
            {
                return;
            }

            Track track = new(_v2);
            TrackCreated?.Invoke(track);
            Tracks.Add(trackName, track);
        }

        public void AddFromCustomData(CustomData customData, bool v2, bool required = true)
        {
            AddFromCustomData(customData, v2 ? V2_TRACK : TRACK, required);
        }

        [AssertionMethod]
        public void AddFromCustomData(CustomData customData, string name, bool required = true)
        {
            string? trackName = customData.Get<string>(name);
            if (trackName != null)
            {
                AddTrack(trackName);
            }
            else if (required)
            {
                throw new JsonNotDefinedException(name);
            }
        }
    }

    public class Track
    {
        private readonly bool _v2;

        public Track(bool v2)
        {
            _v2 = v2;

            AddProperty(POSITION, PropertyType.Vector3, V2_POSITION);
            AddProperty(LOCAL_POSITION, PropertyType.Vector3, V2_LOCAL_POSITION);
            AddProperty(ROTATION, PropertyType.Quaternion, V2_ROTATION);
            AddProperty(LOCAL_ROTATION, PropertyType.Quaternion, V2_LOCAL_ROTATION);
            AddProperty(SCALE, PropertyType.Vector3, V2_SCALE);
        }

        public event Action<GameObject>? GameObjectAdded;

        public event Action<GameObject>? GameObjectRemoved;

        public HashSet<GameObject> GameObjects { get; } = new();

        internal IDictionary<string, Property> Properties { get; } = new Dictionary<string, Property>();

        internal IDictionary<string, Property> PathProperties { get; } = new Dictionary<string, Property>();

        internal IDictionary<string, List<Property>> PropertyAliases { get; } = new Dictionary<string, List<Property>>();

        internal IDictionary<string, List<Property>> PathPropertyAliases { get; } = new Dictionary<string, List<Property>>();

        public void AddGameObject(GameObject gameObject)
        {
            if (gameObject == null)
            {
                throw new ArgumentNullException(nameof(gameObject));
            }

            GameObjects.Add(gameObject);
            GameObjectAdded?.Invoke(gameObject);
        }

        public void RemoveGameObject(GameObject gameObject)
        {
            if (gameObject == null)
            {
                throw new ArgumentNullException(nameof(gameObject));
            }

            GameObjects.Remove(gameObject);
            GameObjectRemoved?.Invoke(gameObject);
        }

        public void AddProperty(string name, PropertyType propertyType, string? v2Alias = null)
        {
            Property property = new(propertyType);
            AddProperty(name, property, v2Alias, Properties, PropertyAliases);
        }

        public void AddPathProperty(string name, PropertyType propertyType, string? v2Alias = null)
        {
            PathProperty property = new(propertyType);
            AddProperty(name, property, v2Alias, PathProperties, PathPropertyAliases);
        }

        private void AddProperty(
            string name,
            Property property,
            string? v2Alias,
            IDictionary<string, Property> properties,
            IDictionary<string, List<Property>> aliases)
        {
            if (properties.ContainsKey(name))
            {
                return;
            }

            properties[name] = property;

            // handle v2 aliasing
            if (!_v2 || v2Alias == null)
            {
                return;
            }

            if (!aliases.TryGetValue(v2Alias, out List<Property> aliasedProperties))
            {
                aliasedProperties = new List<Property>();
                aliases[v2Alias] = aliasedProperties;
            }

            aliasedProperties.Add(property);
        }
    }
}
