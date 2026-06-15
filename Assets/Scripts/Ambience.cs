using UnityEngine;

// fica soltando umas particulazinhas neon flutuando pela tela, so pra dar
// um clima de cidade cyberpunk. coloco isso na camera e elas nascem dentro do que da pra ver.
public class Ambience : MonoBehaviour
{
    public Camera cam;
    float _timer;

    void Update()
    {
        if (cam == null) return;
        _timer -= Time.deltaTime;
        if (_timer > 0f) return;
        _timer = 0.35f; // de quanto em quanto tempo nasce uma

        // sorteio um ponto dentro da area que a camera ta mostrando
        float vx = Random.value, vy = Random.value;
        Vector3 p = cam.ViewportToWorldPoint(new Vector3(vx, vy, 10f));
        p.z = 0f;

        var go = new GameObject("mote");
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteFactory.White();
        // ora ciano, ora magenta, bem de leve
        sr.color = (Random.value < 0.5f) ? new Color(0.16f, 0.86f, 1f, 0.5f) : new Color(1f, 0.3f, 0.8f, 0.45f);
        sr.sortingOrder = -50; // atras de tudo (menos o fundo)
        go.transform.position = p;
        go.transform.localScale = Vector3.one * Random.Range(0.04f, 0.09f);
        // sobe devagarinho e some
        go.AddComponent<Spark>().Init(new Vector2(Random.Range(-0.2f, 0.2f), Random.Range(0.3f, 0.7f)), Random.Range(2.5f, 4f));
    }
}
