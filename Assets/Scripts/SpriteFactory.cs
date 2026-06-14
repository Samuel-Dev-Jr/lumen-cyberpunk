using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Fábrica de sprites PROCEDURAIS (arte original gerada por código).
/// Em vez de depender de imagens baixadas, desenhamos cada sprite pixel a pixel
/// em uma Texture2D e a convertimos em Sprite. Isso garante que o jogo rode em
/// qualquer máquina sem depender de assets externos, e conta como arte original.
///
/// Convenção: cada sprite tem 16x16 pixels e usa PPU (Pixels Per Unit) = 16,
/// então 1 tile = 1 unidade de mundo. O ponto de pivô é o centro (0.5, 0.5).
/// </summary>
public static class SpriteFactory
{
    public const int PPU = 16;
    const int S = 16; // tamanho padrão do tile/sprite

    // ----- Paleta de cores do jogo (tema "caverna de luz") -----
    static readonly Color32 CLEAR = new Color32(0, 0, 0, 0);

    // Personagem "Lumen" (centelha âmbar que brilha)
    static readonly Color32 P_BODY = new Color32(255, 196, 75, 255);
    static readonly Color32 P_HI = new Color32(255, 233, 168, 255);
    static readonly Color32 P_DK = new Color32(58, 42, 18, 255);
    static readonly Color32 P_GLOW = new Color32(255, 170, 40, 90);

    // Chão / plataforma
    static readonly Color32 T_BODY = new Color32(70, 80, 110, 255);
    static readonly Color32 T_DARK = new Color32(44, 53, 80, 255);
    static readonly Color32 T_LIGHT = new Color32(95, 108, 150, 255);
    static readonly Color32 T_GRASS = new Color32(107, 203, 119, 255);
    static readonly Color32 T_GRASS_D = new Color32(74, 158, 90, 255);

    // Cristal / coletável
    static readonly Color32 C_BODY = new Color32(255, 210, 74, 255);
    static readonly Color32 C_HI = new Color32(255, 244, 190, 255);
    static readonly Color32 C_EDGE = new Color32(201, 150, 42, 255);

    // Inimigo (criatura das sombras)
    static readonly Color32 E_BODY = new Color32(106, 63, 160, 255);
    static readonly Color32 E_DARK = new Color32(64, 34, 105, 255);
    static readonly Color32 E_EYE = new Color32(255, 85, 85, 255);

    // Espinhos / perigo
    static readonly Color32 H_BODY = new Color32(200, 205, 225, 255);
    static readonly Color32 H_DARK = new Color32(120, 126, 150, 255);

    // Portal de saída (a "luz")
    static readonly Color32 X_GLOW = new Color32(130, 230, 255, 255);
    static readonly Color32 X_CORE = new Color32(245, 255, 255, 255);

    // Coração (HUD de vidas)
    static readonly Color32 HEART = new Color32(232, 70, 90, 255);
    static readonly Color32 HEART_HI = new Color32(255, 130, 145, 255);

    // ----- Cache: geramos cada sprite só uma vez -----
    static readonly Dictionary<string, Sprite> _cache = new Dictionary<string, Sprite>();
    static readonly Dictionary<string, Sprite[]> _cacheAnim = new Dictionary<string, Sprite[]>();

    // =========================================================
    //  Utilidades de desenho
    // =========================================================
    static Color32[] NewBuf(int w, int h)
    {
        var buf = new Color32[w * h];
        for (int i = 0; i < buf.Length; i++) buf[i] = CLEAR;
        return buf;
    }

    static void Px(Color32[] buf, int w, int h, int x, int y, Color32 c)
    {
        if (x < 0 || y < 0 || x >= w || y >= h) return;
        buf[y * w + x] = c;
    }

    static void FillRect(Color32[] buf, int w, int h, int x0, int y0, int rw, int rh, Color32 c)
    {
        for (int y = y0; y < y0 + rh; y++)
            for (int x = x0; x < x0 + rw; x++)
                Px(buf, w, h, x, y, c);
    }

    static void Disc(Color32[] buf, int w, int h, float cx, float cy, float r, Color32 c)
    {
        int x0 = Mathf.FloorToInt(cx - r), x1 = Mathf.CeilToInt(cx + r);
        int y0 = Mathf.FloorToInt(cy - r), y1 = Mathf.CeilToInt(cy + r);
        for (int y = y0; y <= y1; y++)
            for (int x = x0; x <= x1; x++)
            {
                float dx = x + 0.5f - cx, dy = y + 0.5f - cy;
                if (dx * dx + dy * dy <= r * r) Px(buf, w, h, x, y, c);
            }
    }

    static Sprite Make(Color32[] buf, int w, int h)
    {
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.SetPixels32(buf);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), PPU);
    }

    // =========================================================
    //  PERSONAGEM (Lumen)
    // =========================================================
    static Sprite PlayerFrame(int feetMode, int bodyYOffset, bool armsUp)
    {
        var b = NewBuf(S, S);
        float cy = 8.5f + bodyYOffset;
        // brilho difuso
        Disc(b, S, S, 8f, cy, 6.2f, P_GLOW);
        // corpo
        Disc(b, S, S, 8f, cy, 5f, P_BODY);
        // realce superior-esquerdo
        Disc(b, S, S, 6.4f, cy + 1.6f, 2.2f, P_HI);
        // olhos
        FillRect(b, S, S, 6, (int)cy, 1, 2, P_DK);
        FillRect(b, S, S, 10, (int)cy, 1, 2, P_DK);

        if (armsUp)
        {
            // braços levantados (escalada)
            FillRect(b, S, S, 3, (int)cy + 3, 1, 2, P_BODY);
            FillRect(b, S, S, 12, (int)cy + 3, 1, 2, P_BODY);
        }

        // pés (feetMode: 0 parado, 1 esquerdo à frente, 2 direito à frente, 3 tucked/pulo)
        int footY = 2 + bodyYOffset;
        if (footY < 0) footY = 0;
        switch (feetMode)
        {
            case 0:
                FillRect(b, S, S, 5, 1, 2, 2, P_DK);
                FillRect(b, S, S, 9, 1, 2, 2, P_DK);
                break;
            case 1:
                FillRect(b, S, S, 4, 1, 2, 2, P_DK);
                FillRect(b, S, S, 9, 2, 2, 2, P_DK);
                break;
            case 2:
                FillRect(b, S, S, 5, 2, 2, 2, P_DK);
                FillRect(b, S, S, 10, 1, 2, 2, P_DK);
                break;
            case 3: // pulo: pés recolhidos
                FillRect(b, S, S, 5, 3, 2, 2, P_DK);
                FillRect(b, S, S, 9, 3, 2, 2, P_DK);
                break;
        }
        return Make(b, S, S);
    }

    public static Sprite[] PlayerIdle()
    {
        if (!_cacheAnim.TryGetValue("p_idle", out var a))
        {
            a = new[] { PlayerFrame(0, 0, false), PlayerFrame(0, 1, false) };
            _cacheAnim["p_idle"] = a;
        }
        return a;
    }

    public static Sprite[] PlayerWalk()
    {
        if (!_cacheAnim.TryGetValue("p_walk", out var a))
        {
            a = new[] { PlayerFrame(1, 0, false), PlayerFrame(0, 1, false), PlayerFrame(2, 0, false), PlayerFrame(0, 1, false) };
            _cacheAnim["p_walk"] = a;
        }
        return a;
    }

    public static Sprite[] PlayerJump()
    {
        if (!_cacheAnim.TryGetValue("p_jump", out var a))
        {
            a = new[] { PlayerFrame(3, 1, false) };
            _cacheAnim["p_jump"] = a;
        }
        return a;
    }

    public static Sprite[] PlayerFall()
    {
        if (!_cacheAnim.TryGetValue("p_fall", out var a))
        {
            a = new[] { PlayerFrame(3, 0, false) };
            _cacheAnim["p_fall"] = a;
        }
        return a;
    }

    public static Sprite[] PlayerClimb()
    {
        if (!_cacheAnim.TryGetValue("p_climb", out var a))
        {
            a = new[] { PlayerFrame(1, 0, true), PlayerFrame(2, 0, true) };
            _cacheAnim["p_climb"] = a;
        }
        return a;
    }

    // =========================================================
    //  TILES (chão e plataforma)
    // =========================================================
    public static Sprite Tile(bool grassTop)
    {
        string key = grassTop ? "tile_grass" : "tile_solid";
        if (_cache.TryGetValue(key, out var s)) return s;

        var b = NewBuf(S, S);
        FillRect(b, S, S, 0, 0, S, S, T_BODY);
        // bisel: borda direita/baixo escura, esquerda/cima clara
        FillRect(b, S, S, 0, 0, S, 1, T_DARK);
        FillRect(b, S, S, S - 1, 0, 1, S, T_DARK);
        FillRect(b, S, S, 0, 0, 1, S, T_LIGHT);
        // textura interna (alguns pontos)
        Px(b, S, S, 4, 5, T_DARK); Px(b, S, S, 11, 8, T_DARK);
        Px(b, S, S, 7, 11, T_LIGHT); Px(b, S, S, 9, 4, T_DARK);

        if (grassTop)
        {
            FillRect(b, S, S, 0, S - 4, S, 4, T_GRASS);
            FillRect(b, S, S, 0, S - 4, S, 1, T_GRASS_D);
            // pequenas pontas de grama no topo
            Px(b, S, S, 2, S - 1, T_GRASS); Px(b, S, S, 6, S - 1, T_GRASS_D);
            Px(b, S, S, 10, S - 1, T_GRASS); Px(b, S, S, 13, S - 1, T_GRASS_D);
        }
        else
        {
            FillRect(b, S, S, 0, S - 1, S, 1, T_LIGHT);
        }
        s = Make(b, S, S);
        _cache[key] = s;
        return s;
    }

    // Escada / cipó
    public static Sprite Ladder()
    {
        if (_cache.TryGetValue("ladder", out var s)) return s;
        var b = NewBuf(S, S);
        Color32 rail = new Color32(150, 110, 70, 255);
        Color32 rung = new Color32(190, 150, 95, 255);
        FillRect(b, S, S, 3, 0, 2, S, rail);
        FillRect(b, S, S, 11, 0, 2, S, rail);
        for (int y = 1; y < S; y += 5) FillRect(b, S, S, 3, y, 10, 2, rung);
        s = Make(b, S, S);
        _cache["ladder"] = s;
        return s;
    }

    // =========================================================
    //  CRISTAL coletável (com animação de giro)
    // =========================================================
    static Sprite CrystalFrame(float wScale)
    {
        var b = NewBuf(S, S);
        float cx = 8f;
        int half = Mathf.Max(1, Mathf.RoundToInt(5 * wScale));
        for (int y = 2; y <= 13; y++)
        {
            // largura do losango varia com a altura (forma de diamante)
            float t = 1f - Mathf.Abs(y - 7.5f) / 6f;
            int wEdge = Mathf.RoundToInt(half * t);
            for (int x = -wEdge; x <= wEdge; x++)
            {
                Color32 c = (x <= -wEdge + 1) ? C_EDGE : (x >= wEdge - 1 ? C_EDGE : C_BODY);
                Px(b, S, S, (int)cx + x, y, c);
            }
        }
        // realce
        Px(b, S, S, 7, 9, C_HI); Px(b, S, S, 7, 10, C_HI); Px(b, S, S, 8, 8, C_HI);
        return Make(b, S, S);
    }

    public static Sprite[] Crystal()
    {
        if (_cacheAnim.TryGetValue("crystal", out var a)) return a;
        a = new[] { CrystalFrame(1f), CrystalFrame(0.6f), CrystalFrame(0.2f), CrystalFrame(0.6f) };
        _cacheAnim["crystal"] = a;
        return a;
    }

    // =========================================================
    //  INIMIGO (criatura das sombras, com leve animação)
    // =========================================================
    static Sprite EnemyFrame(int squash)
    {
        var b = NewBuf(S, S);
        float cy = 7f - squash * 0.5f;
        float r = 5f + squash * 0.4f;
        Disc(b, S, S, 8f, cy, r, E_BODY);
        FillRect(b, S, S, 3, 1, S - 6, 2, E_DARK); // base
        // espinhos no topo
        Px(b, S, S, 5, (int)(cy + r) - 1, E_DARK);
        Px(b, S, S, 8, (int)(cy + r), E_DARK);
        Px(b, S, S, 11, (int)(cy + r) - 1, E_DARK);
        // olhos
        FillRect(b, S, S, 5, (int)cy, 2, 2, E_EYE);
        FillRect(b, S, S, 9, (int)cy, 2, 2, E_EYE);
        return Make(b, S, S);
    }

    public static Sprite[] Enemy()
    {
        if (_cacheAnim.TryGetValue("enemy", out var a)) return a;
        a = new[] { EnemyFrame(0), EnemyFrame(1) };
        _cacheAnim["enemy"] = a;
        return a;
    }

    // =========================================================
    //  ESPINHOS (perigo)
    // =========================================================
    public static Sprite Spike()
    {
        if (_cache.TryGetValue("spike", out var s)) return s;
        var b = NewBuf(S, S);
        // três triângulos apontando para cima
        int[] bases = { 1, 6, 11 };
        foreach (int bx in bases)
        {
            for (int y = 0; y < 8; y++)
            {
                int half = (8 - y) / 2;
                for (int x = bx + 2 - half; x <= bx + 2 + half; x++)
                {
                    Color32 c = (x == bx + 2 - half || x == bx + 2 + half) ? H_DARK : H_BODY;
                    Px(b, S, S, x, y, c);
                }
            }
        }
        FillRect(b, S, S, 0, 0, S, 1, H_DARK);
        s = Make(b, S, S);
        _cache["spike"] = s;
        return s;
    }

    // =========================================================
    //  PORTAL DE SAÍDA (a luz) — 16x24, animado (pulsa)
    // =========================================================
    static Sprite ExitFrame(float glow)
    {
        int w = 16, h = 24;
        var b = NewBuf(w, h);
        float cx = 8f, cy = 12f;
        // halo
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float dx = (x - cx) / 6f, dy = (y - cy) / 10f;
                float d = dx * dx + dy * dy;
                if (d <= 1f)
                {
                    byte a = (byte)Mathf.Clamp(255 * (1f - d) * glow + 60, 0, 255);
                    Color32 c = d < 0.35f ? X_CORE : X_GLOW;
                    c.a = a;
                    Px(b, w, h, x, y, c);
                }
            }
        return Sprite.Create(MakeTex(b, w, h), new Rect(0, 0, w, h), new Vector2(0.5f, 0.3f), PPU);
    }

    static Texture2D MakeTex(Color32[] buf, int w, int h)
    {
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.SetPixels32(buf);
        tex.Apply();
        return tex;
    }

    public static Sprite[] Exit()
    {
        if (_cacheAnim.TryGetValue("exit", out var a)) return a;
        a = new[] { ExitFrame(1f), ExitFrame(0.7f), ExitFrame(0.85f), ExitFrame(0.7f) };
        _cacheAnim["exit"] = a;
        return a;
    }

    // =========================================================
    //  ÍCONES de HUD
    // =========================================================
    public static Sprite Heart()
    {
        if (_cache.TryGetValue("heart", out var s)) return s;
        var b = NewBuf(S, S);
        Disc(b, S, S, 5.5f, 10f, 3.2f, HEART);
        Disc(b, S, S, 10.5f, 10f, 3.2f, HEART);
        for (int y = 2; y <= 10; y++)
        {
            int half = (y - 2) / 1;
            int wEdge = Mathf.Max(0, 6 - (10 - y));
            for (int x = 8 - wEdge; x <= 8 + wEdge; x++) Px(b, S, S, x, y, HEART);
        }
        Disc(b, S, S, 5f, 11f, 1.2f, HEART_HI);
        s = Make(b, S, S);
        _cache["heart"] = s;
        return s;
    }

    public static Sprite CrystalIcon()
    {
        if (_cache.TryGetValue("crystal_icon", out var s)) return s;
        s = CrystalFrame(1f);
        _cache["crystal_icon"] = s;
        return s;
    }

    // Fundo: gradiente vertical escuro com "estrelas" e brilhos sutis
    public static Sprite Background()
    {
        if (_cache.TryGetValue("bg", out var s)) return s;
        int w = 64, h = 36;
        var b = NewBuf(w, h);
        for (int y = 0; y < h; y++)
        {
            float t = (float)y / (h - 1);
            byte r = (byte)Mathf.Lerp(12, 30, t);
            byte g = (byte)Mathf.Lerp(14, 22, t);
            byte bl = (byte)Mathf.Lerp(26, 52, t);
            for (int x = 0; x < w; x++) Px(b, w, h, x, y, new Color32(r, g, bl, 255));
        }
        // brilhos suaves
        Disc(b, w, h, 16f, 26f, 9f, new Color32(40, 50, 90, 255));
        Disc(b, w, h, 48f, 10f, 8f, new Color32(34, 30, 60, 255));
        // estrelas
        var rng = new System.Random(7);
        for (int i = 0; i < 70; i++)
        {
            int x = rng.Next(w), y = rng.Next(h);
            byte v = (byte)rng.Next(120, 230);
            Px(b, w, h, x, y, new Color32(v, v, (byte)Mathf.Min(255, v + 20), 255));
        }
        s = Sprite.Create(MakeTex(b, w, h), new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), PPU);
        _cache["bg"] = s;
        return s;
    }

    // Pixel branco 1x1 (para barras/fundos de UI e partículas)
    public static Sprite White()
    {
        if (_cache.TryGetValue("white", out var s)) return s;
        var b = new Color32[] { new Color32(255, 255, 255, 255) };
        var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        tex.SetPixels32(b);
        tex.Apply();
        s = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);
        _cache["white"] = s;
        return s;
    }
}
