using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

/// <summary>
/// Animación holográfica cyberpunk para botones del menú principal.
/// Adjuntar a cada botón. Asignar referencias en el Inspector.
/// </summary>
public class MenuButtonFX : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Referencias")]
    [SerializeField] private Image background;
    [SerializeField] private Image borderLeft;
    [SerializeField] private TMP_Text label;
    [SerializeField] private RectTransform rectTransform;

    [Header("Colores")]
    [SerializeField] private Color colorBgNormal    = new Color(0.039f, 0.098f, 0.161f, 0.78f);
    [SerializeField] private Color colorBgHover     = new Color(0.118f, 0.565f, 1f,     0.35f);
    [SerializeField] private Color colorBgPress     = new Color(0f,     1f,     1f,     0.18f);

    [SerializeField] private Color colorBorderNormal = new Color(0f, 1f, 1f, 1f);
    [SerializeField] private Color colorBorderHover  = new Color(0f, 1f, 1f, 1f);
    [SerializeField] private Color colorBorderPress  = new Color(0f, 1f, 0.6f, 1f);

    [SerializeField] private Color colorTextNormal  = Color.white;
    [SerializeField] private Color colorTextHover   = new Color(0f, 1f, 1f, 1f);
    [SerializeField] private Color colorTextPress   = new Color(0f, 1f, 0.6f, 1f);

    [Header("Animación")]
    [SerializeField] private float duracionTransicion = 0.12f;
    [SerializeField] private float desplazamientoHover = 10f;
    [SerializeField] private float scaleHover = 1.04f;
    [SerializeField] private float scalePress = 0.97f;

    // Estado
    private Vector2 posicionOriginal;
    private bool estaHover = false;
    private Coroutine corTransicion;
    private Coroutine corGlowPulse;

    void Awake()
    {
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        posicionOriginal = rectTransform.anchoredPosition;
        AplicarEstadoInstante(EstadoBoton.Normal);
    }

    void OnEnable()
    {
        // Animación de entrada al aparecer la escena
        StartCoroutine(AnimacionEntrada());
    }

    // ── Eventos de puntero ──────────────────────────────────────────────

    public void OnPointerEnter(PointerEventData eventData)
    {
        estaHover = true;
        IniciarTransicion(EstadoBoton.Hover);
        if (corGlowPulse != null) StopCoroutine(corGlowPulse);
        corGlowPulse = StartCoroutine(GlowPulse());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        estaHover = false;
        if (corGlowPulse != null) { StopCoroutine(corGlowPulse); corGlowPulse = null; }
        IniciarTransicion(EstadoBoton.Normal);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        IniciarTransicion(EstadoBoton.Press);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        IniciarTransicion(estaHover ? EstadoBoton.Hover : EstadoBoton.Normal);
    }

    // ── Transiciones ────────────────────────────────────────────────────

    private enum EstadoBoton { Normal, Hover, Press }

    private void IniciarTransicion(EstadoBoton estado)
    {
        if (corTransicion != null) StopCoroutine(corTransicion);
        corTransicion = StartCoroutine(Transicion(estado));
    }

    private IEnumerator Transicion(EstadoBoton estado)
    {
        Color bgObjivo, borderObjetivo, textObjetivo;
        Vector2 posObjetivo;
        float scaleObjetivo;

        switch (estado)
        {
            case EstadoBoton.Hover:
                bgObjivo       = colorBgHover;
                borderObjetivo = colorBorderHover;
                textObjetivo   = colorTextHover;
                posObjetivo    = posicionOriginal + Vector2.right * desplazamientoHover;
                scaleObjetivo  = scaleHover;
                break;
            case EstadoBoton.Press:
                bgObjivo       = colorBgPress;
                borderObjetivo = colorBorderPress;
                textObjetivo   = colorTextPress;
                posObjetivo    = posicionOriginal + Vector2.right * (desplazamientoHover * 0.5f);
                scaleObjetivo  = scalePress;
                break;
            default: // Normal
                bgObjivo       = colorBgNormal;
                borderObjetivo = colorBorderNormal;
                textObjetivo   = colorTextNormal;
                posObjetivo    = posicionOriginal;
                scaleObjetivo  = 1f;
                break;
        }

        float t = 0f;
        Color bgInicio      = background  != null ? background.color  : Color.clear;
        Color borderInicio  = borderLeft  != null ? borderLeft.color  : Color.clear;
        Color textInicio    = label       != null ? label.color        : Color.white;
        Vector2 posInicio   = rectTransform.anchoredPosition;
        Vector3 scaleInicio = rectTransform.localScale;
        Vector3 scaleFinal  = new Vector3(scaleObjetivo, scaleObjetivo, 1f);

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / duracionTransicion;
            float ease = 1f - Mathf.Pow(1f - Mathf.Clamp01(t), 3f); // ease out cubic

            if (background != null) background.color = Color.Lerp(bgInicio, bgObjivo, ease);
            if (borderLeft != null) borderLeft.color  = Color.Lerp(borderInicio, borderObjetivo, ease);
            if (label      != null) label.color        = Color.Lerp(textInicio, textObjetivo, ease);

            rectTransform.anchoredPosition = Vector2.Lerp(posInicio, posObjetivo, ease);
            rectTransform.localScale       = Vector3.Lerp(scaleInicio, scaleFinal, ease);

            yield return null;
        }
    }

    private IEnumerator GlowPulse()
    {
        // Pulso sutil en el borde mientras está en hover
        while (true)
        {
            float t = 0f;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime * 2f;
                float alpha = Mathf.Lerp(0.6f, 1f, Mathf.PingPong(t, 1f));
                if (borderLeft != null)
                {
                    Color c = borderLeft.color;
                    c.a = alpha;
                    borderLeft.color = c;
                }
                yield return null;
            }
        }
    }

    private IEnumerator AnimacionEntrada()
    {
        // Slide desde la izquierda + fade in al iniciar la escena
        Vector2 posInicio = posicionOriginal + Vector2.left * 60f;
        rectTransform.anchoredPosition = posInicio;

        if (background != null) { Color c = colorBgNormal; c.a = 0f; background.color = c; }
        if (label      != null) { Color c = colorTextNormal; c.a = 0f; label.color = c; }
        if (borderLeft != null) { Color c = colorBorderNormal; c.a = 0f; borderLeft.color = c; }

        float duracion = 0.35f;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / duracion;
            float ease = 1f - Mathf.Pow(1f - Mathf.Clamp01(t), 3f);

            rectTransform.anchoredPosition = Vector2.Lerp(posInicio, posicionOriginal, ease);

            if (background != null) { Color c = colorBgNormal; c.a = Mathf.Lerp(0f, colorBgNormal.a, ease); background.color = c; }
            if (label      != null) { Color c = colorTextNormal; c.a = ease; label.color = c; }
            if (borderLeft != null) { Color c = colorBorderNormal; c.a = ease; borderLeft.color = c; }

            yield return null;
        }

        AplicarEstadoInstante(EstadoBoton.Normal);
    }

    private void AplicarEstadoInstante(EstadoBoton estado)
    {
        if (background != null) background.color = estado == EstadoBoton.Normal ? colorBgNormal : colorBgHover;
        if (borderLeft != null) borderLeft.color  = colorBorderNormal;
        if (label      != null) label.color        = colorTextNormal;
        rectTransform.localScale       = Vector3.one;
        rectTransform.anchoredPosition = posicionOriginal;
    }
}
