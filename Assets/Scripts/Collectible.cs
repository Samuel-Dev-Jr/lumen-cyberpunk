using UnityEngine;

// o cristal que da pra pegar. soma pontos e conta no total da fase.
public class Collectible : MonoBehaviour
{
    public int value = 100;
    bool _taken;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (_taken) return;
        var p = other.GetComponent<PlayerController>();
        if (p == null) return;

        _taken = true;
        GameManager.Instance.CollectCrystal(value);
        AudioManager.Instance.PlayCoin();
        SpawnFlash();
        // mostra o "+100" subindo pra dar aquele feedback de pontos
        FloatingText.Spawn(transform.position + Vector3.up * 0.4f, "+" + value, new Color(0.16f, 0.86f, 1f), transform.parent);
        Destroy(gameObject);
    }

    // so um efeitinho de luz quando pega, fica mais legal
    void SpawnFlash()
    {
        for (int i = 0; i < 6; i++)
        {
            var go = new GameObject("flash");
            go.transform.position = transform.position;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteFactory.White();
            sr.color = new Color(1f, 0.9f, 0.5f, 1f);
            sr.sortingOrder = 30;
            go.transform.localScale = Vector3.one * 0.12f;
            float ang = i * 60f * Mathf.Deg2Rad;
            var vel = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * 3f;
            go.AddComponent<Spark>().Init(vel, 0.4f);
        }
    }
}
