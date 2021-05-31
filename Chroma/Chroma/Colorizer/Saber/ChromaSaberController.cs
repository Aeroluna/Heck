namespace Chroma.Colorizer
{
    using UnityEngine;

    internal class ChromaSaberController : MonoBehaviour
    {
        private SaberType _saber;

        internal void Init(Saber saber)
        {
            _saber = saber.saberType;
            new SaberColorizer(saber);
        }

        private void OnDestroy()
        {
            SaberColorizer.Colorizers.Remove(_saber);
        }
    }
}
