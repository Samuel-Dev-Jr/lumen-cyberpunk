using UnityEngine;

/// <summary>Coração de vida: ao pegar, aumenta a vida do jogador (até o máximo).</summary>
public class HeartPickup : MonoBehaviour
{
    bool _taken;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (_taken) return;
        var p = other.GetComponent<PlayerController>();
        if (p == null) return;
        _taken = true;
        GameManager.Instance.GainLife();
        AudioManager.Instance.PlayPickup();
        Destroy(gameObject);
    }
}
