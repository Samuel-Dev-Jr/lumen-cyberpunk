using UnityEngine;

// inimigo do jogo. tem dois tipos: o que anda no chao e o drone que voa.
// o do chao vira quando bate na parede ou chega na beirada pra nao cair.
// da pra pisar ou atirar neles, e as vezes cai uma arma quando morrem.
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
        _dir = (Mathf.FloorToInt(transform.position.x) % 2 == 0) ? 1f : -1f; // truque pra uns comecarem indo pra cada lado
        if (GameManager.Instance != null) speed *= GameManager.Instance.EnemySpeedMul; // na dificuldade maior eles ficam mais rapidos
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
        var wall = Physics2D.Raycast(pos, Vector2.right * _dir, 0.55f); // ve se tem parede na frente
        var ahead = new Vector2(pos.x + _dir * 0.5f, pos.y - 0.3f);
        var ground = Physics2D.Raycast(ahead, Vector2.down, 0.9f); // ve se ainda tem chao um pouco a frente
        if (wall.collider != null || ground.collider == null) _dir = -_dir; // se bateu na parede ou ia cair, da meia volta
        transform.position += new Vector3(_dir * speed * Time.deltaTime, 0f, 0f);
    }

    void FlyMove()
    {
        var wall = Physics2D.Raycast(transform.position, Vector2.right * _dir, 0.6f);
        if (wall.collider != null || transform.position.x > _spawnX + 5f || transform.position.x < _spawnX - 5f)
            _dir = -_dir;
        var pos = transform.position;
        pos.x += _dir * speed * Time.deltaTime;
        pos.y = _baseY + Mathf.Sin(Time.time * 2.5f + _spawnX) * 0.6f; // sobe e desce com o sin pra parecer que ta flutuando
        transform.position = pos;
    }

    void OnTriggerEnter2D(Collider2D other) => Contact(other);
    void OnTriggerStay2D(Collider2D other) => Contact(other);

    void Contact(Collider2D other)
    {
        if (_dead) return;
        var p = other.GetComponent<PlayerController>();
        if (p == null) return;
        // se cair em cima de um inimigo de chao, mata ele e a Luna quica. nos voadores nao da pra pisar
        if (!flying && p.Velocity.y < 0.5f && p.transform.position.y > transform.position.y + 0.35f)
        {
            Die(); p.Bounce();
        }
        else p.TakeDamage(transform.position);
    }

    // chamado quando um tiro do player acerta o inimigo
    public void Hit() { if (!_dead) Die(); }

    void Die()
    {
        _dead = true;
        AudioManager.Instance.PlayStomp();
        GameManager.Instance.AddScore(150);
        FloatingText.Spawn(transform.position + Vector3.up * 0.5f, "+150", new Color(1f, 0.9f, 0.4f), transform.parent);
        GameManager.Instance.Shake(0.15f);

        // uns 40% de chance de soltar uma arma quando morre
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

        // efeitinho de morte: vira de cabeca pra baixo, achata e some depois de um tempinho
        if (_sr != null) _sr.flipY = true;
        var c = GetComponent<Collider2D>(); if (c != null) c.enabled = false; // desliga a colisao pra nao dar dano morto
        var anim = GetComponent<SpriteAnimator>(); if (anim != null) anim.enabled = false;
        transform.localScale = new Vector3(transform.localScale.x, 0.4f, 1f);
        Destroy(gameObject, 0.4f);
    }
}
