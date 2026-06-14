using UnityEngine;

/// <summary>Portal de saída da fase. Ao tocá-lo, avança para o próximo nível.</summary>
public class LevelExit : MonoBehaviour
{
    bool _used;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (_used) return;
        var p = other.GetComponent<PlayerController>();
        if (p == null) return;
        _used = true;
        GameManager.Instance.ReachExit();
    }
}
