using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Chroma.Utils {

    public class Base64Sprites {

        public static string SpriteToBase64(Sprite input) {
            return Convert.ToBase64String(input.texture.EncodeToPNG());
        }

        public static Sprite Base64ToSprite(string input) {
            string base64 = input;
            if (input.Contains(",")) {
                base64 = input.Substring(input.IndexOf(','));
            }
            Texture2D tex = Base64ToTexture2D(base64);
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), (Vector2.one / 2f));
        }

        public static Texture2D Base64ToTexture2D(string encodedData) {
            byte[] imageData = Convert.FromBase64String(encodedData);

            Texture2D texture = new Texture2D(0, 0, TextureFormat.ARGB32, false, true);
            texture.hideFlags = HideFlags.HideAndDontSave;
            texture.filterMode = FilterMode.Trilinear;
            if (!ImageConversion.LoadImage(texture, imageData)) {
                ChromaLogger.Log(new Exception("Failed to load image from Base64String ["+encodedData+"]"));
            }
            return texture;
        }

    }

}
