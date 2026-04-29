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

        // 1x2 bed: bottom tile = mattress (sheets + base), top tile = pillow + headboard.
        public static Sprite BedBase(Color sheets)
        {
            var tex = NewTex();
            var dark = Darker(sheets, 0.45f);
            var light = Lighter(sheets, 0.10f);
            for (int y = 0; y < Size; y++)
                for (int x = 0; x < Size; x++)
                {
                    Color c = sheets;
                    if (y < 4) c = dark;                              // wood foot-board
                    else if (y == 4) c = Lighter(dark, 0.2f);
                    else if ((x % 6) == 0 && y > 4 && y < Size - 2) c = light; // sheet creases
                    if (x == 0 || x == Size - 1) c = dark;            // bed-frame sides
                    tex.SetPixel(x, y, c);
                }
            tex.Apply();
            return MakeSprite(tex);
        }

        public static Sprite BedHead(Color sheets, Color pillow)
        {
            var tex = NewTex();
            var dark = Darker(sheets, 0.45f);
            for (int y = 0; y < Size; y++)
                for (int x = 0; x < Size; x++)
                {
                    Color c = sheets;
                    if (y > Size - 8) c = dark;                        // wood headboard
                    if (x == 0 || x == Size - 1) c = dark;             // bed-frame sides
                    tex.SetPixel(x, y, c);
                }
            // Pillow oval near the top
            for (int y = 4; y < 16; y++)
                for (int x = 5; x < Size - 5; x++)
                {
                    float dx = (x - Size / 2f) / (Size / 2f - 5f);
                    float dy = (y - 10f) / 6f;
                    if (dx * dx + dy * dy < 1f)
                    {
                        Color p = (dx * dx + dy * dy > 0.7f) ? Darker(pillow, 0.20f) : pillow;
                        tex.SetPixel(x, y, p);
                    }
                }
            tex.Apply();
            return MakeSprite(tex);
        }

        // Top-down round plate with a small mound of food in the middle.
        // Background is transparent so it overlays the table tile beneath.
        public static Sprite Plate(Color rim, Color food)
        {
            var tex = NewTex();
            for (int i = 0; i < Size * Size; i++) tex.SetPixel(i % Size, i / Size, new Color(0, 0, 0, 0));
            float cx = Size / 2f - 0.5f, cy = Size / 2f - 0.5f;
            var rimDark = Darker(rim, 0.35f);
            var rimLight = Lighter(rim, 0.10f);
            var foodDark = Darker(food, 0.25f);
            for (int y = 0; y < Size; y++)
                for (int x = 0; x < Size; x++)
                {
                    float dx = x - cx, dy = y - cy;
                    float r = Mathf.Sqrt(dx * dx + dy * dy);
                    Color c;
                    if (r > 13.5f)      continue;                    // transparent outside
                    else if (r > 11.5f) c = rimDark;                 // outer ring
                    else if (r > 9f)    c = rim;                     // plate body
                    else if (r > 7f)    c = rimLight;                // inner highlight
                    else if (r > 4.5f)  c = food;                    // food mound
                    else                c = foodDark;                // mound shadow
                    tex.SetPixel(x, y, c);
                }
            tex.Apply();
            return MakeSprite(tex);
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

        // Solid floor tile with a soft inner pattern — used for paths and interiors.
        public static Sprite Floor(Color color)
        {
            var tex = new Texture2D(Size, Size, TextureFormat.RGBA32, false)
            { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
            var dark = Darker(color, 0.10f);
            var light = Lighter(color, 0.06f);
            for (int y = 0; y < Size; y++)
                for (int x = 0; x < Size; x++)
                {
                    var c = ((x * 7 + y * 13) & 7) == 0 ? light :
                            ((x + y) & 11) == 0 ? dark : color;
                    tex.SetPixel(x, y, c);
                }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, Size, Size), new Vector2(0.5f, 0.5f), Size);
        }

        // Sloped roof tile — top half lighter, bottom half darker, faint shingles.
        public static Sprite Roof(Color color)
        {
            var tex = new Texture2D(Size, Size, TextureFormat.RGBA32, false)
            { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
            var dark = Darker(color, 0.30f);
            var light = Lighter(color, 0.10f);
            for (int y = 0; y < Size; y++)
                for (int x = 0; x < Size; x++)
                {
                    var c = y > Size * 0.55f ? light : color;
                    if ((y % 6) == 0) c = dark;        // shingle line
                    if (x == 0 || y == 0 || x == Size - 1 || y == Size - 1) c = dark;
                    tex.SetPixel(x, y, c);
                }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, Size, Size), new Vector2(0.5f, 0.5f), Size);
        }

        // Wooden door with a brass handle dot.
        public static Sprite Door(Color color)
        {
            var tex = new Texture2D(Size, Size, TextureFormat.RGBA32, false)
            { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
            var dark = Darker(color, 0.45f);
            var brass = new Color(0.85f, 0.70f, 0.30f);
            for (int y = 0; y < Size; y++)
                for (int x = 0; x < Size; x++)
                {
                    bool border = x < 3 || x > Size - 4 || y < 3 || y > Size - 4;
                    bool plank = (x % 8) == 0;
                    var c = border ? dark : (plank ? Darker(color, 0.20f) : color);
                    tex.SetPixel(x, y, c);
                }
            // Arch top
            for (int x = 5; x < Size - 5; x++)
            {
                int yTop = (Size - 4) - Mathf.Abs(x - Size / 2) / 4;
                if (yTop < Size) tex.SetPixel(x, yTop, dark);
            }
            // Brass handle
            tex.SetPixel(Size - 9, Size / 2, brass);
            tex.SetPixel(Size - 9, Size / 2 - 1, brass);
            tex.SetPixel(Size - 10, Size / 2, brass);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, Size, Size), new Vector2(0.5f, 0.5f), Size);
        }

        // Small decor item: a potted plant / cairn / bench.  Picks shape from `kind`.
        public static Sprite Decor(Color color, int kind)
        {
            var tex = new Texture2D(Size, Size, TextureFormat.RGBA32, false)
            { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
            for (int i = 0; i < Size * Size; i++) tex.SetPixel(i % Size, i / Size, new Color(0, 0, 0, 0));
            var dark = Darker(color, 0.45f);
            switch (kind % 3)
            {
                case 0: // potted plant
                    var pot = new Color(0.55f, 0.30f, 0.18f);
                    for (int y = 4; y < 14; y++)
                        for (int x = 10; x < 22; x++)
                            tex.SetPixel(x, y, y < 6 ? Darker(pot, 0.4f) : pot);
                    for (int y = 14; y < 26; y++)
                        for (int x = 8; x < 24; x++)
                        {
                            float dx = x - 16, dy = y - 20;
                            if (dx * dx + dy * dy < 50) tex.SetPixel(x, y, color);
                        }
                    break;
                case 1: // cairn (3 stones)
                    for (int y = 4; y < 12; y++)
                        for (int x = 8; x < 24; x++)
                            tex.SetPixel(x, y, color);
                    for (int y = 12; y < 19; y++)
                        for (int x = 10; x < 22; x++)
                            tex.SetPixel(x, y, Darker(color, 0.10f));
                    for (int y = 19; y < 25; y++)
                        for (int x = 13; x < 19; x++)
                            tex.SetPixel(x, y, Lighter(color, 0.10f));
                    break;
                case 2: // bench
                    for (int y = 8; y < 12; y++)
                        for (int x = 4; x < 28; x++) tex.SetPixel(x, y, color);
                    for (int x = 6; x < 9; x++) for (int y = 4; y < 8; y++) tex.SetPixel(x, y, dark);
                    for (int x = 23; x < 26; x++) for (int y = 4; y < 8; y++) tex.SetPixel(x, y, dark);
                    for (int y = 12; y < 18; y++)
                        for (int x = 4; x < 28; x++) if (((x + y) & 3) == 0) tex.SetPixel(x, y, dark);
                    break;
            }
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

        // ─── Building parts ──────────────────────────────────────────────────
        // Each tile is rendered as a 32x32 sprite. Buildings are composed of
        // these in a multi-row layout (foundation → facade → eaves → roof → ridge).

        // Vertical wood-plank wall (used for residential / shop side walls).
        public static Sprite WallPlank(Color color)
        {
            var tex = NewTex();
            var seam = Darker(color, 0.45f);
            var grain = Darker(color, 0.18f);
            for (int y = 0; y < Size; y++)
                for (int x = 0; x < Size; x++)
                {
                    Color c = color;
                    if (x % 8 == 0) c = seam;                         // plank seam
                    if (((Hash(x, y * 3) * 31) % 7) < 1) c = grain;  // grain noise
                    if (y == 0 || y == Size - 1) c = seam;            // top/bottom edge
                    tex.SetPixel(x, y, c);
                }
            tex.Apply();
            return MakeSprite(tex);
        }

        // Plaster / clay-render wall (used for temple, shrine).
        public static Sprite WallPlaster(Color color)
        {
            var tex = NewTex();
            var stain = Darker(color, 0.10f);
            var hairline = Darker(color, 0.28f);
            var trim = Darker(color, 0.40f);
            for (int y = 0; y < Size; y++)
                for (int x = 0; x < Size; x++)
                {
                    float n = Hash(x, y);
                    Color c = color;
                    if (n < 0.20f) c = stain;
                    // a few hairline cracks
                    if ((x == 7 && y > 6 && y < 22 && (y % 3) != 0) ||
                        (x == 24 && y > 12 && y < 28 && (y % 4) != 0)) c = hairline;
                    if (y == 0 || y == Size - 1) c = trim;
                    tex.SetPixel(x, y, c);
                }
            tex.Apply();
            return MakeSprite(tex);
        }

        // Stone foundation course — heavy mortared stones at the base of walls.
        public static Sprite Foundation(Color color)
        {
            var tex = NewTex();
            var dark = Darker(color, 0.45f);
            var light = Lighter(color, 0.10f);
            // Three rows of staggered stones
            for (int y = 0; y < Size; y++)
                for (int x = 0; x < Size; x++)
                {
                    int row = y / 11;                 // 0..2
                    int offset = (row & 1) * 6;
                    bool mortar = ((x + offset) % 12) == 0 || (y % 11) == 0;
                    Color baseC = ((x + offset) / 12 % 2 == 0) ? color : Darker(color, 0.06f);
                    Color c = mortar ? dark : baseC;
                    if ((Hash(x, y * 5)) < 0.10f && !mortar) c = light;
                    tex.SetPixel(x, y, c);
                }
            tex.Apply();
            return MakeSprite(tex);
        }

        // Facade tile that includes a small window with warm interior glow.
        public static Sprite WallWithWindow(Color wall, Color frame, Color glow)
        {
            var tex = NewTex();
            var seam = Darker(wall, 0.45f);
            var grain = Darker(wall, 0.18f);
            for (int y = 0; y < Size; y++)
                for (int x = 0; x < Size; x++)
                {
                    Color c = wall;
                    if (x % 8 == 0) c = seam;
                    if (((Hash(x, y * 3) * 31) % 7) < 1) c = grain;
                    if (y == 0 || y == Size - 1) c = seam;
                    tex.SetPixel(x, y, c);
                }
            // Window aperture
            const int wx0 = 9, wx1 = 23, wy0 = 11, wy1 = 24;
            // frame
            for (int x = wx0 - 1; x <= wx1 + 1; x++)
            { tex.SetPixel(x, wy0 - 1, frame); tex.SetPixel(x, wy1 + 1, frame); }
            for (int y = wy0 - 1; y <= wy1 + 1; y++)
            { tex.SetPixel(wx0 - 1, y, frame); tex.SetPixel(wx1 + 1, y, frame); }
            // glass + glow
            for (int y = wy0; y <= wy1; y++)
                for (int x = wx0; x <= wx1; x++)
                {
                    float dx = x - 16, dy = y - 17;
                    float r = Mathf.Sqrt(dx * dx + dy * dy);
                    Color g = Color.Lerp(glow, Darker(glow, 0.55f), Mathf.Clamp01(r / 9f));
                    tex.SetPixel(x, y, g);
                }
            // mullions
            for (int y = wy0; y <= wy1; y++) tex.SetPixel(16, y, frame);
            for (int x = wx0; x <= wx1; x++) tex.SetPixel(x, 17, frame);
            tex.Apply();
            return MakeSprite(tex);
        }

        // Eave row — visual band that transitions wall to roof. Casts a darker
        // strip at the bottom (under-eave shadow) and roof color above.
        public static Sprite Eave(Color roof, Color underShadow)
        {
            var tex = NewTex();
            var dark = Darker(roof, 0.30f);
            for (int y = 0; y < Size; y++)
                for (int x = 0; x < Size; x++)
                {
                    Color c = y < 6 ? underShadow                       // under-eave shadow
                            : (y < 10 ? Darker(roof, 0.10f)              // edge of roof
                            : ((y % 7) == 0 ? dark : roof));             // shingle line
                    if (y == Size - 1) c = dark;
                    tex.SetPixel(x, y, c);
                }
            tex.Apply();
            return MakeSprite(tex);
        }

        // Roof shingle tile (interior of building roof) — diagonal shingles.
        public static Sprite RoofShingle(Color color)
        {
            var tex = NewTex();
            var dark = Darker(color, 0.30f);
            var light = Lighter(color, 0.12f);
            for (int y = 0; y < Size; y++)
                for (int x = 0; x < Size; x++)
                {
                    bool seam = (y % 7) == 0;
                    bool stagger = (((y / 7) & 1) == 0);
                    bool tileSeam = ((x + (stagger ? 0 : 5)) % 10) == 0;
                    Color c = color;
                    if (tileSeam) c = Darker(color, 0.18f);
                    if (seam) c = dark;
                    if ((y % 7) == 1) c = light;          // tiny highlight under each seam
                    tex.SetPixel(x, y, c);
                }
            tex.Apply();
            return MakeSprite(tex);
        }

        // Roof ridge cap — top of the roof with two finial bumps.
        public static Sprite RoofRidge(Color color, Color finial)
        {
            var tex = NewTex();
            var dark = Darker(color, 0.45f);
            var light = Lighter(color, 0.10f);
            for (int y = 0; y < Size; y++)
                for (int x = 0; x < Size; x++)
                {
                    Color c = y < 18 ? color : (y < 24 ? light : dark);
                    if ((x % 4) == 0 && y < 18) c = Darker(color, 0.22f);
                    if (y == Size - 1 || y == 17 || y == 23) c = dark;
                    tex.SetPixel(x, y, c);
                }
            // Two finial bumps near the corners
            for (int dy = 0; dy < 5; dy++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    tex.SetPixel(6  + dx, 24 + dy, finial);
                    tex.SetPixel(25 + dx, 24 + dy, finial);
                }
            }
            tex.SetPixel(6,  29, Lighter(finial, 0.4f));
            tex.SetPixel(25, 29, Lighter(finial, 0.4f));
            tex.Apply();
            return MakeSprite(tex);
        }

        // Door frame (left or right of door tile) — wood post.
        public static Sprite DoorFrame(Color wood, bool isLeft)
        {
            var tex = NewTex();
            var dark = Darker(wood, 0.45f);
            var light = Lighter(wood, 0.15f);
            for (int y = 0; y < Size; y++)
                for (int x = 0; x < Size; x++)
                {
                    Color c = wood;
                    if (((Hash(x, y * 3) * 31) % 7) < 1) c = Darker(wood, 0.18f);
                    if (y == 0 || y == Size - 1) c = dark;
                    tex.SetPixel(x, y, c);
                }
            // Vertical post on the door side
            int postX = isLeft ? Size - 6 : 5;
            for (int y = 4; y < Size - 4; y++)
            {
                tex.SetPixel(postX,     y, dark);
                tex.SetPixel(postX + 1, y, light);
                tex.SetPixel(postX - 1, y, dark);
            }
            tex.Apply();
            return MakeSprite(tex);
        }

        // Hanging banner above shop door — colored strip with two ties.
        public static Sprite Banner(Color body, Color trim)
        {
            var tex = NewTex();
            for (int i = 0; i < Size * Size; i++) tex.SetPixel(i % Size, i / Size, new Color(0, 0, 0, 0));
            // Two ties at top corners
            for (int y = Size - 6; y < Size; y++)
            {
                tex.SetPixel(6,  y, trim);
                tex.SetPixel(25, y, trim);
            }
            // Body
            for (int y = 6; y < Size - 6; y++)
                for (int x = 4; x < Size - 4; x++)
                {
                    Color c = body;
                    if (y == 6 || y == Size - 7) c = trim;
                    if (x == 4 || x == Size - 5) c = trim;
                    if ((x + y) % 6 == 0) c = Darker(body, 0.15f);
                    tex.SetPixel(x, y, c);
                }
            // Pointed bottom
            for (int x = 4; x < Size - 4; x++)
            {
                int dy = Mathf.Abs(x - Size / 2);
                for (int y = 4; y < 6 + dy / 2; y++)
                {
                    if (x > 4 && x < Size - 5)
                        tex.SetPixel(x, y, new Color(0, 0, 0, 0));
                }
            }
            tex.Apply();
            return MakeSprite(tex);
        }

        // String of prayer flags — five colored squares with a string overhead.
        public static Sprite PrayerFlags(int seed)
        {
            var tex = NewTex();
            for (int i = 0; i < Size * Size; i++) tex.SetPixel(i % Size, i / Size, new Color(0, 0, 0, 0));
            // String
            for (int x = 0; x < Size; x++) tex.SetPixel(x, Size - 4, new Color(0.30f, 0.25f, 0.20f, 0.9f));
            // Five flags, alternating colors
            Color[] colors = {
                new Color(0.85f, 0.30f, 0.30f), // red
                new Color(0.95f, 0.85f, 0.35f), // yellow
                new Color(0.40f, 0.65f, 0.35f), // green
                new Color(0.30f, 0.55f, 0.85f), // blue
                new Color(0.95f, 0.92f, 0.85f), // white
            };
            for (int i = 0; i < 5; i++)
            {
                int x0 = 1 + i * 6;
                Color flag = colors[(i + seed) % 5];
                for (int y = Size - 14; y < Size - 5; y++)
                    for (int x = x0; x < x0 + 5; x++)
                    {
                        Color c = flag;
                        if (y == Size - 14 || y == Size - 6 || x == x0 || x == x0 + 4) c = Darker(flag, 0.3f);
                        tex.SetPixel(x, y, c);
                    }
            }
            tex.Apply();
            return MakeSprite(tex);
        }

        // Small step-stone tile placed in front of a door.
        public static Sprite Stoop(Color color)
        {
            var tex = NewTex();
            var dark = Darker(color, 0.40f);
            for (int y = 0; y < Size; y++)
                for (int x = 0; x < Size; x++)
                {
                    Color c = (y > Size / 2) ? Lighter(color, 0.05f) : color;
                    if (y == Size / 2) c = dark;
                    if (x == 0 || x == Size - 1 || y == 0 || y == Size - 1) c = dark;
                    if ((Hash(x * 2, y) < 0.08f)) c = Darker(color, 0.15f);
                    tex.SetPixel(x, y, c);
                }
            tex.Apply();
            return MakeSprite(tex);
        }

        // ─── Internal helpers ────────────────────────────────────────────────
        static Texture2D NewTex()
        {
            var t = new Texture2D(Size, Size, TextureFormat.RGBA32, false)
            { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
            return t;
        }
        static Sprite MakeSprite(Texture2D tex)
            => Sprite.Create(tex, new Rect(0, 0, Size, Size), new Vector2(0.5f, 0.5f), Size);

        // Cheap deterministic hash → [0..1). Used for grain / noise patterns.
        static float Hash(int x, int y)
        {
            unchecked
            {
                int n = x * 374761393 + y * 668265263;
                n = (n ^ (n >> 13)) * 1274126177;
                n = n ^ (n >> 16);
                return (n & 0xFFFFFF) / (float)0xFFFFFF;
            }
        }

        static Color Darker (Color c, float f) => new Color(c.r * (1f - f), c.g * (1f - f), c.b * (1f - f), c.a);
        static Color Lighter(Color c, float f) => new Color(Mathf.Lerp(c.r, 1f, f), Mathf.Lerp(c.g, 1f, f), Mathf.Lerp(c.b, 1f, f), c.a);
    }
}
#endif
