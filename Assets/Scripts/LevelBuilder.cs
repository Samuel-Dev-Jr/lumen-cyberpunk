using UnityEngine;

/// <summary>Informações resultantes da construção de uma fase.</summary>
public class LevelInfo
{
    public GameObject player;
    public Vector3 playerStart;
    public int crystals;
    public float minX, maxX, minY, maxY;
}

/// <summary>
/// Lê um mapa ASCII (LevelData) e instancia todos os objetos da fase em tempo
/// de execução: blocos, escadas, espinhos, cristais, inimigos, saída e o jogador.
/// Tudo fica sob um objeto-pai "Level" para facilitar a limpeza entre fases.
/// </summary>
public static class LevelBuilder
{
    public static LevelInfo Build(string[] map, Transform parent)
    {
        int rows = map.Length;
        int width = 0;
        foreach (var line in map) if (line.Length > width) width = line.Length;

        var info = new LevelInfo
        {
            minX = -1f,
            maxX = width,
            minY = -1f,
            maxY = rows,
        };

        for (int row = 0; row < rows; row++)
        {
            string line = map[row];
            for (int col = 0; col < line.Length; col++)
            {
                char c = line[col];
                if (c == ' ') continue;
                float x = col;
                float y = rows - 1 - row; // primeira linha = topo

                switch (c)
                {
                    case '#': MakeTile(x, y, IsTopTile(map, row, col), parent); break;
                    case 'H': MakeLadder(x, y, parent); break;
                    case 'o': MakeCrystal(x, y, parent); info.crystals++; break;
                    case '^': MakeSpike(x, y, parent); break;
                    case 'E': MakeEnemy(x, y, parent); break;
                    case 'X': MakeExit(x, y, parent); break;
                    case 'P':
                        info.playerStart = new Vector3(x, y, 0f);
                        break;
                }
            }
        }

        info.player = MakePlayer(info.playerStart, parent);
        info.player.GetComponent<PlayerController>().killY = info.minY - 8f;
        return info;
    }

    static char CharAt(string[] map, int row, int col)
    {
        if (row < 0 || row >= map.Length) return ' ';
        string line = map[row];
        return col < line.Length ? line[col] : ' ';
    }

    // bloco com "grama" no topo quando não há bloco logo acima
    static bool IsTopTile(string[] map, int row, int col) => CharAt(map, row - 1, col) != '#';

    static GameObject NewObj(string name, float x, float y, Transform parent, int order)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent);
        go.transform.position = new Vector3(x, y, 0f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sortingOrder = order;
        return go;
    }

    static void MakeTile(float x, float y, bool grassTop, Transform parent)
    {
        var go = NewObj("Tile", x, y, parent, 0);
        go.GetComponent<SpriteRenderer>().sprite = SpriteFactory.Tile(grassTop);
        var col = go.AddComponent<BoxCollider2D>();
        col.size = Vector2.one;
    }

    static void MakeLadder(float x, float y, Transform parent)
    {
        var go = NewObj("Ladder", x, y, parent, -1);
        go.GetComponent<SpriteRenderer>().sprite = SpriteFactory.Ladder();
        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.7f, 1f);
        go.AddComponent<Ladder>();
    }

    static void MakeCrystal(float x, float y, Transform parent)
    {
        var go = NewObj("Crystal", x, y, parent, 3);
        go.AddComponent<SpriteAnimator>().Play(SpriteFactory.Crystal(), 8f, true);
        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.35f;
        go.AddComponent<Collectible>();
    }

    static void MakeSpike(float x, float y, Transform parent)
    {
        var go = NewObj("Spike", x, y, parent, 2);
        go.GetComponent<SpriteRenderer>().sprite = SpriteFactory.Spike();
        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.9f, 0.45f);
        col.offset = new Vector2(0f, -0.22f);
        go.AddComponent<Hazard>();
    }

    static void MakeEnemy(float x, float y, Transform parent)
    {
        var go = NewObj("Enemy", x, y, parent, 4);
        go.AddComponent<SpriteAnimator>().Play(SpriteFactory.Enemy(), 4f, true);
        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.7f, 0.7f);
        go.AddComponent<Enemy>();
    }

    static void MakeExit(float x, float y, Transform parent)
    {
        var go = NewObj("Exit", x, y, parent, 2);
        go.AddComponent<SpriteAnimator>().Play(SpriteFactory.Exit(), 6f, true);
        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.9f, 1.5f);
        col.offset = new Vector2(0f, 0.45f);
        go.AddComponent<LevelExit>();
    }

    static GameObject MakePlayer(Vector3 pos, Transform parent)
    {
        var go = new GameObject("Player");
        go.transform.SetParent(parent);
        go.transform.position = pos;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 10;
        sr.sprite = SpriteFactory.PlayerIdle()[0];

        go.AddComponent<SpriteAnimator>();

        var rb = go.AddComponent<Rigidbody2D>();
        rb.freezeRotation = true;

        var col = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.6f, 0.85f);
        col.offset = new Vector2(0f, -0.05f);

        go.AddComponent<PlayerController>();
        return go;
    }
}
