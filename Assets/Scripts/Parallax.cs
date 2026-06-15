using UnityEngine;

// faz uma camada do fundo se mover um pouco mais devagar que a camera, pra dar
// aquela sensacao de profundidade (parallax). coloco isso numa camada de cidade.
public class Parallax : MonoBehaviour
{
    public Transform cam;
    public float factor = 0.12f; // quanto maior, mais ela "fica pra tras"
    Vector3 _base;

    void Start() { _base = transform.localPosition; }

    void LateUpdate()
    {
        if (cam == null) return;
        // a camada ta presa na camera, entao desloco ela ao contrario do movimento pra ela parecer mais longe
        transform.localPosition = new Vector3(
            _base.x - cam.position.x * factor,
            _base.y - cam.position.y * factor * 0.4f,
            _base.z);
    }
}
