using UnityEngine;

// fica mudando o brilho do fundo pra dar aquele efeito de neon piscando
public class SkyController : MonoBehaviour
{
    SpriteRenderer _sr;
    float _next;

    void Awake() { _sr = GetComponent<SpriteRenderer>(); }

    void Update()
    {
        // uma pulsada suave o tempo todo, e de vez em quando uma piscada mais forte
        float pulse = 0.88f + 0.12f * Mathf.Sin(Time.time * 1.3f);
        float flick = 1f;
        _next -= Time.deltaTime;
        if (_next <= 0f)
        {
            _next = Random.Range(0.15f, 0.9f);
            flick = Random.value < 0.3f ? Random.Range(0.6f, 0.8f) : 1f;
        }
        float b = pulse * flick;
        if (_sr != null) _sr.color = new Color(b, b, b, 1f);
    }
}
