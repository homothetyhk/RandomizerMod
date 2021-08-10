using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


namespace RandomizerMod
{
    internal static class SpriteManager
    {
        private static Dictionary<string, Sprite> _sprites;

        public static void LoadEmbeddedPngs(string prefix)
        {
            Assembly a = typeof(SpriteManager).Assembly;
            _sprites = new Dictionary<string, Sprite>();
            foreach (string name in a.GetManifestResourceNames().Where(name => name.Substring(name.Length - 3).ToLower() == "png"))
            {
                string altName = prefix != null ? name.Substring(prefix.Length) : name;
                altName = altName.Remove(altName.Length - 4);
                Sprite sprite = FromStream(a.GetManifestResourceStream(name));
                _sprites[altName] = sprite;
            }
        }

        public static Sprite GetSprite(string name)
        {
            if (_sprites != null && _sprites.TryGetValue(name, out Sprite sprite)) return sprite;
            else return FromStream(typeof(SpriteManager).Assembly.GetManifestResourceStream(name));
        }

        private static Sprite FromStream(Stream s)
        {
            Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            byte[] buffer = ToArray(s);
            tex.LoadImage(buffer, markNonReadable: true);
            tex.filterMode = FilterMode.Point;
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }

        private static byte[] ToArray(Stream s)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                s.CopyTo(ms);
                return ms.ToArray();
            }
        }
    }
}
