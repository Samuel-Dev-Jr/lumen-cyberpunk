using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HUD (interface) construída inteiramente por código: vidas, pontuação,
/// cristais coletados, nome da fase e mensagens centrais (Game Over / Vitória /
/// instruções). Usa UI legada (Canvas + Text) com a fonte embutida da Unity.
/// </summary>
public class HUDController : MonoBehaviour
{
    Text _lives, _score, _crystals, _banner, _center, _centerSub, _hint;
    Font _font;
    Coroutine _bannerCo;

    void Awake()
    {
        _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        Build();
    }

    void Build()
    {
        // ----- Canvas -----
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
        var root = canvasGO.transform;

        // ----- ícones -----
        MakeIcon(root, SpriteFactory.Heart(), new Vector2(0, 1), new Vector2(28, -28));
        MakeIcon(root, SpriteFactory.CrystalIcon(), new Vector2(0, 1), new Vector2(28, -64));

        // ----- textos -----
        _lives = MakeText(root, "lives", new Vector2(0, 1), new Vector2(0, 1), new Vector2(52, -22), new Vector2(220, 32), 26, TextAnchor.MiddleLeft);
        _crystals = MakeText(root, "crystals", new Vector2(0, 1), new Vector2(0, 1), new Vector2(52, -58), new Vector2(220, 32), 26, TextAnchor.MiddleLeft);
        _score = MakeText(root, "score", new Vector2(1, 1), new Vector2(1, 1), new Vector2(-20, -22), new Vector2(280, 32), 26, TextAnchor.MiddleRight);
        _banner = MakeText(root, "banner", new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -34), new Vector2(700, 40), 30, TextAnchor.MiddleCenter);

        _center = MakeText(root, "center", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 30), new Vector2(900, 90), 56, TextAnchor.MiddleCenter);
        _centerSub = MakeText(root, "centerSub", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -40), new Vector2(900, 60), 28, TextAnchor.MiddleCenter);
        _hint = MakeText(root, "hint", new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 36), new Vector2(1100, 40), 22, TextAnchor.MiddleCenter);

        _center.gameObject.SetActive(false);
        _centerSub.gameObject.SetActive(false);
    }

    Image MakeIcon(Transform parent, Sprite sprite, Vector2 anchor, Vector2 pos)
    {
        var go = new GameObject("icon");
        go.transform.SetParent(parent);
        var img = go.AddComponent<Image>();
        img.sprite = sprite;
        img.preserveAspect = true;
        var rt = img.rectTransform;
        rt.anchorMin = rt.anchorMax = anchor;
        rt.pivot = new Vector2(0, 1);
        rt.sizeDelta = new Vector2(28, 28);
        rt.anchoredPosition = pos;
        return img;
    }

    Text MakeText(Transform parent, string name, Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 size, int fontSize, TextAnchor align)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent);
        var t = go.AddComponent<Text>();
        t.font = _font;
        t.fontSize = fontSize;
        t.alignment = align;
        t.color = Color.white;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        var rt = t.rectTransform;
        rt.anchorMin = aMin;
        rt.anchorMax = aMax;
        rt.pivot = aMin; // pivô no mesmo canto da âncora -> evita o texto sair da tela
        rt.sizeDelta = size;
        rt.anchoredPosition = pos;
        // sombra para legibilidade
        var sh = go.AddComponent<Shadow>();
        sh.effectColor = new Color(0, 0, 0, 0.8f);
        sh.effectDistance = new Vector2(2, -2);
        return t;
    }

    // ----- API -----
    public void SetLives(int n) { _lives.text = "x " + n; }
    public void SetScore(int n) { _score.text = "Pontos: " + n; }
    public void SetCrystals(int c, int total) { _crystals.text = "x " + c + " / " + total; }

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
        _center.gameObject.SetActive(true);
        _centerSub.gameObject.SetActive(true);
        _center.text = main; _center.color = color;
        _centerSub.text = sub;
    }

    public void HideCenter()
    {
        _center.gameObject.SetActive(false);
        _centerSub.gameObject.SetActive(false);
    }

    public void SetHint(string text) { _hint.text = text; }
}
