using UnityEngine;

// camera que segue o player de forma suave, mas presa nos limites da fase
// pra nao aparecer o "nada" fora do cenario. tbm faco o tremor de tela (screen shake) aqui.
public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smooth = 0.12f;
    public Vector2 offset = new Vector2(0f, 1f);

    Camera _cam;
    Vector3 _vel;
    Vector3 _basePos;   // posicao "limpa" da camera (sem o tremor), pra ele nao acumular
    float _shake;
    bool _hasBounds;
    float _minX, _maxX, _minY, _maxY;

    void Awake()
    {
        _cam = GetComponent<Camera>();
        _basePos = transform.position;
    }

    public void SetBounds(float minX, float maxX, float minY, float maxY)
    {
        _minX = minX; _maxX = maxX; _minY = minY; _maxY = maxY;
        _hasBounds = true;
    }

    // outros scripts chamam isso pra dar aquele tremor (dano, pisao, tiro no boss...)
    public void Shake(float amount)
    {
        if (amount > _shake) _shake = amount;
    }

    Vector3 ComputeGoal()
    {
        Vector3 goal = new Vector3(target.position.x + offset.x, target.position.y + offset.y, _basePos.z);
        if (_hasBounds && _cam != null)
        {
            float halfH = _cam.orthographicSize;
            float halfW = halfH * _cam.aspect;
            // quando a fase for menor que a tela nao tem como dar clamp, entao so centraliza
            float cx = (_minX + _maxX) * 0.5f;
            float cy = (_minY + _maxY) * 0.5f;
            goal.x = (_maxX - _minX) > 2 * halfW ? Mathf.Clamp(goal.x, _minX + halfW, _maxX - halfW) : cx;
            goal.y = (_maxY - _minY) > 2 * halfH ? Mathf.Clamp(goal.y, _minY + halfH, _maxY - halfH) : cy;
        }
        return goal;
    }

    // joga a camera direto em cima do alvo, sem suavizar. uso isso quando comeca/recarrega a fase
    public void SnapToTarget()
    {
        if (target == null) return;
        if (_cam == null) _cam = GetComponent<Camera>();
        _vel = Vector3.zero;
        _basePos = ComputeGoal();
        transform.position = _basePos;
    }

    void LateUpdate()
    {
        if (target == null) return;
        _basePos = Vector3.SmoothDamp(_basePos, ComputeGoal(), ref _vel, smooth);

        Vector3 shakeOff = Vector3.zero;
        if (_shake > 0f)
        {
            Vector2 r = Random.insideUnitCircle * _shake;
            shakeOff = new Vector3(r.x, r.y, 0f);
            _shake = Mathf.MoveTowards(_shake, 0f, Time.deltaTime * 8f); // vai sumindo rapido
        }
        transform.position = _basePos + shakeOff;
    }
}
