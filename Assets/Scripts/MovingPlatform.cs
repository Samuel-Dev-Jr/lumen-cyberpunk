using UnityEngine;

// plataforma que vai e volta entre dois pontos. ela carrega o player junto quando
// ele ta em cima (como o player nao tem atrito, eu tenho que empurrar ele na mao).
public class MovingPlatform : MonoBehaviour
{
    public Vector2 moveBy = new Vector2(3.5f, 0f); // o quanto ela anda de um ponto ao outro
    public float speed = 1.8f;

    Vector3 _a, _b, _prev;
    float _t;
    int _dir = 1;
    BoxCollider2D _col;

    void Start()
    {
        _a = transform.position;
        _b = _a + (Vector3)moveBy;
        _prev = transform.position;
        _col = GetComponent<BoxCollider2D>();
    }

    void FixedUpdate()
    {
        float dist = Mathf.Max(0.01f, ((Vector2)(_b - _a)).magnitude);
        _t += _dir * speed * Time.fixedDeltaTime / dist;
        if (_t >= 1f) { _t = 1f; _dir = -1; }
        else if (_t <= 0f) { _t = 0f; _dir = 1; }

        Vector3 pos = Vector3.Lerp(_a, _b, _t);
        Vector3 delta = pos - _prev;

        // se o player ta em cima, ando ele junto
        var p = GameManager.Instance != null ? GameManager.Instance.Player : null;
        if (p != null && PlayerOnTop()) p.transform.position += delta;

        transform.position = pos;
        _prev = pos;
    }

    bool PlayerOnTop()
    {
        if (_col == null) return false;
        Bounds b = _col.bounds;
        Vector2 center = new Vector2(b.center.x, b.max.y + 0.1f);
        Vector2 size = new Vector2(b.size.x, 0.3f);
        var hits = Physics2D.OverlapBoxAll(center, size, 0f); // triggers sao ignorados (config global)
        foreach (var h in hits)
            if (h.GetComponent<PlayerController>() != null) return true;
        return false;
    }
}
