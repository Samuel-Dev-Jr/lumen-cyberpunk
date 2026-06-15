using UnityEngine;

// torreta parada que atira no player de tempo em tempo. machuca no contato,
// mas da pra destruir com tiro ou pulando em cima.
public class Turret : MonoBehaviour
{
    public float interval = 1.6f;
    float _timer;
    bool _dead;
    SpriteRenderer _sr;

    void Start()
    {
        _sr = GetComponent<SpriteRenderer>();
        _timer = Random.Range(0.5f, interval); // pra elas nao atirarem todas juntas
    }

    void Update()
    {
        if (_dead) return;
        _timer -= Time.deltaTime;
        if (_timer <= 0f) { _timer = interval; Shoot(); }
    }

    void Shoot()
    {
        var p = GameManager.Instance.Player;
        if (p == null) return;
        Vector2 dir = (p.transform.position - transform.position).normalized;
        if (_sr != null) _sr.flipX = dir.x < 0; // vira o cano pro lado do player
        Projectile.Spawn(transform.position + (Vector3)dir * 0.5f, dir, 7f, false, transform.parent);
    }

    // chamado pelo tiro do player
    public void Hit()
    {
        if (_dead) return;
        _dead = true;
        AudioManager.Instance.PlayStomp();
        GameManager.Instance.AddScore(150);
        FloatingText.Spawn(transform.position + Vector3.up * 0.5f, "+150", new Color(1f, 0.9f, 0.4f), transform.parent);
        GameManager.Instance.Shake(0.15f);
        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other) => Touch(other);
    void OnTriggerStay2D(Collider2D other) => Touch(other);

    void Touch(Collider2D other)
    {
        if (_dead) return;
        var p = other.GetComponent<PlayerController>();
        if (p == null) return;
        // se pular em cima dela tbm destroi (igual nos inimigos)
        if (p.Velocity.y < 0.5f && p.transform.position.y > transform.position.y + 0.35f)
        {
            Hit(); p.Bounce();
        }
        else p.TakeDamage(transform.position);
    }
}
