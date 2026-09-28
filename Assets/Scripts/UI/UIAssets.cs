using System.Collections.Generic;
using ComputerExplorer.Core;
using UnityEngine;

namespace ComputerExplorer.UI
{
    /// <summary>
    /// Runtime-generated and cached UI resources: the rounded 9-slice sprite, fonts, icons and images.
    /// Loading icons as Texture2D and wrapping them makes the project independent of texture import settings.
    /// </summary>
    public static class UIAssets
    {
        private const int RoundSize = 128;
        public const float RoundRadius = RoundSize / 2f;

        private static Sprite rounded;
        private static Font regular, bold;
        private static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();

        /// <summary>
        /// A white circle texture sliced with 63 px borders: used Sliced it is a rounded rectangle whose radius is set
        /// via Image.pixelsPerUnitMultiplier; used Simple it is a circle.
        /// </summary>
        public static Sprite Rounded
        {
            get
            {
                if (rounded != null) return rounded;
                var tex = new Texture2D(RoundSize, RoundSize, TextureFormat.RGBA32, false)
                {
                    name = "UI_Rounded",
                    wrapMode = TextureWrapMode.Clamp,
                    filterMode = FilterMode.Bilinear
                };
                var px = new Color32[RoundSize * RoundSize];
                float c = RoundSize / 2f;
                for (int y = 0; y < RoundSize; y++)
                for (int x = 0; x < RoundSize; x++)
                {
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(c, c));
                    byte a = (byte)(Mathf.Clamp01(c - d + 0.5f) * 255);
                    px[y * RoundSize + x] = new Color32(255, 255, 255, a);
                }
                tex.SetPixels32(px);
                tex.Apply(false, true);
                float b = RoundRadius - 1;
                rounded = Sprite.Create(tex, new Rect(0, 0, RoundSize, RoundSize), new Vector2(0.5f, 0.5f), 100f, 0,
                    SpriteMeshType.FullRect, new Vector4(b, b, b, b));
                rounded.name = "UI_Rounded";
                return rounded;
            }
        }

        public static Font Regular => regular != null ? regular : regular = LoadFont(AppConstants.ResourcePaths.FontRegular);
        public static Font Bold => bold != null ? bold : bold = LoadFont(AppConstants.ResourcePaths.FontBold);

        private static Font LoadFont(string path)
        {
            var f = Resources.Load<Font>(path);
            if (f != null) return f;
            Debug.LogWarning($"[UI] Font '{path}' not found, using built-in font.");
            return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        public static Sprite Icon(string iconName) => Load(AppConstants.ResourcePaths.Icons + iconName);

        public static Sprite Image(string imageName) => Load(AppConstants.ResourcePaths.Images + imageName);

        private static Sprite Load(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            if (Cache.TryGetValue(path, out var s) && s != null) return s;
            s = Resources.Load<Sprite>(path);
            if (s == null)
            {
                var tex = Resources.Load<Texture2D>(path);
                if (tex != null)
                    s = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
            }
            if (s == null) Debug.LogWarning($"[UI] Missing sprite: Resources/{path}");
            Cache[path] = s;
            return s;
        }
    }
}
