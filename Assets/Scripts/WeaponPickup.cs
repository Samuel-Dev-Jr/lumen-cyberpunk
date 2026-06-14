using UnityEngine;

/// <summary>Item de arma (blaster). Ao pegar, o jogador ganha munição e passa a poder atirar.</summary>
public class WeaponPickup : MonoBehaviour
{
    public int ammo = 12;
    bool _taken;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (_taken) return;
        var p = other.GetComponent<PlayerController>();
        if (p == null) return;
        _taken = true;
        p.GiveWeapon(ammo);
        AudioManager.Instance.PlayPickup();
        Destroy(gameObject);
    }
}
