using UnityEngine;

// os espinhos. machuca o player se encostar
public class Hazard : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other) => Hit(other);
    void OnTriggerStay2D(Collider2D other) => Hit(other);

    void Hit(Collider2D other)
    {
        var p = other.GetComponent<PlayerController>();
        if (p != null) p.TakeDamage(transform.position);
    }
}
