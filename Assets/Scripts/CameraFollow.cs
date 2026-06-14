using UnityEngine;

/// <summary>
/// Faz a câmera seguir suavemente o jogador, mantendo-a dentro dos limites da
/// fase para nunca mostrar "fora do mundo".
/// </summary>
public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smooth = 0.12f;
    public Vector2 offset = new Vector2(0f, 1f);

    Camera _cam;
    Vector3 _vel;
    bool _hasBounds;
    float _minX, _maxX, _minY, _maxY;

    void Awake() { _cam = GetComponent<Camera>(); }

    public void SetBounds(float minX, float maxX, float minY, float maxY)
    {
        _minX = minX; _maxX = maxX; _minY = minY; _maxY = maxY;
        _hasBounds = true;
    }

    Vector3 ComputeGoal()
    {
        Vector3 goal = new Vector3(target.position.x + offset.x, target.position.y + offset.y, transform.position.z);
        if (_hasBounds && _cam != null)
        {
            float halfH = _cam.orthographicSize;
            float halfW = halfH * _cam.aspect;
            // se a fase for menor que a tela, centraliza
            float cx = (_minX + _maxX) * 0.5f;
            float cy = (_minY + _maxY) * 0.5f;
            goal.x = (_maxX - _minX) > 2 * halfW ? Mathf.Clamp(goal.x, _minX + halfW, _maxX - halfW) : cx;
            goal.y = (_maxY - _minY) > 2 * halfH ? Mathf.Clamp(goal.y, _minY + halfH, _maxY - halfH) : cy;
        }
        return goal;
    }

    /// <summary>Posiciona a câmera instantaneamente no alvo (usado ao iniciar/recarregar fase).</summary>
    public void SnapToTarget()
    {
        if (target == null) return;
        if (_cam == null) _cam = GetComponent<Camera>();
        _vel = Vector3.zero;
        transform.position = ComputeGoal();
    }

    void LateUpdate()
    {
        if (target == null) return;
        transform.position = Vector3.SmoothDamp(transform.position, ComputeGoal(), ref _vel, smooth);
    }
}
