using UnityEngine;

// textinho que aparece quando pega cristal/mata inimigo (tipo "+100") e sobe sumindo.
// uso o TextMesh do Unity pq da pra colocar texto direto no mundo, sem precisar de Canvas.
public class FloatingText : MonoBehaviour
{
    TextMesh _tm;
    Color _color;
    float _life = 0.8f;
    float _max = 0.8f;

    public static void Spawn(Vector3 pos, string text, Color color, Transform parent)
    {
        var go = new GameObject("FloatingText");
        go.transform.SetParent(parent);
        go.transform.position = pos;

        var tm = go.AddComponent<TextMesh>();
        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        tm.font = font;
        tm.text = text;
        tm.color = color;
        tm.fontSize = 48;
        tm.characterSize = 0.09f;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.fontStyle = FontStyle.Bold;

        var mr = go.GetComponent<MeshRenderer>();
        mr.sharedMaterial = font.material; // senao o texto sai sem material e nao aparece
        mr.sortingOrder = 40;

        go.AddComponent<FloatingText>();
    }

    void Start()
    {
        _tm = GetComponent<TextMesh>();
        _color = _tm.color;
    }

    void Update()
    {
        _life -= Time.deltaTime;
        transform.position += Vector3.up * Time.deltaTime * 1.6f; // vai subindo
        if (_tm != null)
        {
            _color.a = _life / _max; // vai sumindo
            _tm.color = _color;
        }
        if (_life <= 0f) Destroy(gameObject);
    }
}
