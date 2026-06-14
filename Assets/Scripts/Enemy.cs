using UnityEngine;

/// <summary>
/// Inimigo patrulheiro (criatura das sombras). Anda de um lado para o outro,
/// virando ao encontrar parede ou beirada. Causa dano por contato — mas pode
/// ser derrotado se o jogador pular em cima dele (mecânica de "pisão").
/// </summary>
public class Enemy : MonoBehaviour
{
    public float speed = 2f;
    int _dir = 1;
    bool _dead;
    SpriteRenderer _sr;

    void Start()
    {
        _sr = GetComponent<SpriteRenderer>();
        // direção inicial aleatória para variar
        _dir = (transform.position.x % 2f < 1f) ? 1 : -1;
    }

    void Update()
    {
        if (_dead) return;

        Vector2 pos = transform.position;
        // parede à frente?
        var wall = Physics2D.Raycast(pos, Vector2.right * _dir, 0.55f);
        // chão logo à frente? (para não cair da beirada)
        var ahead = new Vector2(pos.x + _dir * 0.5f, pos.y - 0.3f);
        var ground = Physics2D.Raycast(ahead, Vector2.down, 0.9f);

        if ((wall.collider != null) || (ground.collider == null))
            _dir = -_dir;

        transform.position += new Vector3(_dir * speed * Time.deltaTime, 0f, 0f);
        if (_sr != null) _sr.flipX = _dir < 0;
    }

    void OnTriggerEnter2D(Collider2D other) => Contact(other);
    void OnTriggerStay2D(Collider2D other) => Contact(other);

    void Contact(Collider2D other)
    {
        if (_dead) return;
        var p = other.GetComponent<PlayerController>();
        if (p == null) return;

        // Jogador caindo e acima do inimigo => pisão (derrota o inimigo)
        if (p.Velocity.y < 0.5f && p.transform.position.y > transform.position.y + 0.35f)
        {
            Die();
            p.Bounce();
        }
        else
        {
            p.TakeDamage(transform.position);
        }
    }

    void Die()
    {
        _dead = true;
        AudioManager.Instance.PlayStomp();
        GameManager.Instance.AddScore(150);
        // efeito de "esmagado"
        if (_sr != null) _sr.flipY = true;
        var c = GetComponent<Collider2D>();
        if (c != null) c.enabled = false;
        var anim = GetComponent<SpriteAnimator>();
        if (anim != null) anim.enabled = false;
        transform.localScale = new Vector3(transform.localScale.x, 0.4f, 1f);
        Destroy(gameObject, 0.4f);
    }
}
