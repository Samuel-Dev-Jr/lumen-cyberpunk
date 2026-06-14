using System.Collections.Generic;
using UnityEngine;

// aqui eu desenho todos os sprites por codigo, pixel a pixel, pra nao depender de imagem nenhuma.
// tema cyberpunk, paleta neon (ciano/magenta/amarelo) no fundo escuro.
// PPU = 16, entao cada tile de 16px vira 1 unidade no mundo. pivo no centro (com umas excecoes)
public static class SpriteFactory
{
    public const int PPU = 16;
    const int S = 16;

    static readonly Color32 CLEAR = new Color32(0, 0, 0, 0);

    // ----- a paleta neon que uso em tudo -----
    static readonly Color32 NCYAN = new Color32(40, 220, 255, 255);
    static readonly Color32 NCYAN_D = new Color32(20, 130, 180, 255);
    static readonly Color32 NMAG = new Color32(255, 60, 200, 255);
    static readonly Color32 NYEL = new Color32(255, 225, 70, 255);
    static readonly Color32 NPURP = new Color32(150, 80, 255, 255);
    static readonly Color32 NRED = new Color32(255, 70, 90, 255);
    static readonly Color32 WHITE = new Color32(240, 250, 255, 255);
    static readonly Color32 METAL = new Color32(42, 48, 78, 255);
    static readonly Color32 METAL_D = new Color32(24, 28, 50, 255);
    static readonly Color32 METAL_L = new Color32(64, 72, 110, 255);
    static readonly Color32 INK = new Color32(10, 12, 22, 255);

    static readonly Dictionary<string, Sprite> _cache = new Dictionary<string, Sprite>();
    static readonly Dictionary<string, Sprite[]> _cacheAnim = new Dictionary<string, Sprite[]>();

    // ========================= funcoezinhas pra desenhar =========================
    static Color32[] NewBuf(int w, int h)
    {
        var b = new Color32[w * h];
        for (int i = 0; i < b.Length; i++) b[i] = CLEAR;
        return b;
    }
    static void Px(Color32[] b, int w, int h, int x, int y, Color32 c)
    {
        if (x < 0 || y < 0 || x >= w || y >= h) return;
        b[y * w + x] = c;
    }
    static void FillRect(Color32[] b, int w, int h, int x0, int y0, int rw, int rh, Color32 c)
    {
        for (int y = y0; y < y0 + rh; y++)
            for (int x = x0; x < x0 + rw; x++) Px(b, w, h, x, y, c);
    }
    static void Disc(Color32[] b, int w, int h, float cx, float cy, float r, Color32 c)
    {
        int x0 = Mathf.FloorToInt(cx - r), x1 = Mathf.CeilToInt(cx + r);
        int y0 = Mathf.FloorToInt(cy - r), y1 = Mathf.CeilToInt(cy + r);
        for (int y = y0; y <= y1; y++)
            for (int x = x0; x <= x1; x++)
            {
                float dx = x + 0.5f - cx, dy = y + 0.5f - cy;
                if (dx * dx + dy * dy <= r * r) Px(b, w, h, x, y, c);
            }
    }
    static void Ring(Color32[] b, int w, int h, float cx, float cy, float r, float thick, Color32 c)
    {
        int x0 = Mathf.FloorToInt(cx - r), x1 = Mathf.CeilToInt(cx + r);
        int y0 = Mathf.FloorToInt(cy - r), y1 = Mathf.CeilToInt(cy + r);
        for (int y = y0; y <= y1; y++)
            for (int x = x0; x <= x1; x++)
            {
                float dx = x + 0.5f - cx, dy = y + 0.5f - cy;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                if (d <= r && d >= r - thick) Px(b, w, h, x, y, c);
            }
    }
    static Texture2D MakeTex(Color32[] b, int w, int h)
    {
        var t = new Texture2D(w, h, TextureFormat.RGBA32, false);
        t.filterMode = FilterMode.Point;
        t.wrapMode = TextureWrapMode.Clamp;
        t.SetPixels32(b);
        t.Apply();
        return t;
    }
    static Sprite Make(Color32[] b, int w, int h) =>
        Sprite.Create(MakeTex(b, w, h), new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), PPU);
    static Sprite MakePivot(Color32[] b, int w, int h, float px, float py) =>
        Sprite.Create(MakeTex(b, w, h), new Rect(0, 0, w, h), new Vector2(px, py), PPU);

    // ========================= o personagem =========================
    // monto o frame do player na mao. os parametros mudam os pes, a altura do corpo e os bracos
    static Sprite PlayerFrame(int feetMode, int bodyYOffset, bool armsUp, bool shooting)
    {
        var b = NewBuf(S, S);
        float cy = 8.5f + bodyYOffset;
        Disc(b, S, S, 8f, cy, 6.2f, new Color32(40, 220, 255, 70)); // o brilho em volta
        Disc(b, S, S, 8f, cy, 5f, NCYAN);                            // corpo
        Disc(b, S, S, 6.4f, cy + 1.6f, 2.1f, WHITE);                 // pontinho de luz
        FillRect(b, S, S, 5, (int)cy, 6, 2, NMAG);                   // o visor magenta
        Px(b, S, S, 5, (int)cy, INK); Px(b, S, S, 10, (int)cy, INK);

        if (armsUp) { FillRect(b, S, S, 3, (int)cy + 3, 1, 2, NCYAN); FillRect(b, S, S, 12, (int)cy + 3, 1, 2, NCYAN); }
        if (shooting) { FillRect(b, S, S, 11, (int)cy - 1, 4, 2, WHITE); Px(b, S, S, 15, (int)cy, NYEL); } // braco esticado com a arma

        switch (feetMode)
        {
            case 0: FillRect(b, S, S, 5, 1, 2, 2, NCYAN_D); FillRect(b, S, S, 9, 1, 2, 2, NCYAN_D); break;
            case 1: FillRect(b, S, S, 4, 1, 2, 2, NCYAN_D); FillRect(b, S, S, 9, 2, 2, 2, NCYAN_D); break;
            case 2: FillRect(b, S, S, 5, 2, 2, 2, NCYAN_D); FillRect(b, S, S, 10, 1, 2, 2, NCYAN_D); break;
            case 3: FillRect(b, S, S, 5, 3, 2, 2, NCYAN_D); FillRect(b, S, S, 9, 3, 2, 2, NCYAN_D); break;
        }
        return Make(b, S, S);
    }

    public static Sprite[] PlayerIdle() => Anim("p_idle", () => new[] { PlayerFrame(0, 0, false, false), PlayerFrame(0, 1, false, false) });
    public static Sprite[] PlayerWalk() => Anim("p_walk", () => new[] { PlayerFrame(1, 0, false, false), PlayerFrame(0, 1, false, false), PlayerFrame(2, 0, false, false), PlayerFrame(0, 1, false, false) });
    public static Sprite[] PlayerJump() => Anim("p_jump", () => new[] { PlayerFrame(3, 1, false, false) });
    public static Sprite[] PlayerFall() => Anim("p_fall", () => new[] { PlayerFrame(3, 0, false, false) });
    public static Sprite[] PlayerClimb() => Anim("p_climb", () => new[] { PlayerFrame(1, 0, true, false), PlayerFrame(2, 0, true, false) });
    public static Sprite[] PlayerShoot() => Anim("p_shoot", () => new[] { PlayerFrame(0, 0, false, true) });

    // ========================= os tiles do chao =========================
    public static Sprite Tile(bool neonTop)
    {
        string key = neonTop ? "tile_top" : "tile_solid";
        if (_cache.TryGetValue(key, out var s)) return s;
        var b = NewBuf(S, S);
        FillRect(b, S, S, 0, 0, S, S, METAL);
        FillRect(b, S, S, 0, 0, S, 1, METAL_D);
        FillRect(b, S, S, S - 1, 0, 1, S, METAL_D);
        FillRect(b, S, S, 0, 0, 1, S, METAL_L);
        // uns detalhes pra nao ficar liso demais
        Px(b, S, S, 4, 5, METAL_D); Px(b, S, S, 11, 8, METAL_D); Px(b, S, S, 7, 11, METAL_L);
        if (neonTop)
        {
            FillRect(b, S, S, 0, S - 2, S, 2, NCYAN);            // a faixa neon que so aparece no tile de cima
            FillRect(b, S, S, 0, S - 3, S, 1, new Color32(40, 220, 255, 120));
        }
        s = Make(b, S, S); _cache[key] = s; return s;
    }

    public static Sprite Ladder()
    {
        if (_cache.TryGetValue("ladder", out var s)) return s;
        var b = NewBuf(S, S);
        Color32 rail = new Color32(120, 80, 200, 255);
        for (int y = 0; y < S; y++) { Px(b, S, S, 3, y, rail); Px(b, S, S, 4, y, NPURP); Px(b, S, S, 11, y, NPURP); Px(b, S, S, 12, y, rail); }
        for (int y = 1; y < S; y += 5) FillRect(b, S, S, 3, y, 10, 1, NCYAN);
        s = Make(b, S, S); _cache["ladder"] = s; return s;
    }

    // ========================= o cristal/shard que da pra coletar =========================
    static Sprite ShardFrame(float wScale)
    {
        var b = NewBuf(S, S);
        int half = Mathf.Max(1, Mathf.RoundToInt(5 * wScale));
        for (int y = 2; y <= 13; y++)
        {
            float t = 1f - Mathf.Abs(y - 7.5f) / 6f;
            int we = Mathf.RoundToInt(half * t);
            for (int x = -we; x <= we; x++)
            {
                Color32 c = (Mathf.Abs(x) >= we - 1) ? NCYAN_D : NCYAN;
                Px(b, S, S, 8 + x, y, c);
            }
        }
        Px(b, S, S, 7, 9, WHITE); Px(b, S, S, 8, 8, WHITE); Px(b, S, S, 7, 10, WHITE);
        return Make(b, S, S);
    }
    public static Sprite[] Crystal() => Anim("shard", () => new[] { ShardFrame(1f), ShardFrame(0.6f), ShardFrame(0.2f), ShardFrame(0.6f) });

    // ========================= o inimigo que anda no chao =========================
    // o squash so achata um pouco o corpo pra dar a impressao de movimento
    static Sprite EnemyFrame(int squash)
    {
        var b = NewBuf(S, S);
        float cy = 7f - squash * 0.5f;
        FillRect(b, S, S, 3, (int)cy - 4, 10, 9, METAL);     // corpo
        FillRect(b, S, S, 3, (int)cy - 4, 10, 1, METAL_D);
        FillRect(b, S, S, 3, 1, 10, 2, METAL_D);             // base
        FillRect(b, S, S, 4, (int)cy, 8, 2, NRED);           // visor vermelho
        Px(b, S, S, 5, (int)cy, WHITE); Px(b, S, S, 10, (int)cy, WHITE);
        Px(b, S, S, 8, (int)cy + 6, NRED);                   // a anteninha
        FillRect(b, S, S, 3, (int)cy - 4, 1, 9, NMAG);       // as faixas neon dos lados
        FillRect(b, S, S, 12, (int)cy - 4, 1, 9, NMAG);
        return Make(b, S, S);
    }
    public static Sprite[] Enemy() => Anim("enemy", () => new[] { EnemyFrame(0), EnemyFrame(1) });

    // ========================= o drone que voa =========================
    static Sprite DroneFrame(int rotor)
    {
        var b = NewBuf(S, S);
        Disc(b, S, S, 8f, 8f, 5.5f, new Color32(40, 220, 255, 60)); // brilho
        Disc(b, S, S, 8f, 8f, 3.6f, METAL);                          // corpo
        Ring(b, S, S, 8f, 8f, 3.6f, 1f, NCYAN);
        FillRect(b, S, S, 6, 7, 4, 2, NRED);                         // o olho
        // os rotores dos lados, eles mudam de tamanho pra dar a animacao
        int rx = rotor == 0 ? 1 : 2;
        FillRect(b, S, S, 0, 8, 3, rx, NMAG);
        FillRect(b, S, S, 13, 8, 3, rx, NMAG);
        return Make(b, S, S);
    }
    public static Sprite[] Drone() => Anim("drone", () => new[] { DroneFrame(0), DroneFrame(1) });

    // ========================= os espinhos que machucam =========================
    public static Sprite Spike()
    {
        if (_cache.TryGetValue("spike", out var s)) return s;
        var b = NewBuf(S, S);
        int[] bases = { 1, 6, 11 };
        foreach (int bx in bases)
            for (int y = 0; y < 9; y++)
            {
                int half = (9 - y) / 2;
                for (int x = bx + 2 - half; x <= bx + 2 + half; x++)
                {
                    Color32 c = (x == bx + 2 - half || x == bx + 2 + half) ? NMAG : new Color32(255, 130, 230, 255);
                    Px(b, S, S, x, y, c);
                }
            }
        FillRect(b, S, S, 0, 0, S, 1, NMAG);
        s = Make(b, S, S); _cache["spike"] = s; return s;
    }

    // ========================= o portal de saida da fase =========================
    static Sprite ExitFrame(float glow)
    {
        int w = 16, h = 24;
        var b = NewBuf(w, h);
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float dx = (x - 8f) / 6f, dy = (y - 12f) / 10f, d = dx * dx + dy * dy;
                if (d <= 1f)
                {
                    byte a = (byte)Mathf.Clamp(255 * (1f - d) * glow + 50, 0, 255);
                    Color32 c = d < 0.4f ? WHITE : (y % 4 < 2 ? NCYAN : NMAG);
                    c.a = a; Px(b, w, h, x, y, c);
                }
            }
        return MakePivot(b, w, h, 0.5f, 0.3f);
    }
    public static Sprite[] Exit() => Anim("exit", () => new[] { ExitFrame(1f), ExitFrame(0.7f), ExitFrame(0.9f), ExitFrame(0.7f) });

    // ========================= a arma que da pra pegar =========================
    public static Sprite Weapon()
    {
        if (_cache.TryGetValue("weapon", out var s)) return s;
        var b = NewBuf(S, S);
        Disc(b, S, S, 8f, 8f, 6f, new Color32(255, 225, 70, 50));
        FillRect(b, S, S, 3, 6, 9, 3, METAL_L);     // o corpo da arma
        FillRect(b, S, S, 3, 6, 9, 1, NYEL);
        FillRect(b, S, S, 11, 7, 3, 1, NCYAN);      // o cano
        FillRect(b, S, S, 4, 3, 2, 4, METAL);       // o cabo
        s = Make(b, S, S); _cache["weapon"] = s; return s;
    }

    // ========================= o tiro =========================
    public static Sprite Projectile(bool player)
    {
        string key = player ? "proj_p" : "proj_e";
        if (_cache.TryGetValue(key, out var s)) return s;
        int w = 8, h = 6;
        var b = NewBuf(w, h);
        Color32 core = WHITE;
        Color32 edge = player ? NCYAN : NRED;
        Disc(b, w, h, 4f, 3f, 2.6f, edge);
        Disc(b, w, h, 4f, 3f, 1.3f, core);
        FillRect(b, w, h, 0, 2, 3, 2, edge); // o rastrinho atras
        s = Make(b, w, h); _cache[key] = s; return s;
    }

    // ========================= o chefao =========================
    // é bem maior (48x48). o phase troca o tamanho do olho pra ele ficar pulsando
    static Sprite BossFrame(int phase)
    {
        int w = 48, h = 48;
        var b = NewBuf(w, h);
        Disc(b, w, h, 24f, 24f, 22f, new Color32(255, 60, 200, 40)); // aura por tras
        FillRect(b, w, h, 8, 8, 32, 32, METAL);                       // corpo
        FillRect(b, w, h, 8, 8, 32, 2, METAL_D);
        FillRect(b, w, h, 8, 38, 32, 2, METAL_L);
        // a borda neon em volta
        for (int x = 8; x < 40; x++) { Px(b, w, h, x, 8, NMAG); Px(b, w, h, x, 39, NMAG); }
        for (int y = 8; y < 40; y++) { Px(b, w, h, 8, y, NCYAN); Px(b, w, h, 39, y, NCYAN); }
        // o olho do meio, que pulsa conforme o phase
        float er = phase == 0 ? 7f : 8.5f;
        Disc(b, w, h, 24f, 24f, er, new Color32(255, 70, 90, 255));
        Disc(b, w, h, 24f, 24f, er - 3f, NYEL);
        Disc(b, w, h, 24f, 24f, 2f, WHITE);
        // umas antenas em cima, tipo chifre
        FillRect(b, w, h, 12, 40, 3, 5, NCYAN);
        FillRect(b, w, h, 33, 40, 3, 5, NCYAN);
        return Make(b, w, h);
    }
    public static Sprite[] Boss() => Anim("boss", () => new[] { BossFrame(0), BossFrame(1) });

    // ========================= os iconinhos do HUD =========================
    public static Sprite Heart()
    {
        if (_cache.TryGetValue("heart", out var s)) return s;
        var b = NewBuf(S, S);
        Disc(b, S, S, 5.5f, 10f, 3.2f, NMAG);
        Disc(b, S, S, 10.5f, 10f, 3.2f, NMAG);
        for (int y = 2; y <= 10; y++) { int we = Mathf.Max(0, 6 - (10 - y)); for (int x = 8 - we; x <= 8 + we; x++) Px(b, S, S, x, y, NMAG); }
        Disc(b, S, S, 5f, 11f, 1.2f, WHITE);
        s = Make(b, S, S); _cache["heart"] = s; return s;
    }
    public static Sprite CrystalIcon()
    {
        if (_cache.TryGetValue("shard_icon", out var s)) return s;
        s = ShardFrame(1f); _cache["shard_icon"] = s; return s;
    }
    public static Sprite WeaponIcon() => Weapon();

    public static Sprite White()
    {
        if (_cache.TryGetValue("white", out var s)) return s;
        var t = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        t.filterMode = FilterMode.Point; t.SetPixels32(new[] { new Color32(255, 255, 255, 255) }); t.Apply();
        s = Sprite.Create(t, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);
        _cache["white"] = s; return s;
    }

    // ========================= o fundo da cidade (muda por fase) =========================
    // cada fase tem sua cor de ceu: {cor de cima, cor de baixo, neon das janelas}
    static readonly Color32[][] SkyPalettes = {
        new[]{ new Color32(36,52,110,255), new Color32(12,16,40,255),  new Color32(70,240,255,255)  }, // ciano
        new[]{ new Color32(70,34,120,255), new Color32(16,10,40,255),  new Color32(185,110,255,255) }, // roxo
        new[]{ new Color32(104,28,104,255),new Color32(22,8,34,255),   new Color32(255,90,225,255)  }, // magenta
        new[]{ new Color32(112,52,46,255), new Color32(26,12,18,255),  new Color32(255,175,80,255)  }, // laranja
        new[]{ new Color32(118,24,40,255), new Color32(28,6,14,255),   new Color32(255,95,120,255)  }, // vermelho (boss)
    };

    public static Sprite Background(int level)
    {
        string key = "bg" + level;
        if (_cache.TryGetValue(key, out var s)) return s;
        var pal = SkyPalettes[Mathf.Clamp(level, 0, SkyPalettes.Length - 1)];
        int w = 96, h = 54;
        var b = NewBuf(w, h);
        // primeiro o degrade do ceu, de baixo pra cima
        for (int y = 0; y < h; y++)
        {
            float t = (float)y / (h - 1);
            byte r = (byte)Mathf.Lerp(pal[1].r, pal[0].r, t);
            byte g = (byte)Mathf.Lerp(pal[1].g, pal[0].g, t);
            byte bl = (byte)Mathf.Lerp(pal[1].b, pal[0].b, t);
            for (int x = 0; x < w; x++) Px(b, w, h, x, y, new Color32(r, g, bl, 255));
        }
        // uma lua/brilho neon no canto
        Disc(b, w, h, 22f, 40f, 12f, new Color32(pal[2].r, pal[2].g, pal[2].b, 70));
        Disc(b, w, h, 22f, 40f, 7f, new Color32(pal[2].r, pal[2].g, pal[2].b, 110));
        // os predios. uso seed fixa pra fase ficar sempre igual
        var rng = new System.Random(100 + level);
        int x0 = 0;
        while (x0 < w)
        {
            int bw = rng.Next(7, 14);
            int bh = rng.Next(10, 30);
            Color32 bld = new Color32(8, 9, 18, 255);
            FillRect(b, w, h, x0, 0, bw, bh, bld);
            // uma linha neon no topo de cada predio
            for (int x = x0; x < x0 + bw && x < w; x++) Px(b, w, h, x, bh - 1, new Color32(pal[2].r, pal[2].g, pal[2].b, 120));
            // as janelinhas acesas (nem todas, pra dar variedade)
            for (int wy = 2; wy < bh - 2; wy += 3)
                for (int wx = x0 + 2; wx < x0 + bw - 1; wx += 3)
                    if (rng.NextDouble() > 0.45) Px(b, w, h, wx, wy, pal[2]);
            x0 += bw + 1;
        }
        // umas estrelinhas pra preencher o ceu
        for (int i = 0; i < 50; i++)
        {
            int x = rng.Next(w), y = rng.Next(h / 2, h);
            byte v = (byte)rng.Next(120, 220);
            Px(b, w, h, x, y, new Color32(v, v, 255, 200));
        }
        s = Make(b, w, h); _cache[key] = s; return s;
    }

    // guardo as animacoes no cache pra nao ficar redesenhando os frames toda hora
    static Sprite[] Anim(string key, System.Func<Sprite[]> make)
    {
        if (_cacheAnim.TryGetValue(key, out var a)) return a;
        a = make(); _cacheAnim[key] = a; return a;
    }
}
