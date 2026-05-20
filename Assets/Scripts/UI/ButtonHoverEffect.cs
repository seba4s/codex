using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

/// <summary>
/// Efecto visual cyberpunk para botones del menú principal.
/// Agrega este componente a cada botón junto con el componente Button estándar.
/// </summary>
[RequireComponent(typeof(Image))]
public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Color neón hover")]
    [Tooltip("Color del brillo al pasar el cursor (neón)")]
    public Color colorHover = new Color(0f, 1f, 0.85f, 1f); // cian neón

    [Tooltip("Color normal del botón")]
    public Color colorNormal = new Color(1f, 1f, 1f, 1f);

    [Tooltip("Color al presionar el botón")]
    public Color colorPressed = new Color(0.4f, 0.4f, 0.4f, 1f);

    [Header("Escala")]
    [Tooltip("Escala al hacer hover (1.05 = 5% más grande)")]
    public float escalaHover = 1.08f;

    [Tooltip("Escala al presionar")]
    public float escalaPressed = 0.95f;

    [Header("Velocidad de transición")]
    public float velocidad = 10f;

    private Image _imagen;
    private Vector3 _escalaOriginal;
    private Color _colorObjetivo;
    private Vector3 _escalaObjetivo;
    private bool _estaHover = false;

    void Awake()
    {
        _imagen = GetComponent<Image>();
        _escalaOriginal = transform.localScale;
        _colorObjetivo = colorNormal;
        _escalaObjetivo = _escalaOriginal;
    }

    void Update()
    {
        // Interpolar color suavemente
        _imagen.color = Color.Lerp(_imagen.color, _colorObjetivo, Time.deltaTime * velocidad);

        // Interpolar escala suavemente
        transform.localScale = Vector3.Lerp(transform.localScale, _escalaObjetivo, Time.deltaTime * velocidad);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _estaHover = true;
        _colorObjetivo = colorHover;
        _escalaObjetivo = _escalaOriginal * escalaHover;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _estaHover = false;
        _colorObjetivo = colorNormal;
        _escalaObjetivo = _escalaOriginal;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _colorObjetivo = colorPressed;
        _escalaObjetivo = _escalaOriginal * escalaPressed;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (_estaHover)
        {
            _colorObjetivo = colorHover;
            _escalaObjetivo = _escalaOriginal * escalaHover;
        }
        else
        {
            _colorObjetivo = colorNormal;
            _escalaObjetivo = _escalaOriginal;
        }
    }

    // Resetear al desactivar (por si se cambia de escena durante hover)
    void OnDisable()
    {
        _imagen.color = colorNormal;
        transform.localScale = _escalaOriginal;
        _colorObjetivo = colorNormal;
        _escalaObjetivo = _escalaOriginal;
    }
}
