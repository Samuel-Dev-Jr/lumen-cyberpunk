using UnityEngine;

// coracaozinho de vida. pega e recupera uma vida (o GainLife ja cuida de nao passar do maximo)
public class HeartPickup : MonoBehaviour
{
    bool _taken;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (_taken) return; // mesma ideia do outro pickup, evita pegar duas vezes
        var p = other.GetComponent<PlayerController>();
        if (p == null) return;
        _taken = true;
        GameManager.Instance.GainLife();
        AudioManager.Instance.PlayPickup();
        Destroy(gameObject);
    }
}
