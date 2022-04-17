using System;
using System.Collections.Generic;
using UnityEngine;

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
    }

    public class Track
    {
        private readonly bool _v2;

        public Track(bool v2)
        {
            _v2 = v2;
        }

        public event Action<GameObject>? OnGameObjectAdded;

        public event Action<GameObject>? OnGameObjectRemoved;

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
            OnGameObjectAdded?.Invoke(gameObject);
        }

        public void RemoveGameObject(GameObject gameObject)
        {
            if (gameObject == null)
            {
                throw new ArgumentNullException(nameof(gameObject));
            }

            GameObjects.Remove(gameObject);
            OnGameObjectRemoved?.Invoke(gameObject);
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
