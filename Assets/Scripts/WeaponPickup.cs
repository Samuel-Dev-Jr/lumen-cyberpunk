using UnityEngine;

// item da arma. quando a Luna encosta ela ganha a arma e a municao e ja pode atirar
public class WeaponPickup : MonoBehaviour
{
    public int ammo = 12;
    bool _taken;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (_taken) return; // _taken pra nao dar a arma duas vezes se entrar no trigger de novo
        var p = other.GetComponent<PlayerController>();
        if (p == null) return;
        _taken = true;
        p.GiveWeapon(ammo);
        AudioManager.Instance.PlayPickup();
        Destroy(gameObject);
    }
}
