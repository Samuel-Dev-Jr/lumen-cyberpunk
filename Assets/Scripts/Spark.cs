using UnityEngine;

/// <summary>Faísca de feedback visual: move-se, desacelera, some e se destrói.</summary>
public class Spark : MonoBehaviour
{
    Vector2 _vel;
    float _life, _maxLife;
    SpriteRenderer _sr;
    Color _color;

    public void Init(Vector2 vel, float life)
    {
        _vel = vel;
        _life = _maxLife = life;
        _sr = GetComponent<SpriteRenderer>();
        _color = _sr.color;
    }

    void Update()
    {
        _life -= Time.deltaTime;
        if (_life <= 0f) { Destroy(gameObject); return; }
        transform.position += (Vector3)_vel * Time.deltaTime;
        _vel *= 0.92f;
        if (_sr != null)
        {
            _color.a = _life / _maxLife;
            _sr.color = _color;
        }
    }
}
