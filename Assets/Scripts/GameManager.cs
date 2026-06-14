using System.Collections;
using UnityEngine;

/// <summary>
/// Cérebro do jogo (padrão Singleton). Controla o game loop de alto nível:
/// vidas, pontuação, cristais, construção/limpeza das fases, transições,
/// derrota (Game Over) e vitória. É criado em tempo de execução pelo Bootstrap.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    enum State { Playing, Transition, GameOver, Win }
    State _state;

    int _lives = 3;
    int _score;
    int _crystals;       // coletados na fase atual
    int _totalCrystals;  // total da fase atual
    int _levelIndex;

    HUDController _hud;
    Camera _camera;
    CameraFollow _camFollow;
    GameObject _container;
    PlayerController _player;
    Vector3 _levelStart;
    SpriteRenderer _backdropSR;
    int _bossMax;

    public PlayerController Player => _player;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        // Consultas de física (Raycast/Overlap) ignoram triggers -> simplifica
        // a detecção de chão (só pega blocos sólidos) sem precisar de tags/layers.
        Physics2D.queriesHitTriggers = false;
    }

    public void Init()
    {
        // Áudio
        gameObject.AddComponent<AudioManager>();

        // HUD
        var hudGO = new GameObject("HUD");
        hudGO.transform.SetParent(transform);
        _hud = hudGO.AddComponent<HUDController>();

        // Câmera
        SetupCamera();

        AudioManager.Instance.StartMusic();
        LoadLevel(StartLevel());
    }

    // permite iniciar numa fase especifica via linha de comando: -startlevel N (debug)
    int StartLevel()
    {
        var args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++)
            if (args[i] == "-startlevel" && int.TryParse(args[i + 1], out int n))
                return Mathf.Clamp(n, 0, LevelData.Maps.Length - 1);
        return 0;
    }

    void SetupCamera()
    {
        _camera = Camera.main;
        if (_camera == null)
        {
            var camGO = new GameObject("Main Camera");
            camGO.tag = "MainCamera";
            _camera = camGO.AddComponent<Camera>();
            camGO.AddComponent<AudioListener>();
        }
        _camera.orthographic = true;
        _camera.orthographicSize = 6.5f;
        _camera.clearFlags = CameraClearFlags.SolidColor;
        _camera.backgroundColor = new Color32(12, 14, 26, 255);
        var t = _camera.transform;
        t.position = new Vector3(0, 0, -10);

        _camFollow = _camera.GetComponent<CameraFollow>();
        if (_camFollow == null) _camFollow = _camera.gameObject.AddComponent<CameraFollow>();

        // Fundo (segue a câmera) com flicker neon
        var bg = new GameObject("Backdrop");
        bg.transform.SetParent(_camera.transform);
        bg.transform.localPosition = new Vector3(0, 0, 20);
        _backdropSR = bg.AddComponent<SpriteRenderer>();
        _backdropSR.sprite = SpriteFactory.Background(0);
        _backdropSR.sortingOrder = -100;
        bg.AddComponent<SkyController>();
        float h = _camera.orthographicSize * 2f * 1.25f;
        float w = h * _camera.aspect;
        var bgSprite = _backdropSR.sprite;
        bg.transform.localScale = new Vector3(w / bgSprite.bounds.size.x, h / bgSprite.bounds.size.y, 1f);
    }

    // ===================== FASES =====================
    void LoadLevel(int index)
    {
        _levelIndex = index;
        _state = State.Playing;

        if (_container != null) Destroy(_container);
        _container = new GameObject("Level");

        var info = LevelBuilder.Build(LevelData.Maps[index], _container.transform);
        _player = info.player.GetComponent<PlayerController>();
        _levelStart = info.playerStart;
        _crystals = 0;
        _totalCrystals = info.crystals;

        _camFollow.target = _player.transform;
        _camFollow.SetBounds(info.minX, info.maxX, info.minY, info.maxY);
        _camFollow.SnapToTarget();

        // céu/cidade muda de cor por fase
        if (_backdropSR != null) _backdropSR.sprite = SpriteFactory.Background(index);

        _hud.SetLives(_lives);
        _hud.SetScore(_score);
        _hud.SetCrystals(_crystals, _totalCrystals);
        _hud.SetAmmo(_player.HasWeapon, _player.Ammo);
        _hud.ShowBoss(false);
        _hud.HideCenter();
        _hud.ShowBanner("Fase " + (index + 1) + " — " + LevelData.Names[index]);
        _hud.SetHint(index == 0
            ? "Setas/A-D: mover  |  Shift: correr  |  Espaco: pular  |  Cima/Baixo: escalar  |  J: atirar"
            : "");
    }

    // ===================== EVENTOS DE JOGO =====================
    public void CollectCrystal(int value)
    {
        _score += value;
        _crystals++;
        _hud.SetScore(_score);
        _hud.SetCrystals(_crystals, _totalCrystals);
    }

    public void AddScore(int value)
    {
        _score += value;
        _hud.SetScore(_score);
    }

    public void OnAmmoChanged(bool hasWeapon, int ammo) => _hud.SetAmmo(hasWeapon, ammo);

    public void GainLife()
    {
        if (_lives < 5) _lives++;
        _hud.SetLives(_lives);
    }

    public void RegisterBoss(int maxHp)
    {
        _bossMax = maxHp;
        _hud.ShowBoss(true);
        _hud.SetBossHealth(maxHp, maxHp);
    }

    public void UpdateBossHealth(int cur) => _hud.SetBossHealth(cur, _bossMax);

    public void BossDefeated()
    {
        if (_state != State.Playing) return;
        _state = State.Transition;
        _score += 2000;
        _hud.SetScore(_score);
        _hud.ShowBoss(false);
        if (_player != null) _player.SetControl(false);
        StartCoroutine(WinAfter(1.4f));
    }

    public void DamagePlayer(bool fellInPit)
    {
        if (_state != State.Playing) return;
        _lives--;
        _hud.SetLives(Mathf.Max(0, _lives));
        AudioManager.Instance.PlayHurt();

        if (_lives <= 0)
        {
            GameOver();
        }
        else if (fellInPit)
        {
            _player.RespawnAt(_levelStart);
            _camFollow.SnapToTarget();
        }
    }

    public void PlayerFell() => DamagePlayer(true);

    public void ReachExit()
    {
        if (_state != State.Playing) return;
        _state = State.Transition;
        _score += 500 + _crystals * 25; // bônus de conclusão
        _hud.SetScore(_score);
        _player.SetControl(false);
        AudioManager.Instance.PlayLevelComplete();

        if (_levelIndex + 1 >= LevelData.Maps.Length)
            StartCoroutine(WinAfter(1.2f));
        else
            StartCoroutine(NextAfter(1.2f));
    }

    IEnumerator NextAfter(float t)
    {
        _hud.ShowCenter("Fase Concluida!", "", new Color(0.6f, 1f, 0.7f));
        yield return new WaitForSeconds(t);
        LoadLevel(_levelIndex + 1);
    }

    IEnumerator WinAfter(float t)
    {
        yield return new WaitForSeconds(t);
        WinGame();
    }

    void GameOver()
    {
        _state = State.GameOver;
        _player.SetControl(false);
        AudioManager.Instance.PlayGameOver();
        _hud.ShowCenter("GAME OVER", "Pressione R para recomecar", new Color(1f, 0.4f, 0.4f));
        _hud.SetHint("");
    }

    void WinGame()
    {
        _state = State.Win;
        if (_player != null) _player.SetControl(false);
        AudioManager.Instance.PlayWin();
        _hud.ShowCenter("VOCE VENCEU!", "Pontuacao final: " + _score + "   |   Pressione R para jogar de novo", new Color(1f, 0.9f, 0.4f));
        _hud.SetHint("");
    }

    void RestartGame()
    {
        _lives = 3;
        _score = 0;
        LoadLevel(0);
    }

    void Update()
    {
        if ((_state == State.GameOver || _state == State.Win) && Input.GetKeyDown(KeyCode.R))
            RestartGame();

        if (Input.GetKeyDown(KeyCode.M))
            AudioManager.Instance.ToggleMute();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
#if !UNITY_EDITOR
            Application.Quit();
#endif
        }
    }
}
