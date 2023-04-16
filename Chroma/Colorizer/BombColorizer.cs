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
        // we set "_Color" too becaause if any custom model replaces the bomb, it'll allow using "_Color"
        private static readonly int _simpleColor = Shader.PropertyToID("_SimpleColor");
        private static readonly int _color = Shader.PropertyToID("_Color");

        private readonly MaterialPropertyBlockController _materialPropertyBlockController;
        private readonly BombColorizerManager _manager;

        internal BombColorizer(NoteControllerBase noteController, BombColorizerManager manager)
        {
            _materialPropertyBlockController = noteController.GetComponent<MaterialPropertyBlockController>();
            OriginalColor = noteController.GetComponentInChildren<Renderer>().material.GetColor(_simpleColor);
            MaterialPropertyBlock materialPropertyBlock = _materialPropertyBlockController.materialPropertyBlock;
            materialPropertyBlock.SetColor(_simpleColor, OriginalColor);
            materialPropertyBlock.SetColor(_color, OriginalColor);

            _manager = manager;
        }

        protected override Color? GlobalColorGetter => _manager.GlobalColor;

        internal override void Refresh()
        {
            MaterialPropertyBlock bombMaterial = _materialPropertyBlockController.materialPropertyBlock;
            Color color = Color;
            if (color == bombMaterial.GetColor(_simpleColor))
            {
                return;
            }

            bombMaterial.SetColor(_simpleColor, color);
            bombMaterial.SetColor(_color, color);
            _materialPropertyBlockController.ApplyChanges();
        }

        [UsedImplicitly]
        internal class Factory : PlaceholderFactory<NoteControllerBase, BombColorizer>
        {
        }
    }
}
