using UnityEngine;

// dá uma "respirada" no fundo, o brilho sobe e desce bem devagar.
// antes eu tinha botado uma piscada aleatoria mais forte, mas no jogo parecia bug,
// entao tirei e deixei so esse pulso suave.
public class SkyController : MonoBehaviour
{
    SpriteRenderer _sr;

    void Awake() { _sr = GetComponent<SpriteRenderer>(); }

    void Update()
    {
        if (_sr == null) return;
        float b = 0.94f + 0.06f * Mathf.Sin(Time.time * 0.7f); // variacao pequena, quase nao da pra perceber
        _sr.color = new Color(b, b, b, 1f);
    }
}
