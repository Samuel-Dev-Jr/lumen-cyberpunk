using UnityEngine;

// script que controla a Luna (o player). fiz aqui o andar, correr (shift), pular (espaço) e escalar escada.
// tem umas coisinhas pra deixar o pulo gostoso tipo coyote time e jump buffer.
[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimento")]
    public float walkSpeed = 6.5f;
    public float runSpeed = 9.5f;
    public float jumpForce = 16f;
    public float climbSpeed = 4.5f;
    public float gravityScale = 3.8f;
    public float fallMultiplier = 2.2f;   // cai mais rapido, fica menos flutuante
    public float lowJumpMultiplier = 2.5f; // se soltar o botao o pulo fica baixinho

    [Header("Tolerâncias")]
    public float coyoteTime = 0.10f;   // coyote time, da uns ms pra pular dps de sair da plataforma
    public float jumpBuffer = 0.12f;   // se apertar pular um pouco antes do chao, ainda vale

    public float killY = -50f; // se cair abaixo disso morreu (caiu no buraco)

    [Header("Arma")]
    public float fireCooldown = 0.28f;
    public float bulletSpeed = 13f;
    bool _hasWeapon;
    int _ammo;
    float _fireTimer, _shootAnimTimer;
    public bool HasWeapon => _hasWeapon;
    public int Ammo => _ammo;

    // variaveis de controle interno
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

    // o sprite fica num objeto filho pra eu poder usar a arte da Luna
    Transform _visual;
    bool _useCustom;
    float _visBaseY;
    Vector3 _visBaseScale = Vector3.one;

    public Vector2 Velocity => _rb.linearVelocity;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<BoxCollider2D>();
        _sr = GetComponentInChildren<SpriteRenderer>();
        _anim = GetComponentInChildren<SpriteAnimator>();

        SetupVisual();

        _rb.gravityScale = gravityScale;
        _rb.freezeRotation = true;
        _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // material sem atrito senao a Luna gruda na parede quando encosta, ficava travando
        var noFriction = new PhysicsMaterial2D("NoFriction") { friction = 0f, bounciness = 0f };
        _col.sharedMaterial = noFriction;
        _rb.sharedMaterial = noFriction;
    }

    void Update()
    {
        if (_invuln > 0f)
        {
            _invuln -= Time.deltaTime;
            // fica piscando enquanto ta invencivel pra mostrar que tomou dano
            float a = Mathf.PingPong(Time.time * 12f, 1f) > 0.5f ? 0.35f : 1f;
            var c = _sr.color; c.a = a; _sr.color = c;
            if (_invuln <= 0f) { var cc = _sr.color; cc.a = 1f; _sr.color = cc; }
        }

        if (!_control) { _h = _v = 0f; UpdateAnimation(); return; }

        // leitura das teclas, deixei funcionar com WASD e com as setas tbm
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

        // atirar: pode ser no J ou no botao esquerdo do mouse
        if (_fireTimer > 0f) _fireTimer -= Time.deltaTime;
        if (_shootAnimTimer > 0f) _shootAnimTimer -= Time.deltaTime;
        if ((Input.GetKey(KeyCode.J) || Input.GetMouseButton(0)) && _hasWeapon && _ammo > 0 && _fireTimer <= 0f)
            Shoot();

        // se ta em cima de uma escada e aperta pra cima/baixo, comeca a escalar
        if (_ladderCount > 0 && Mathf.Abs(_v) > 0.1f) _climbing = true;
        if (_ladderCount == 0) _climbing = false;

        // vira o sprite pro lado que ta andando
        if (_h > 0.1f && !_facingRight) Flip();
        else if (_h < -0.1f && _facingRight) Flip();

        // checa se caiu no buraco
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
            // se apertar pra pular enquanto escala, ela larga a escada e pula
            if (_bufferTimer > 0f)
            {
                _climbing = false;
                _rb.gravityScale = gravityScale;
                DoJump();
            }
            return;
        }

        _rb.gravityScale = gravityScale;

        // anda na horizontal, se tiver com shift corre
        float speed = _runHeld ? runSpeed : walkSpeed;
        _rb.linearVelocity = new Vector2(_h * speed, _rb.linearVelocity.y);

        // conta o coyote time
        if (_grounded) _coyoteTimer = coyoteTime;
        else if (_coyoteTimer > 0f) _coyoteTimer -= Time.fixedDeltaTime;

        // so pula se tiver buffer e ainda dentro do coyote
        if (_bufferTimer > 0f && _coyoteTimer > 0f)
            DoJump();

        // essa parte aumenta a gravidade pra cair mais rapido e o pulo nao ficar boiando
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
        // como o queriesHitTriggers ta desligado isso aqui so pega coisa solida, ignora trigger
        var hits = Physics2D.OverlapBoxAll(center, size, 0f);
        _grounded = false;
        foreach (var h in hits)
        {
            if (h.gameObject == gameObject) continue;
            _grounded = true;
            break;
        }
    }

    void SetupVisual()
    {
        if (_sr == null) return;
        _visual = _sr.transform;
        var luna = Resources.Load<Sprite>("luna");
        if (luna != null)
        {
            _useCustom = true;
            if (_anim != null) _anim.enabled = false;
            _sr.sprite = luna;
            if (luna.texture != null) luna.texture.filterMode = FilterMode.Point; // point pra nao borrar o pixel art
            float target = 1.5f; // altura que eu quero que ela tenha
            float s = target / luna.bounds.size.y;
            _visBaseScale = new Vector3(s, s, 1f);
            float colBottom = _col.offset.y - _col.size.y / 2f;
            _visBaseY = colBottom + target / 2f; // calculo pra deixar o pe dela encostando no chao
            _visual.localScale = _visBaseScale;
            _visual.localPosition = new Vector3(0f, _visBaseY, 0f);
        }
    }

    // como a Luna nao tem varios frames, faco a animacao mexendo no transform mesmo (sobe/desce e estica)
    void CustomVisual()
    {
        float bob = 0f, stretchY = 1f, squashX = 1f;
        if (_climbing) bob = Mathf.Sin(Time.time * 8f) * 0.05f;
        else if (!_grounded) { stretchY = 1.08f; squashX = 0.94f; }
        else if (Mathf.Abs(_rb.linearVelocity.x) > 0.3f) bob = Mathf.Abs(Mathf.Sin(Time.time * 12f)) * 0.07f;
        else bob = Mathf.Sin(Time.time * 3f) * 0.03f;
        _visual.localPosition = new Vector3(0f, _visBaseY + bob, 0f);
        _visual.localScale = new Vector3(_visBaseScale.x * squashX, _visBaseScale.y * stretchY, 1f);
    }

    void UpdateAnimation()
    {
        if (_sr == null) return;
        if (_useCustom) { CustomVisual(); return; }
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
        else if (_shootAnimTimer > 0f)
        {
            _anim.Play(SpriteFactory.PlayerShoot(), 1f, false);
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

    // conta em quantas escadas a Luna ta encostando (pode ser mais de uma colada)
    void OnTriggerEnter2D(Collider2D other) { if (other.GetComponent<Ladder>() != null) _ladderCount++; }
    void OnTriggerExit2D(Collider2D other) { if (other.GetComponent<Ladder>() != null) _ladderCount = Mathf.Max(0, _ladderCount - 1); }

    // quando toma dano
    public void TakeDamage(Vector2 source)
    {
        if (_invuln > 0f || !_control) return;

        GameManager.Instance.DamagePlayer(false);
        if (!_control) return; // se morreu nao adianta empurrar

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

    void Shoot()
    {
        Vector2 dir = _facingRight ? Vector2.right : Vector2.left;
        Vector3 pos = transform.position + (Vector3)(dir * 0.6f) + Vector3.up * 0.1f;
        Projectile.Spawn(pos, dir, bulletSpeed, true, transform.parent);
        _ammo--;
        _fireTimer = fireCooldown;
        _shootAnimTimer = 0.18f;
        AudioManager.Instance.PlayShoot();
        GameManager.Instance.OnAmmoChanged(_hasWeapon, _ammo);
    }

    public void GiveWeapon(int amount)
    {
        _hasWeapon = true;
        _ammo += amount;
        GameManager.Instance.OnAmmoChanged(_hasWeapon, _ammo);
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
