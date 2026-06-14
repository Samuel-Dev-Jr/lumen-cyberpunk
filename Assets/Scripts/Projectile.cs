using UnityEngine;

// o tiro. tanto o player quanto os inimigos/boss usam o mesmo script.
// o _fromPlayer marca de quem foi o tiro pra ele saber quem pode acertar.
public class Projectile : MonoBehaviour
{
    Vector2 _dir;
    float _speed;
    bool _fromPlayer;
    float _life = 2.5f;

    // cria o tiro na hora ja com tudo configurado (sem precisar de prefab)
    public static Projectile Spawn(Vector3 pos, Vector2 dir, float speed, bool fromPlayer, Transform parent)
    {
        var go = new GameObject(fromPlayer ? "PlayerBolt" : "EnemyBolt");
        go.transform.SetParent(parent);
        go.transform.position = pos;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteFactory.Projectile(fromPlayer);
        sr.sortingOrder = 8;
        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.18f;
        var p = go.AddComponent<Projectile>();
        p._dir = dir.normalized; p._speed = speed; p._fromPlayer = fromPlayer;
        return p;
    }

    void Update()
    {
        transform.position += (Vector3)(_dir * _speed * Time.deltaTime);
        _life -= Time.deltaTime;
        if (_life <= 0f) Destroy(gameObject); // se voar muito tempo sem bater em nada eu apago, senao enche a cena de tiro
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (_fromPlayer)
        {
            if (other.GetComponent<PlayerController>() != null) return; // tiro do player nao acerta o proprio player
            var boss = other.GetComponent<Boss>();
            if (boss != null) { boss.Hit(1); Destroy(gameObject); return; }
            var enemy = other.GetComponent<Enemy>();
            if (enemy != null) { enemy.Hit(); Destroy(gameObject); return; }
        }
        else
        {
            // tiro de inimigo: passa direto por outros inimigos e pelo boss, so machuca a Luna
            if (other.GetComponent<Enemy>() != null || other.GetComponent<Boss>() != null) return;
            var p = other.GetComponent<PlayerController>();
            if (p != null) { p.TakeDamage(transform.position); Destroy(gameObject); return; }
        }
        // se bateu em parede/chao (coisa solida, nao trigger) o tiro some
        if (!other.isTrigger) Destroy(gameObject);
    }
}
