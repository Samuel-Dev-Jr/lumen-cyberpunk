using UnityEngine;

// o portal de saida. quando o player encosta, vai pra proxima fase
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
