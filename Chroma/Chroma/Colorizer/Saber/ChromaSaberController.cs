namespace Chroma.Colorizer
{
    using System.Collections.Generic;
    using UnityEngine;

    internal class ChromaSaberController : MonoBehaviour
    {
        private SaberType _saberType;
        private Saber _saber;

        internal void Init(Saber saber)
        {
            _saber = saber;
            _saberType = saber.saberType;
            new SaberColorizer(saber);
        }

        private void OnDestroy()
        {
            if (SaberColorizer.Colorizers.TryGetValue(_saberType, out List<SaberColorizer> colorizers))
            {
                int index = colorizers.FindIndex(n => n.Saber == _saber);
                colorizers.RemoveAt(index);
            }
        }
    }
}
