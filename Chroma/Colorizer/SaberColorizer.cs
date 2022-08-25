using System.Collections.Generic;
using JetBrains.Annotations;
using SiraUtil.Sabers;
using UnityEngine;
using Zenject;

namespace Chroma.Colorizer
{
    [UsedImplicitly]
    public class SaberColorizerManager
    {
        private readonly SaberColorizer.Factory _factory;

        internal SaberColorizerManager(SaberColorizer.Factory factory)
        {
            _factory = factory;
        }

        public Dictionary<SaberType, List<SaberColorizer>> Colorizers { get; } = new();

        public Color?[] GlobalColor { get; } = new Color?[2];

        public List<SaberColorizer> GetColorizers(SaberType saber) => Colorizers[saber];

        public void Colorize(SaberType saber, Color? color)
        {
            color = color?.ColorWithAlpha(1);
            GetColorizers(saber).ForEach(n => n.Colorize(color));
        }

        [PublicAPI]
        public void GlobalColorize(Color? color, SaberType saberType)
        {
            GlobalColor[(int)saberType] = color?.ColorWithAlpha(1);
            GetColorizers(saberType).ForEach(n => n.Refresh());
        }

        internal void Create(Saber saber)
        {
            if (!Colorizers.TryGetValue(saber.saberType, out List<SaberColorizer> colorizers))
            {
                colorizers = new List<SaberColorizer>();
                Colorizers.Add(saber.saberType, colorizers);
            }

            colorizers.Add(_factory.Create(saber));
        }
    }

    [UsedImplicitly]
    public class SaberColorizer : ObjectColorizer
    {
        private readonly Saber _saber;
        private readonly SaberColorizerManager _manager;
        private readonly SaberModelManager _modelManager;
        private readonly SaberType _saberType;
        private Color _lastColor;

        private SaberColorizer(Saber saber, ColorManager colorManager, SaberColorizerManager manager, SaberModelManager modelManager)
        {
            _saber = saber;
            _manager = manager;
            _modelManager = modelManager;
            _saberType = saber.saberType;
            _lastColor = colorManager.ColorForSaberType(_saberType);
            OriginalColor = _lastColor;
        }

        protected override Color? GlobalColorGetter => _manager.GlobalColor[(int)_saberType];

        internal override void Refresh()
        {
            Color color = Color;
            if (color == _lastColor)
            {
                return;
            }

            _lastColor = color;

            // not sure why sirautil does default saber coloring or if its even faster than my implementation, but it works
            _modelManager.SetColor(_saber, color);
        }

        [UsedImplicitly]
        internal class Factory : PlaceholderFactory<Saber, SaberColorizer>
        {
        }
    }
}
