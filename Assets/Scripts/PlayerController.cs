using UnityEngine;

/// <summary>
/// Controle do personagem "Lumen". Implementa as mecânicas exigidas:
/// ANDAR, CORRER (Shift), PULAR (Espaço) e ESCALAR (em escadas).
/// Inclui sensação de jogo refinada: coyote time, jump buffer, pulo de altura
/// variável e gravidade aumentada na queda. As animações são trocadas conforme
/// o estado (parado, andando, correndo, pulando, caindo, escalando).
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(SpriteRenderer))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimento")]
    public float walkSpeed = 6.5f;
    public float runSpeed = 9.5f;
    public float jumpForce = 16f;
    public float climbSpeed = 4.5f;
    public float gravityScale = 3.8f;
    public float fallMultiplier = 2.2f;   // cai mais rápido (sensação melhor)
    public float lowJumpMultiplier = 2.5f; // pulo curto ao soltar o botão

    [Header("Tolerâncias")]
    public float coyoteTime = 0.10f;   // pode pular um instante após sair da borda
    public float jumpBuffer = 0.12f;   // registra o pulo um instante antes de tocar o chão

    public float killY = -50f; // abaixo disso, caiu no abismo

    // ----- estado interno -----
    Rigidbody2D _rb;
    BoxCollider2D _col;
    SpriteRenderer _sr;
    SpriteAnimator _anim;

    float _h, _v;
    bool _runHeld, _jumpHeld;
    float _coyoteTimer, _bufferTimer;
    bool _grounded, _climbing;
    int _ladderCount;
    bool _facingRight = true;
    float _invuln;
    bool _control = true;
    bool _fell;

    public Vector2 Velocity => _rb.linearVelocity;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<BoxCollider2D>();
        _sr = GetComponent<SpriteRenderer>();
        _anim = GetComponent<SpriteAnimator>();

        _rb.gravityScale = gravityScale;
        _rb.freezeRotation = true;
        _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // material sem atrito: impede o personagem de "grudar" em paredes/emendas
        var noFriction = new PhysicsMaterial2D("NoFriction") { friction = 0f, bounciness = 0f };
        _col.sharedMaterial = noFriction;
        _rb.sharedMaterial = noFriction;
    }

    void Update()
    {
        if (_invuln > 0f)
        {
            _invuln -= Time.deltaTime;
            // pisca durante a invulnerabilidade
            float a = Mathf.PingPong(Time.time * 12f, 1f) > 0.5f ? 0.35f : 1f;
            var c = _sr.color; c.a = a; _sr.color = c;
            if (_invuln <= 0f) { var cc = _sr.color; cc.a = 1f; _sr.color = cc; }
        }

        if (!_control) { _h = _v = 0f; UpdateAnimation(); return; }

        // ----- input (teclado legado: setas + WASD) -----
        _h = 0f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) _h -= 1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) _h += 1f;
        _v = 0f;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) _v += 1f;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) _v -= 1f;

        _runHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        if (Input.GetKeyDown(KeyCode.Space)) _bufferTimer = jumpBuffer;
        _jumpHeld = Input.GetKey(KeyCode.Space);

        if (_bufferTimer > 0f) _bufferTimer -= Time.deltaTime;

        // entra no modo escalada ao pressionar cima/baixo sobre uma escada
        if (_ladderCount > 0 && Mathf.Abs(_v) > 0.1f) _climbing = true;
        if (_ladderCount == 0) _climbing = false;

        // vira o sprite conforme a direção
        if (_h > 0.1f && !_facingRight) Flip();
        else if (_h < -0.1f && _facingRight) Flip();

        // caiu no abismo?
        if (!_fell && transform.position.y < killY)
        {
            _fell = true;
            GameManager.Instance.PlayerFell();
        }

        UpdateAnimation();
    }

    void FixedUpdate()
    {
        if (!_control)
        {
            _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
            return;
        }

        GroundCheck();

        if (_climbing)
        {
            _rb.gravityScale = 0f;
            _rb.linearVelocity = new Vector2(_h * walkSpeed * 0.7f, _v * climbSpeed);
            // pular sai da escada
            if (_bufferTimer > 0f)
            {
                _climbing = false;
                _rb.gravityScale = gravityScale;
                DoJump();
            }
            return;
        }

        _rb.gravityScale = gravityScale;

        // movimento horizontal (corrida com Shift)
        float speed = _runHeld ? runSpeed : walkSpeed;
        _rb.linearVelocity = new Vector2(_h * speed, _rb.linearVelocity.y);

        // coyote time
        if (_grounded) _coyoteTimer = coyoteTime;
        else if (_coyoteTimer > 0f) _coyoteTimer -= Time.fixedDeltaTime;

        // pulo (com buffer + coyote)
        if (_bufferTimer > 0f && _coyoteTimer > 0f)
            DoJump();

        // gravidade extra para um pulo/queda com melhor sensação
        if (_rb.linearVelocity.y < 0f)
            _rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1f) * Time.fixedDeltaTime;
        else if (_rb.linearVelocity.y > 0f && !_jumpHeld)
            _rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1f) * Time.fixedDeltaTime;
    }

    void DoJump()
    {
        _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, jumpForce);
        _bufferTimer = 0f;
        _coyoteTimer = 0f;
        _grounded = false;
        AudioManager.Instance.PlayJump();
    }

    void GroundCheck()
    {
        Bounds b = _col.bounds;
        Vector2 center = new Vector2(b.center.x, b.min.y - 0.02f);
        Vector2 size = new Vector2(b.size.x * 0.85f, 0.12f);
        // queriesHitTriggers está desligado globalmente, então só pega colisores sólidos
        var hits = Physics2D.OverlapBoxAll(center, size, 0f);
        _grounded = false;
        foreach (var h in hits)
        {
            if (h.gameObject == gameObject) continue;
            _grounded = true;
            break;
        }
    }

    void UpdateAnimation()
    {
        if (_anim == null) return;

        if (_climbing)
        {
            _anim.Play(SpriteFactory.PlayerClimb(), Mathf.Abs(_v) > 0.1f ? 6f : 0f, true);
        }
        else if (!_grounded)
        {
            if (_rb.linearVelocity.y > 0.5f) _anim.Play(SpriteFactory.PlayerJump(), 1f, false);
            else _anim.Play(SpriteFactory.PlayerFall(), 1f, false);
        }
        else if (Mathf.Abs(_rb.linearVelocity.x) > 0.3f)
        {
            bool running = _runHeld && Mathf.Abs(_rb.linearVelocity.x) > walkSpeed + 0.5f;
            _anim.Play(SpriteFactory.PlayerWalk(), running ? 14f : 9f, true);
        }
        else
        {
            _anim.Play(SpriteFactory.PlayerIdle(), 3f, true);
        }
    }

    void Flip()
    {
        _facingRight = !_facingRight;
        _sr.flipX = !_facingRight;
    }

    // ----- escadas -----
    void OnTriggerEnter2D(Collider2D other) { if (other.GetComponent<Ladder>() != null) _ladderCount++; }
    void OnTriggerExit2D(Collider2D other) { if (other.GetComponent<Ladder>() != null) _ladderCount = Mathf.Max(0, _ladderCount - 1); }

    // ----- dano / vida -----
    public void TakeDamage(Vector2 source)
    {
        if (_invuln > 0f || !_control) return;

        GameManager.Instance.DamagePlayer(false);
        if (!_control) return; // morreu (game over) -> não aplica knockback

        _invuln = 1.1f;
        float dir = Mathf.Sign(transform.position.x - source.x);
        if (dir == 0f) dir = _facingRight ? -1f : 1f;
        _climbing = false;
        _rb.gravityScale = gravityScale;
        _rb.linearVelocity = new Vector2(dir * 6f, 8f);
    }

    public void Bounce()
    {
        _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, jumpForce * 0.75f);
    }

    public void RespawnAt(Vector3 pos)
    {
        _fell = false;
        _climbing = false;
        _ladderCount = 0;
        _invuln = 1.0f;
        _rb.linearVelocity = Vector2.zero;
        _rb.gravityScale = gravityScale;
        transform.position = pos;
    }

    public void SetControl(bool on)
    {
        _control = on;
        if (!on) _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
    }
}
