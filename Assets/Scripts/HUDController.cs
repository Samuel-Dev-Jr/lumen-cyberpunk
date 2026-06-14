using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HUD (interface) construída por código — tema cyberpunk. Mostra: barra de vida
/// segmentada (neon), cristais, pontuação, munição da arma, barra de vida do CHEFE
/// e mensagens centrais (fase/Game Over/Vitória).
/// </summary>
public class HUDController : MonoBehaviour
{
    const int MAX_LIVES = 5;
    static readonly Color NEON_MAG = new Color(1f, 0.24f, 0.78f);
    static readonly Color NEON_CYAN = new Color(0.16f, 0.86f, 1f);
    static readonly Color DIM = new Color(0.16f, 0.17f, 0.27f);

    Text _score, _crystals, _banner, _center, _centerSub, _hint, _ammo, _bossLabel;
    Image[] _lifeSegs;
    Image _weaponIcon, _bossBarFill;
    GameObject _bossBar, _ammoGroup;
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

        // ---- vida (coração + segmentos) ----
        MakeIcon(SpriteFactory.Heart(), new Vector2(0, 1), new Vector2(24, -22), 26);
        _lifeSegs = new Image[MAX_LIVES];
        for (int i = 0; i < MAX_LIVES; i++)
            _lifeSegs[i] = MakeBar(new Vector2(0, 1), new Vector2(56 + i * 40, -22), new Vector2(34, 16), NEON_MAG);

        // ---- cristais ----
        MakeIcon(SpriteFactory.CrystalIcon(), new Vector2(0, 1), new Vector2(24, -56), 26);
        _crystals = MakeText("crystals", new Vector2(0, 1), new Vector2(52, -52), new Vector2(220, 30), 24, TextAnchor.MiddleLeft);

        // ---- munição (escondido até pegar arma) ----
        _ammoGroup = new GameObject("ammoGroup"); _ammoGroup.transform.SetParent(_root);
        _weaponIcon = MakeIcon(SpriteFactory.WeaponIcon(), new Vector2(0, 1), new Vector2(24, -90), 26, _ammoGroup.transform);
        _ammo = MakeText("ammo", new Vector2(0, 1), new Vector2(52, -86), new Vector2(180, 30), 24, TextAnchor.MiddleLeft, _ammoGroup.transform);
        _ammoGroup.SetActive(false);

        // ---- pontuação ----
        _score = MakeText("score", new Vector2(1, 1), new Vector2(-20, -22), new Vector2(300, 32), 26, TextAnchor.MiddleRight);

        // ---- barra do chefe (topo central) ----
        _bossBar = new GameObject("bossBar"); _bossBar.transform.SetParent(_root);
        var bgImg = MakeBar(new Vector2(0.5f, 1), new Vector2(0, -50), new Vector2(440, 22), DIM, _bossBar.transform);
        bgImg.rectTransform.pivot = new Vector2(0.5f, 1);
        _bossBarFill = MakeBar(new Vector2(0.5f, 1), new Vector2(0, -50), new Vector2(436, 18), new Color(1f, 0.3f, 0.35f), _bossBar.transform);
        _bossBarFill.rectTransform.pivot = new Vector2(0f, 1f);
        _bossBarFill.rectTransform.anchoredPosition = new Vector2(-218, -52);
        _bossLabel = MakeText("bossLabel", new Vector2(0.5f, 1), new Vector2(0, -28), new Vector2(440, 24), 18, TextAnchor.MiddleCenter, _bossBar.transform);
        _bossLabel.text = "MAINFRAME";
        _bossLabel.color = NEON_CYAN;
        _bossBar.SetActive(false);

        // ---- banners e mensagens ----
        _banner = MakeText("banner", new Vector2(0.5f, 1), new Vector2(0, -34), new Vector2(900, 40), 30, TextAnchor.MiddleCenter);
        _center = MakeText("center", new Vector2(0.5f, 0.5f), new Vector2(0, 30), new Vector2(1000, 90), 56, TextAnchor.MiddleCenter);
        _centerSub = MakeText("centerSub", new Vector2(0.5f, 0.5f), new Vector2(0, -40), new Vector2(1000, 60), 26, TextAnchor.MiddleCenter);
        _hint = MakeText("hint", new Vector2(0.5f, 0), new Vector2(0, 34), new Vector2(1200, 36), 21, TextAnchor.MiddleCenter);
        _center.gameObject.SetActive(false);
        _centerSub.gameObject.SetActive(false);
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

    // ---------- API ----------
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
