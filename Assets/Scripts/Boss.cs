using UnityEngine;

/// <summary>
/// Chefe final ("Mainframe"). Patrulha a arena, atira projéteis no jogador e causa
/// dano por contato. É derrotado com tiros (ou pisões). Ao morrer, vence o jogo.
/// </summary>
public class Boss : MonoBehaviour
{
    public int maxHp = 12;
    int _hp;
    float _spawnX, _range = 6f, _dir = 1f, _speed = 2.6f;
    float _baseY;
    float _shootTimer = 2f;
    float _flash;
    bool _dead;

    SpriteRenderer _sr;

    void Start()
    {
        _hp = maxHp;
        _sr = GetComponent<SpriteRenderer>();
        _spawnX = transform.position.x;
        _baseY = transform.position.y;
        GameManager.Instance.RegisterBoss(maxHp);
    }

    void Update()
    {
        if (_dead) return;

        // movimento vai-e-vem + flutuação
        transform.position += new Vector3(_dir * _speed * Time.deltaTime, 0f, 0f);
        if (transform.position.x > _spawnX + _range) _dir = -1f;
        if (transform.position.x < _spawnX - _range) _dir = 1f;
        var pos = transform.position;
        pos.y = _baseY + Mathf.Sin(Time.time * 1.5f) * 0.5f;
        transform.position = pos;

        // flash ao tomar dano
        if (_flash > 0f)
        {
            _flash -= Time.deltaTime;
            _sr.color = Color.Lerp(Color.white, new Color(1f, 0.5f, 0.5f), Mathf.PingPong(Time.time * 20f, 1f));
            if (_flash <= 0f) _sr.color = Color.white;
        }

        // ataque: atira em direção ao jogador
        _shootTimer -= Time.deltaTime;
        if (_shootTimer <= 0f)
        {
            _shootTimer = Mathf.Lerp(2.2f, 0.9f, 1f - (float)_hp / maxHp); // mais rápido com pouca vida
            Shoot();
        }
    }

    void Shoot()
    {
        var player = GameManager.Instance.Player;
        if (player == null) return;
        Vector2 to = (player.transform.position - transform.position).normalized;
        // tiro triplo em leque
        for (int i = -1; i <= 1; i++)
        {
            float ang = Mathf.Atan2(to.y, to.x) + i * 12f * Mathf.Deg2Rad;
            var dir = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang));
            Projectile.Spawn(transform.position + (Vector3)dir * 1.6f, dir, 7f, false, transform.parent);
        }
    }

    public void Hit(int dmg)
    {
        if (_dead) return;
        _hp -= dmg;
        _flash = 0.15f;
        AudioManager.Instance.PlayBossHit();
        GameManager.Instance.UpdateBossHealth(Mathf.Max(0, _hp));
        if (_hp <= 0) Die();
    }

    void Die()
    {
        _dead = true;
        AudioManager.Instance.PlayBossDie();
        // explosão de partículas
        for (int i = 0; i < 24; i++)
        {
            var go = new GameObject("boom");
            go.transform.position = transform.position;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteFactory.White();
            sr.color = (i % 2 == 0) ? new Color(0.16f, 0.86f, 1f) : new Color(1f, 0.24f, 0.78f);
            sr.sortingOrder = 30;
            go.transform.localScale = Vector3.one * 0.2f;
            float ang = i * 15f * Mathf.Deg2Rad;
            go.AddComponent<Spark>().Init(new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * Random.Range(3f, 7f), 0.8f);
        }
        GameManager.Instance.BossDefeated();
        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other) => Touch(other);
    void OnTriggerStay2D(Collider2D other) => Touch(other);

    void Touch(Collider2D other)
    {
        if (_dead) return;
        var p = other.GetComponent<PlayerController>();
        if (p == null) return;
        if (p.Velocity.y < 0.5f && p.transform.position.y > transform.position.y + 1.2f)
        {
            Hit(1); p.Bounce();
        }
        else p.TakeDamage(transform.position);
    }
}
