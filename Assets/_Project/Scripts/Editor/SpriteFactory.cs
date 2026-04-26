#if UNITY_EDITOR
using UnityEngine;

namespace Bandhana.EditorTools
{
    // Procedural pixel-art helpers used by all scene builders. Slightly less
    // ugly than flat colored squares; real art replaces these in production.
    public static class SpriteFactory
    {
        const int Size = 32;

        public static Sprite Player(Color body)
        {
            var tex = new Texture2D(Size, Size, TextureFormat.RGBA32, false)
            { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
            for (int y = 0; y < Size; y++)
                for (int x = 0; x < Size; x++)
                {
                    bool border = x == 0 || y == 0 || x == Size - 1 || y == Size - 1;
                    var c = border ? Darker(body, 0.35f) : body;
                    tex.SetPixel(x, y, c);
                }
            // Two eye dots at the upper-front
            for (int dx = -1; dx <= 0; dx++) tex.SetPixel(11 + dx, 21, Color.black);
            for (int dx = -1; dx <= 0; dx++) tex.SetPixel(20 + dx, 21, Color.black);
            // Hair tuft
            for (int x = 8; x < 24; x++) tex.SetPixel(x, 27, Darker(body, 0.55f));
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, Size, Size), new Vector2(0.5f, 0.5f), Size);
        }

        public static Sprite NPC(Color body)
        {
            var tex = new Texture2D(Size, Size, TextureFormat.RGBA32, false)
            { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
            for (int y = 0; y < Size; y++)
                for (int x = 0; x < Size; x++)
                {
                    bool border = x == 0 || y == 0 || x == Size - 1 || y == Size - 1;
                    tex.SetPixel(x, y, border ? Darker(body, 0.35f) : body);
                }
            // Robe band
            var band = Darker(body, 0.45f);
            for (int x = 4; x < 28; x++) { tex.SetPixel(x, 12, band); tex.SetPixel(x, 13, band); }
            // Head circle (lighter)
            var head = Lighter(body, 0.25f);
            for (int y = 18; y < 26; y++)
                for (int x = 12; x < 20; x++)
                    tex.SetPixel(x, y, head);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, Size, Size), new Vector2(0.5f, 0.5f), Size);
        }

        public static Sprite Wall()
        {
            var tex = new Texture2D(Size, Size, TextureFormat.RGBA32, false)
            { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
            var baseCol = new Color(0.28f, 0.22f, 0.32f);
            var dark    = Darker(baseCol, 0.35f);
            var light   = Lighter(baseCol, 0.15f);
            for (int y = 0; y < Size; y++)
                for (int x = 0; x < Size; x++)
                {
                    var c = ((x ^ y) & 3) == 0 ? light : baseCol;
                    if (x == 0 || y == 0 || x == Size - 1 || y == Size - 1) c = dark;
                    tex.SetPixel(x, y, c);
                }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, Size, Size), new Vector2(0.5f, 0.5f), Size);
        }

        public static Sprite Haunt(Color glow)
        {
            var tex = new Texture2D(Size, Size, TextureFormat.RGBA32, false)
            { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
            var dark = Darker(glow, 0.55f);
            float cx = Size / 2f - 0.5f, cy = Size / 2f - 0.5f;
            float maxR = Size / 2f;
            for (int y = 0; y < Size; y++)
                for (int x = 0; x < Size; x++)
                {
                    float r = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                    float t = Mathf.Clamp01(r / maxR);
                    tex.SetPixel(x, y, Color.Lerp(glow, dark, t));
                }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, Size, Size), new Vector2(0.5f, 0.5f), Size);
        }

        public static Sprite Drum(Color body)
        {
            // Round drum motif for the Damaru pickup / spirit.
            var tex = new Texture2D(Size, Size, TextureFormat.RGBA32, false)
            { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
            var dark = Darker(body, 0.55f);
            float cx = Size / 2f - 0.5f, cy = Size / 2f - 0.5f;
            for (int y = 0; y < Size; y++)
                for (int x = 0; x < Size; x++)
                {
                    float r = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                    Color c = new Color(0, 0, 0, 0);
                    if (r < 14f) c = body;
                    if (r > 12f && r < 14f) c = dark;
                    if (Mathf.Abs(y - cy) < 1.2f && r < 14f) c = dark; // skin line
                    tex.SetPixel(x, y, c);
                }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, Size, Size), new Vector2(0.5f, 0.5f), Size);
        }

        public static Sprite Solid(Color color)
        {
            var tex = new Texture2D(Size, Size, TextureFormat.RGBA32, false)
            { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
            var pixels = new Color[Size * Size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, Size, Size), new Vector2(0.5f, 0.5f), Size);
        }

        public static Sprite Transition(Color color)
        {
            var tex = new Texture2D(Size, Size, TextureFormat.RGBA32, false)
            { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
            var dark = Darker(color, 0.45f);
            for (int y = 0; y < Size; y++)
                for (int x = 0; x < Size; x++)
                {
                    bool stripe = ((x + y) % 6) < 2;
                    tex.SetPixel(x, y, stripe ? dark : color);
                }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, Size, Size), new Vector2(0.5f, 0.5f), Size);
        }

        static Color Darker (Color c, float f) => new Color(c.r * (1f - f), c.g * (1f - f), c.b * (1f - f), c.a);
        static Color Lighter(Color c, float f) => new Color(Mathf.Lerp(c.r, 1f, f), Mathf.Lerp(c.g, 1f, f), Mathf.Lerp(c.b, 1f, f), c.a);
    }
}
#endif
