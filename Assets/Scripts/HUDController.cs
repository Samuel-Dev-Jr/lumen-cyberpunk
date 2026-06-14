using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// faço toda a HUD aqui na mão (sem prefab), com aquele estilo neon cyberpunk.
// tem a vida em segmentos, cristais, pontos, a munição, a barra do chefe e as
// mensagens do meio da tela (fase, game over, vitoria).
public class HUDController : MonoBehaviour
{
    const int MAX_LIVES = 5;
    static readonly Color NEON_MAG = new Color(1f, 0.24f, 0.78f);
    static readonly Color NEON_CYAN = new Color(0.16f, 0.86f, 1f);
    static readonly Color DIM = new Color(0.16f, 0.17f, 0.27f);

    Text _score, _crystals, _banner, _center, _centerSub, _hint, _ammo, _bossLabel;
    Image[] _lifeSegs;
    Image _weaponIcon, _bossBarFill;
    GameObject _bossBar, _ammoGroup, _menu;
    Font _font;
    Transform _root;
    Coroutine _bannerCo;

    void Awake()
    {
        _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        Build();
    }

    void Build()
    {
        var canvasGO = new GameObject("HUD Canvas");
        canvasGO.transform.SetParent(transform);
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();
        _root = canvasGO.transform;

        // vida: o coracao + os pedacinhos da barra
        MakeIcon(SpriteFactory.Heart(), new Vector2(0, 1), new Vector2(24, -22), 26);
        _lifeSegs = new Image[MAX_LIVES];
        for (int i = 0; i < MAX_LIVES; i++)
            _lifeSegs[i] = MakeBar(new Vector2(0, 1), new Vector2(56 + i * 40, -22), new Vector2(34, 16), DIM);

        // cristais
        MakeIcon(SpriteFactory.CrystalIcon(), new Vector2(0, 1), new Vector2(24, -56), 26);
        _crystals = MakeText("crystals", new Vector2(0, 1), new Vector2(52, -52), new Vector2(220, 30), 24, TextAnchor.MiddleLeft);

        // municao: so aparece depois que o player pega a arma
        _ammoGroup = MakeGroup("ammoGroup");
        _weaponIcon = MakeIcon(SpriteFactory.WeaponIcon(), new Vector2(0, 1), new Vector2(24, -90), 26, _ammoGroup.transform);
        _ammo = MakeText("ammo", new Vector2(0, 1), new Vector2(52, -86), new Vector2(180, 30), 24, TextAnchor.MiddleLeft, _ammoGroup.transform);
        _ammoGroup.SetActive(false);

        // pontuacao (canto direito)
        _score = MakeText("score", new Vector2(1, 1), new Vector2(-20, -22), new Vector2(300, 32), 26, TextAnchor.MiddleRight);

        // barra do chefe la em cima no meio
        _bossBar = MakeGroup("bossBar");
        var bgImg = MakeBar(new Vector2(0.5f, 1), new Vector2(0, -50), new Vector2(440, 22), DIM, _bossBar.transform);
        bgImg.rectTransform.pivot = new Vector2(0.5f, 1);
        _bossBarFill = MakeBar(new Vector2(0.5f, 1), new Vector2(0, -50), new Vector2(436, 18), new Color(1f, 0.3f, 0.35f), _bossBar.transform);
        _bossBarFill.rectTransform.pivot = new Vector2(0f, 1f);
        _bossBarFill.rectTransform.anchoredPosition = new Vector2(-218, -52);
        _bossLabel = MakeText("bossLabel", new Vector2(0.5f, 1), new Vector2(0, -28), new Vector2(440, 24), 18, TextAnchor.MiddleCenter, _bossBar.transform);
        _bossLabel.text = "MAINFRAME";
        _bossLabel.color = NEON_CYAN;
        _bossBar.SetActive(false);

        // banner e as mensagens do meio
        _banner = MakeText("banner", new Vector2(0.5f, 1), new Vector2(0, -34), new Vector2(900, 40), 30, TextAnchor.MiddleCenter);
        _center = MakeText("center", new Vector2(0.5f, 0.5f), new Vector2(0, 30), new Vector2(1000, 90), 56, TextAnchor.MiddleCenter);
        _centerSub = MakeText("centerSub", new Vector2(0.5f, 0.5f), new Vector2(0, -40), new Vector2(1000, 60), 26, TextAnchor.MiddleCenter);
        _hint = MakeText("hint", new Vector2(0.5f, 0), new Vector2(0, 34), new Vector2(1200, 36), 21, TextAnchor.MiddleCenter);
        _center.gameObject.SetActive(false);
        _centerSub.gameObject.SetActive(false);

        // tela de titulo com a escolha de dificuldade
        _menu = MakeGroup("menu");
        var t = _menu.transform;
        var title = MakeText("title", new Vector2(0.5f, 0.5f), new Vector2(0, 150), new Vector2(1000, 90), 72, TextAnchor.MiddleCenter, t);
        title.text = "L U M E N"; title.color = NEON_CYAN;
        var sub = MakeText("sub", new Vector2(0.5f, 0.5f), new Vector2(0, 96), new Vector2(1000, 40), 26, TextAnchor.MiddleCenter, t);
        sub.text = "// PLATAFORMA 2D CYBERPUNK //"; sub.color = NEON_MAG;
        MakeText("o1", new Vector2(0.5f, 0.5f), new Vector2(0, 18), new Vector2(1000, 40), 32, TextAnchor.MiddleCenter, t).text = "[ 1 ]   FACIL";
        MakeText("o2", new Vector2(0.5f, 0.5f), new Vector2(0, -28), new Vector2(1000, 40), 32, TextAnchor.MiddleCenter, t).text = "[ 2 ]   NORMAL";
        MakeText("o3", new Vector2(0.5f, 0.5f), new Vector2(0, -74), new Vector2(1000, 40), 32, TextAnchor.MiddleCenter, t).text = "[ 3 ]   DIFICIL";
        var mh = MakeText("mh", new Vector2(0.5f, 0.5f), new Vector2(0, -140), new Vector2(1100, 40), 22, TextAnchor.MiddleCenter, t);
        mh.text = "Escolha a dificuldade no teclado  -  J: atirar  Shift: correr  Espaco: pular"; mh.color = new Color(0.7f, 0.85f, 1f);
        _menu.SetActive(false);
    }

    public void ShowMenu(bool v) => _menu.SetActive(v);

    // um container que ocupa a tela toda, ai os filhos ancoram certinho
    GameObject MakeGroup(string name)
    {
        var go = new GameObject(name, typeof(RectTransform));
        var rt = (RectTransform)go.transform;
        rt.SetParent(_root, false);
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        return go;
    }

    Image MakeIcon(Sprite sprite, Vector2 anchor, Vector2 pos, float size, Transform parent = null)
    {
        var go = new GameObject("icon");
        go.transform.SetParent(parent == null ? _root : parent);
        var img = go.AddComponent<Image>();
        img.sprite = sprite; img.preserveAspect = true;
        var rt = img.rectTransform;
        rt.anchorMin = rt.anchorMax = anchor; rt.pivot = new Vector2(0, 1);
        rt.sizeDelta = new Vector2(size, size); rt.anchoredPosition = pos;
        return img;
    }

    Image MakeBar(Vector2 anchor, Vector2 pos, Vector2 size, Color color, Transform parent = null)
    {
        var go = new GameObject("bar");
        go.transform.SetParent(parent == null ? _root : parent);
        var img = go.AddComponent<Image>();
        img.sprite = SpriteFactory.White(); img.color = color;
        var rt = img.rectTransform;
        rt.anchorMin = rt.anchorMax = anchor; rt.pivot = anchor;
        rt.sizeDelta = size; rt.anchoredPosition = pos;
        return img;
    }

    Text MakeText(string name, Vector2 aMin, Vector2 pos, Vector2 size, int fontSize, TextAnchor align, Transform parent = null)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent == null ? _root : parent);
        var t = go.AddComponent<Text>();
        t.font = _font; t.fontSize = fontSize; t.alignment = align; t.color = Color.white;
        t.horizontalOverflow = HorizontalWrapMode.Overflow; t.verticalOverflow = VerticalWrapMode.Overflow;
        var rt = t.rectTransform;
        rt.anchorMin = rt.anchorMax = aMin; rt.pivot = aMin;
        rt.sizeDelta = size; rt.anchoredPosition = pos;
        var sh = go.AddComponent<Shadow>();
        sh.effectColor = new Color(0, 0, 0, 0.85f); sh.effectDistance = new Vector2(2, -2);
        return t;
    }

    // metodos que o resto do jogo chama pra atualizar a HUD
    public void SetLives(int n)
    {
        for (int i = 0; i < _lifeSegs.Length; i++)
            _lifeSegs[i].color = i < n ? NEON_MAG : DIM;
    }
    public void SetScore(int n) => _score.text = "PONTOS: " + n;
    public void SetCrystals(int c, int total) => _crystals.text = c + " / " + total;

    public void SetAmmo(bool hasWeapon, int ammo)
    {
        _ammoGroup.SetActive(hasWeapon);
        if (hasWeapon) _ammo.text = "x " + ammo;
    }

    public void ShowBoss(bool visible) => _bossBar.SetActive(visible);
    public void SetBossHealth(int cur, int max)
    {
        float ratio = max > 0 ? (float)cur / max : 0f;
        _bossBarFill.rectTransform.sizeDelta = new Vector2(436 * ratio, 18);
    }

    public void ShowBanner(string text)
    {
        if (_bannerCo != null) StopCoroutine(_bannerCo);
        _bannerCo = StartCoroutine(BannerRoutine(text));
    }
    IEnumerator BannerRoutine(string text)
    {
        _banner.text = text;
        var c = _banner.color; c.a = 1f; _banner.color = c;
        yield return new WaitForSeconds(2.2f);
        float t = 0f;
        while (t < 1f) { t += Time.deltaTime; c.a = 1f - t; _banner.color = c; yield return null; }
        _banner.text = "";
    }

    public void ShowCenter(string main, string sub, Color color)
    {
        _center.gameObject.SetActive(true); _centerSub.gameObject.SetActive(true);
        _center.text = main; _center.color = color; _centerSub.text = sub;
    }
    public void HideCenter()
    {
        _center.gameObject.SetActive(false); _centerSub.gameObject.SetActive(false);
    }
    public void SetHint(string text) => _hint.text = text;
}
