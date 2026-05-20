using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Efecto de levitación suave para el logo del menú principal.
/// Agregar este componente al GameObject del Logo.
/// </summary>
public class LogoFloatEffect : MonoBehaviour
{
    [Header("Levitación")]
    [Tooltip("Qué tan alto/bajo se mueve el logo (en píxeles)")]
    public float amplitud = 15f;

    [Tooltip("Qué tan rápido levita")]
    public float velocidad = 1.2f;

    private RectTransform _rectTransform;
    private Vector2 _posicionOriginal;

    void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _posicionOriginal = _rectTransform.anchoredPosition;
    }

    void Update()
    {
        float offsetY = Mathf.Sin(Time.time * velocidad) * amplitud;
        _rectTransform.anchoredPosition = _posicionOriginal + new Vector2(0f, offsetY);
    }
}
