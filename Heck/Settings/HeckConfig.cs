using System;
using IPA.Config.Stores.Attributes;
using IPA.Config.Stores.Converters;
using JetBrains.Annotations;
using UnityEngine;

// ReSharper disable MemberCanBeMadeStatic.Global
namespace Heck.Settings
{
    // TODO: zenjectify
    public class HeckConfig
    {
        private static HeckConfig? _instance;

        public static HeckConfig Instance
        {
            get => _instance ?? throw new InvalidOperationException("HeckConfigConfig instance not yet created.");
            set => _instance = value;
        }

        [UsedImplicitly]
        public ReLoaderSettings ReLoader { get; set; } = new();

        public class ReLoaderSettings
        {
            [UsedImplicitly]
            [UseConverter(typeof(EnumConverter<KeyCode>))]
            public KeyCode Reload { get; set; } = KeyCode.Space;

            [UsedImplicitly]
            [UseConverter(typeof(EnumConverter<KeyCode>))]
            public KeyCode SaveTime { get; set; } = KeyCode.LeftControl;

            [UsedImplicitly]
            [UseConverter(typeof(EnumConverter<KeyCode>))]
            public KeyCode JumpToSavedTime { get; set; } = KeyCode.Space;

            [UsedImplicitly]
            [UseConverter(typeof(EnumConverter<KeyCode>))]
            public KeyCode ScrubBackwards { get; set; } = KeyCode.LeftArrow;

            [UsedImplicitly]
            [UseConverter(typeof(EnumConverter<KeyCode>))]
            public KeyCode ScrubForwards { get; set; } = KeyCode.RightArrow;

            [UsedImplicitly]
            public float ScrubIncrement { get; set; } = 5;

            [UsedImplicitly]
            public bool ReloadOnRestart { get; set; }
        }
    }
}
