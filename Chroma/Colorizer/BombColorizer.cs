using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;

namespace Chroma.Colorizer
{
    [UsedImplicitly]
    public class BombColorizerManager
    {
        private readonly BombColorizer.Factory _factory;

        internal BombColorizerManager(BombColorizer.Factory factory)
        {
            _factory = factory;
        }

        public Dictionary<NoteControllerBase, BombColorizer> Colorizers { get; } = new();

        public Color? GlobalColor { get; private set; }

        public BombColorizer GetColorizer(NoteControllerBase noteController) => Colorizers[noteController];

        public void Colorize(NoteControllerBase noteController, Color? color) => GetColorizer(noteController).Colorize(color);

        [PublicAPI]
        public void GlobalColorize(Color? color)
        {
            GlobalColor = color;
            foreach (KeyValuePair<NoteControllerBase, BombColorizer> valuePair in Colorizers)
            {
                valuePair.Value.Refresh();
            }
        }

        internal void Create(NoteControllerBase noteController)
        {
            Colorizers.Add(noteController, _factory.Create(noteController));
        }
    }

    [UsedImplicitly]
    public class BombColorizer : ObjectColorizer
    {
        private static readonly int _simpleColor = Shader.PropertyToID("_SimpleColor");
        private readonly Renderer _bombRenderer;
        private readonly BombColorizerManager _manager;

        internal BombColorizer(NoteControllerBase noteController, BombColorizerManager manager)
        {
            _bombRenderer = noteController.gameObject.GetComponentInChildren<Renderer>();
            OriginalColor = _bombRenderer.material.GetColor(_simpleColor);

            _manager = manager;
        }

        protected override Color? GlobalColorGetter => _manager.GlobalColor;

        internal override void Refresh()
        {
            if (!_bombRenderer.enabled)
            {
                return;
            }

            Material bombMaterial = _bombRenderer.material;
            Color color = Color;
            if (color == bombMaterial.GetColor(_simpleColor))
            {
                return;
            }

            bombMaterial.SetColor(_simpleColor, color);
        }

        [UsedImplicitly]
        internal class Factory : PlaceholderFactory<NoteControllerBase, BombColorizer>
        {
        }
    }
}
