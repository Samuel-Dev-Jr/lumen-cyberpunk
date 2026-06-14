using UnityEngine;

// o que sobra depois de montar a fase (referencia do player, limites, etc)
public class LevelInfo
{
    public GameObject player;
    public Vector3 playerStart;
    public int crystals;
    public bool hasBoss;
    public float minX, maxX, minY, maxY;
}

// pego o mapa de texto do LevelData e vou criando tudo na fase: chao, escadas,
// espinhos, cristais, inimigos, a saida e o player. coloco tudo embaixo do "Level"
// pra ficar facil de apagar quando troca de fase.
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

        // joguei todos os blocos de chao num colisor so (CompositeCollider2D).
        // antes o player travava na "emenda" entre um bloco e outro, isso resolveu.
        var groundGO = new GameObject("Ground");
        groundGO.transform.SetParent(parent);
        var grb = groundGO.AddComponent<Rigidbody2D>();
        grb.bodyType = RigidbodyType2D.Static;
        var comp = groundGO.AddComponent<CompositeCollider2D>();
        comp.geometryType = CompositeCollider2D.GeometryType.Polygons;
        comp.generationType = CompositeCollider2D.GenerationType.Manual;
        Transform groundT = groundGO.transform;

        for (int row = 0; row < rows; row++)
        {
            string line = map[row];
            for (int col = 0; col < line.Length; col++)
            {
                char c = line[col];
                if (c == ' ') continue;
                float x = col;
                float y = rows - 1 - row; // a primeira linha do texto eh o topo

                switch (c)
                {
                    case '#': MakeTile(x, y, IsTopTile(map, row, col), groundT); break;
                    case 'H': MakeLadder(x, y, parent); break;
                    case 'o': MakeCrystal(x, y, parent); info.crystals++; break;
                    case '^': MakeSpike(x, y, parent); break;
                    case 'E': MakeEnemy(x, y, parent, false); break;
                    case 'F': MakeEnemy(x, y, parent, true); break;
                    case 'W': MakeWeapon(x, y, parent); break;
                    case 'L': MakeHeart(x, y, parent); break;
                    case 'B': MakeBoss(x, y, parent); info.hasBoss = true; break;
                    case 'X': MakeExit(x, y, parent); break;
                    case 'P':
                        info.playerStart = new Vector3(x, y, 0f);
                        break;
                }
            }
        }

        comp.GenerateGeometry(); // aqui ele gera o colisor unicao do chao

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

    // se nao tem bloco em cima, esse vira o de cima (com "grama")
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

    static void MakeTile(float x, float y, bool grassTop, Transform groundParent)
    {
        var go = NewObj("Tile", x, y, groundParent, 0);
        go.GetComponent<SpriteRenderer>().sprite = SpriteFactory.Tile(grassTop);
        var col = go.AddComponent<BoxCollider2D>();
        col.size = Vector2.one;
        col.usedByComposite = true; // pra ele entrar no colisor unico do chao
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

    static void MakeEnemy(float x, float y, Transform parent, bool flying)
    {
        var go = NewObj(flying ? "Drone" : "Enemy", x, y, parent, 4);
        go.AddComponent<SpriteAnimator>().Play(flying ? SpriteFactory.Drone() : SpriteFactory.Enemy(), flying ? 8f : 4f, true);
        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.7f, 0.7f);
        var e = go.AddComponent<Enemy>();
        e.flying = flying;
        if (flying) e.speed = 3f;
    }

    static void MakeWeapon(float x, float y, Transform parent)
    {
        var go = NewObj("Weapon", x, y, parent, 5);
        go.GetComponent<SpriteRenderer>().sprite = SpriteFactory.Weapon();
        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.45f;
        go.AddComponent<WeaponPickup>();
    }

    static void MakeBoss(float x, float y, Transform parent)
    {
        var go = NewObj("Boss", x, y, parent, 6);
        go.AddComponent<SpriteAnimator>().Play(SpriteFactory.Boss(), 2f, true);
        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(2.6f, 2.6f);
        go.AddComponent<Boss>();
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

        var rb = go.AddComponent<Rigidbody2D>();
        rb.freezeRotation = true;

        var col = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.6f, 0.85f);
        col.offset = new Vector2(0f, -0.05f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 10;
        sr.sprite = SpriteFactory.PlayerIdle()[0];
        go.AddComponent<SpriteAnimator>();

        go.AddComponent<PlayerController>();
        return go;
    }

    static void MakeHeart(float x, float y, Transform parent)
    {
        var go = NewObj("Heart", x, y, parent, 5);
        go.GetComponent<SpriteRenderer>().sprite = SpriteFactory.Heart();
        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.45f;
        go.AddComponent<HeartPickup>();
    }
}
