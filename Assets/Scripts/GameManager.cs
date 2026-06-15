using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;            // Volume / VolumeProfile
using UnityEngine.Rendering.Universal;  // Bloom e o post-processing do URP

// esse aqui eh o "cerebro" do jogo. fiz como Singleton pra qualquer script
// conseguir falar com ele facil. cuida das vidas, pontos, cristais, troca de
// fase, game over e vitoria. quem cria ele eh o Bootstrap quando a cena abre.
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    enum State { Menu, Playing, Transition, GameOver, Win, Paused }
    State _state;

    // dificuldade escolhida la no menu inicial
    public float EnemySpeedMul = 1f;
    public int BossHp = 12;
    int _startLives = 3;
    bool _checkpoints = true;   // facil/normal salvam a fase; no dificil nao tem checkpoint
    int _checkpointLevel;       // pra qual fase eu volto no game over (quando tem checkpoint)

    int _lives = 3;
    int _score;
    int _crystals;       // cristais pegos na fase atual
    int _totalCrystals;  // total de cristais que a fase tem
    int _levelIndex;

    HUDController _hud;
    Camera _camera;
    CameraFollow _camFollow;
    GameObject _container;
    PlayerController _player;
    Vector3 _levelStart;
    SpriteRenderer _backdropSR;
    SpriteRenderer _nearSR;
    int _bossMax;

    public PlayerController Player => _player;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        // deixo as queries de fisica ignorarem os triggers. assim o check de chao
        // so pega bloco solido e eu nao preciso ficar criando tag/layer pra tudo.
        Physics2D.queriesHitTriggers = false;
    }

    public void Init()
    {
        gameObject.AddComponent<AudioManager>();

        var hudGO = new GameObject("HUD");
        hudGO.transform.SetParent(transform);
        _hud = hudGO.AddComponent<HUDController>();

        SetupCamera();
        AudioManager.Instance.StartMusic();

        // comeca sempre na tela de menu pra pessoa escolher a dificuldade
        _state = State.Menu;
        _hud.ShowMenu(true);
    }

    // tabela de dificuldade. mexo nas vidas, na velocidade dos inimigos e na vida do boss
    void ApplyDifficulty(int d)
    {
        switch (d)
        {
            case 0: _startLives = 5; EnemySpeedMul = 0.85f; BossHp = 8; break;   // facil
            case 2: _startLives = 2; EnemySpeedMul = 1.30f; BossHp = 18; break;  // dificil
            default: _startLives = 3; EnemySpeedMul = 1.0f; BossHp = 12; break;  // normal
        }
        _checkpoints = (d != 2); // so o dificil (2) que nao tem checkpoint
    }

    void BeginGame(int level)
    {
        _hud.ShowMenu(false);
        _lives = _startLives;
        _score = 0;
        _checkpointLevel = 0;
        LoadLevel(level);
    }

    void SetupCamera()
    {
        _camera = Camera.main;
        if (_camera == null) // por garantia, se nao tiver camera na cena eu crio uma
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
        _camera.transform.position = new Vector3(0, 0, -10);

        // liga o post-processing e monto um Bloom por codigo pra o neon "acender"
        var camData = _camera.GetUniversalAdditionalCameraData();
        camData.renderPostProcessing = true;
        var fxGO = new GameObject("PostFX");
        var vol = fxGO.AddComponent<Volume>();
        vol.isGlobal = true;
        var profile = ScriptableObject.CreateInstance<VolumeProfile>();
        vol.profile = profile;
        var bloom = profile.Add<Bloom>(true);
        bloom.intensity.value = 0.85f;  // brilho na medida, sem "lavar" a tela
        bloom.threshold.value = 0.9f;   // so o que e bem brilhante (neon) acende
        bloom.scatter.value = 0.6f;

        _camFollow = _camera.GetComponent<CameraFollow>();
        if (_camFollow == null) _camFollow = _camera.gameObject.AddComponent<CameraFollow>();

        // particulazinhas de ambiente flutuando na tela pra dar clima
        var amb = _camera.GetComponent<Ambience>();
        if (amb == null) amb = _camera.gameObject.AddComponent<Ambience>();
        amb.cam = _camera;

        // fundo da cidade preso na camera, com o SkyController fazendo o flicker neon
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

        // camada de cidade mais perto, com parallax (anda mais devagar que a camera = profundidade)
        var near = new GameObject("CityNear");
        near.transform.SetParent(_camera.transform);
        near.transform.localPosition = new Vector3(0, -2.5f, 19f);
        _nearSR = near.AddComponent<SpriteRenderer>();
        _nearSR.sprite = SpriteFactory.CityNear(0);
        _nearSR.sortingOrder = -60;
        float nh = _camera.orthographicSize * 2f * 0.9f;
        float nw = nh * _camera.aspect * 2.6f; // bem largo pra a borda nunca aparecer com o parallax
        var nearSprite = _nearSR.sprite;
        near.transform.localScale = new Vector3(nw / nearSprite.bounds.size.x, nh / nearSprite.bounds.size.y, 1f);
        var par = near.AddComponent<Parallax>();
        par.cam = _camera.transform;
        par.factor = 0.12f;
    }

    // monta uma fase: apaga a anterior, constroi a nova e atualiza o HUD
    void LoadLevel(int index)
    {
        _levelIndex = index;
        _checkpointLevel = index; // passou de fase = checkpoint novo
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

        // cada fase tem um ceu de cor diferente (fundo + a camada de parallax)
        if (_backdropSR != null) _backdropSR.sprite = SpriteFactory.Background(index);
        if (_nearSR != null) _nearSR.sprite = SpriteFactory.CityNear(index);

        _hud.SetLives(_lives);
        _hud.SetScore(_score);
        _hud.SetCrystals(_crystals, _totalCrystals);
        _hud.SetAmmo(_player.HasWeapon, _player.Ammo);
        _hud.ShowBoss(false);
        _hud.HideCenter();
        _hud.ShowBanner("FASE " + LevelData.Names[index]);
        // so mostro a dica de controles na primeira fase pra nao poluir o resto
        _hud.SetHint(index == 0
            ? "Setas/WASD: mover  |  Shift: correr  |  Espaco/W: pular  |  Cima/Baixo: escalar  |  J: atirar"
            : "");
    }

    // ---- coisas que os outros scripts chamam durante o jogo ----
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

    // tremor de tela. qualquer script pode chamar (dano, pisao, tiro no boss...)
    public void Shake(float amount) { if (_camFollow != null) _camFollow.Shake(amount); }

    public void GainLife()
    {
        if (_lives < 5) _lives++; // nao deixo passar de 5 (a barra so tem 5)
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
        _state = State.Transition; // trava aqui pra nao tomar dano de tiro perdido no fim
        Shake(0.7f);
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
        Shake(0.35f);

        if (_lives <= 0)
            GameOver();
        else if (fellInPit) // se caiu no buraco volta pro inicio da fase
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
        _score += 500 + _crystals * 25; // bonus por terminar (vale mais se pegou cristal)
        _hud.SetScore(_score);
        _player.SetControl(false);
        AudioManager.Instance.PlayLevelComplete();

        // se for a ultima fase ganhou o jogo, senao vai pra proxima
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
        // se tem checkpoint e nao ta na fase 1, avisa que volta pra fase do checkpoint
        string sub = (_checkpoints && _checkpointLevel > 0)
            ? "Pressione R para voltar pra Fase " + (_checkpointLevel + 1)
            : "Pressione R para recomecar";
        _hud.ShowCenter("GAME OVER", sub, new Color(1f, 0.4f, 0.4f));
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

    // game over no facil/normal: volta pra fase do checkpoint, mantendo os pontos
    void RestartFromCheckpoint()
    {
        _lives = _startLives;
        LoadLevel(_checkpointLevel);
    }

    // recomeco do zero (dificil, ou quando ganha e quer jogar de novo)
    void RestartFromStart()
    {
        _lives = _startLives;
        _score = 0;
        _checkpointLevel = 0;
        LoadLevel(0);
    }

    void Update()
    {
        // no menu, espero a pessoa apertar 1, 2 ou 3 pra escolher a dificuldade
        if (_state == State.Menu)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) { ApplyDifficulty(0); BeginGame(0); }
            else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) { ApplyDifficulty(1); BeginGame(0); }
            else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3)) { ApplyDifficulty(2); BeginGame(0); }
            return;
        }

        // tela de pausa
        if (_state == State.Paused)
        {
            if (Input.GetKeyDown(KeyCode.Escape)) ResumeGame();
            else if (Input.GetKeyDown(KeyCode.R)) { ResumeGame(); RestartFromCheckpoint(); }
            else if (Input.GetKeyDown(KeyCode.M)) GoToMenu();
            return;
        }

        // Esc pausa durante o jogo
        if (_state == State.Playing && Input.GetKeyDown(KeyCode.Escape)) { PauseGame(); return; }

        // R reinicia: no game over volta pro checkpoint (se tiver), na vitoria comeca tudo de novo
        if (_state == State.GameOver && Input.GetKeyDown(KeyCode.R))
        {
            if (_checkpoints) RestartFromCheckpoint(); else RestartFromStart();
        }
        else if (_state == State.Win && Input.GetKeyDown(KeyCode.R))
            RestartFromStart();

        if (Input.GetKeyDown(KeyCode.M))
            AudioManager.Instance.ToggleMute(); // M liga/desliga o som
    }

    void PauseGame()
    {
        _state = State.Paused;
        Time.timeScale = 0f; // congela tudo
        _hud.ShowPause(true);
    }

    void ResumeGame()
    {
        _state = State.Playing;
        Time.timeScale = 1f;
        _hud.ShowPause(false);
    }

    void GoToMenu()
    {
        Time.timeScale = 1f;
        _hud.ShowPause(false);
        _hud.HideCenter();
        _hud.ShowBoss(false);
        if (_container != null) Destroy(_container);
        _state = State.Menu;
        _hud.ShowMenu(true);
    }
}
