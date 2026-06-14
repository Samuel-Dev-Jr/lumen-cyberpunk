using UnityEngine;

/// <summary>
/// Inimigo. Dois tipos: terrestre (patrulha o chão, vira em paredes/beiradas) e
/// voador/drone (flutua e vai-e-vem no ar). Causa dano por contato, pode ser
/// pisado ou destruído com tiro, e às vezes dropa uma arma ao morrer.
/// </summary>
public class Enemy : MonoBehaviour
{
    public float speed = 2f;
    public bool flying = false;

    float _dir = 1f, _spawnX, _baseY;
    bool _dead;
    SpriteRenderer _sr;

    void Start()
    {
        _sr = GetComponent<SpriteRenderer>();
        _spawnX = transform.position.x;
        _baseY = transform.position.y;
        _dir = (Mathf.FloorToInt(transform.position.x) % 2 == 0) ? 1f : -1f;
    }

    void Update()
    {
        if (_dead) return;
        if (flying) FlyMove(); else GroundMove();
        if (_sr != null) _sr.flipX = _dir < 0;
    }

    void GroundMove()
    {
        Vector2 pos = transform.position;
        var wall = Physics2D.Raycast(pos, Vector2.right * _dir, 0.55f);
        var ahead = new Vector2(pos.x + _dir * 0.5f, pos.y - 0.3f);
        var ground = Physics2D.Raycast(ahead, Vector2.down, 0.9f);
        if (wall.collider != null || ground.collider == null) _dir = -_dir;
        transform.position += new Vector3(_dir * speed * Time.deltaTime, 0f, 0f);
    }

    void FlyMove()
    {
        var wall = Physics2D.Raycast(transform.position, Vector2.right * _dir, 0.6f);
        if (wall.collider != null || transform.position.x > _spawnX + 5f || transform.position.x < _spawnX - 5f)
            _dir = -_dir;
        var pos = transform.position;
        pos.x += _dir * speed * Time.deltaTime;
        pos.y = _baseY + Mathf.Sin(Time.time * 2.5f + _spawnX) * 0.6f;
        transform.position = pos;
    }

    void OnTriggerEnter2D(Collider2D other) => Contact(other);
    void OnTriggerStay2D(Collider2D other) => Contact(other);

    void Contact(Collider2D other)
    {
        if (_dead) return;
        var p = other.GetComponent<PlayerController>();
        if (p == null) return;
        if (!flying && p.Velocity.y < 0.5f && p.transform.position.y > transform.position.y + 0.35f)
        {
            Die(); p.Bounce();
        }
        else p.TakeDamage(transform.position);
    }

    /// <summary>Chamado por um projétil do jogador.</summary>
    public void Hit() { if (!_dead) Die(); }

    void Die()
    {
        _dead = true;
        AudioManager.Instance.PlayStomp();
        GameManager.Instance.AddScore(150);

        // chance de dropar uma arma
        if (Random.value < 0.4f)
        {
            var go = new GameObject("WeaponDrop");
            go.transform.SetParent(transform.parent);
            go.transform.position = transform.position;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteFactory.Weapon();
            sr.sortingOrder = 5;
            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true; col.radius = 0.45f;
            go.AddComponent<WeaponPickup>().ammo = 10;
        }

        if (_sr != null) _sr.flipY = true;
        var c = GetComponent<Collider2D>(); if (c != null) c.enabled = false;
        var anim = GetComponent<SpriteAnimator>(); if (anim != null) anim.enabled = false;
        transform.localScale = new Vector3(transform.localScale.x, 0.4f, 1f);
        Destroy(gameObject, 0.4f);
    }
}
