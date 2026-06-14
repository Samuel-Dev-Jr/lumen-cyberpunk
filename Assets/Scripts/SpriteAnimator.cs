using UnityEngine;

/// <summary>
/// Reproduz uma sequência de Sprites (frames) em um SpriteRenderer,
/// trocando o quadro a cada intervalo de tempo. É o nosso "sistema de animação"
/// por troca de quadros (sprite-sheet animation feita por código).
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class SpriteAnimator : MonoBehaviour
{
    Sprite[] _frames;
    float _fps = 8f;
    bool _loop = true;
    float _timer;
    int _index;
    SpriteRenderer _sr;

    void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
    }

    /// <summary>Troca a animação atual. Só reinicia se os frames forem diferentes.</summary>
    public void Play(Sprite[] frames, float fps = 8f, bool loop = true)
    {
        if (frames == null || frames.Length == 0) return;
        if (_frames == frames) { _fps = fps; _loop = loop; return; }
        _frames = frames;
        _fps = fps;
        _loop = loop;
        _index = 0;
        _timer = 0f;
        if (_sr == null) _sr = GetComponent<SpriteRenderer>();
        _sr.sprite = _frames[0];
    }

    void Update()
    {
        if (_frames == null || _frames.Length <= 1 || _fps <= 0f) return;
        _timer += Time.deltaTime;
        float frameDur = 1f / _fps;
        while (_timer >= frameDur)
        {
            _timer -= frameDur;
            _index++;
            if (_index >= _frames.Length)
                _index = _loop ? 0 : _frames.Length - 1;
            _sr.sprite = _frames[_index];
        }
    }
}
