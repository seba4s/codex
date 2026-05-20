using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controla el panel visual de una mejora individual.
/// Muestra el ícono, nombre, nivel actual, descripción del próximo nivel,
/// costo y los segmentos de nivel (vacío/lleno).
/// </summary>
public class UpgradeSlotUI : MonoBehaviour
{
    [Header("Referencias UI")]
    public Image iconoMejora;
    public TextMeshProUGUI textoNombre;
    public TextMeshProUGUI textoDescripcion;
    public TextMeshProUGUI textoCosto;
    public Button botonMejorar;
    public Image imagenBotonMejorar;
    public TextMeshProUGUI textoBotonMejorar;

    [Header("Segmentos de nivel")]
    public Image[] segmentos;       // Array de imágenes para cada nivel (máx 5)
    public Sprite spriteSegVacio;
    public Sprite spriteSegLleno;

    [HideInInspector] public UpgradeData data;
    private MejorasManager _manager;

    public void Inicializar(UpgradeData upgradeData, MejorasManager manager)
    {
        data = upgradeData;
        _manager = manager;
        Refrescar();
    }

    /// <summary>Actualiza toda la UI según el estado actual de la mejora.</summary>
    public void Refrescar()
    {
        if (data == null) return;

        int nivelActual = UpgradeSystem.ObtenerNivel(data.id);
        int datos = UpgradeSystem.ObtenerDatos();

        // Ícono y nombre
        if (iconoMejora != null) iconoMejora.sprite = data.icono;
        if (textoNombre != null) textoNombre.text = data.nombreMejora;

        // Descripción del próximo nivel o MAX
        if (textoDescripcion != null)
        {
            if (nivelActual >= data.nivelMaximo)
                textoDescripcion.text = "MÓDULO AL MÁXIMO";
            else if (data.descripcionPorNivel != null && nivelActual < data.descripcionPorNivel.Length)
                textoDescripcion.text = data.descripcionPorNivel[nivelActual];
        }

        // Costo
        if (textoCosto != null)
        {
            if (nivelActual >= data.nivelMaximo)
                textoCosto.text = "---";
            else
                textoCosto.text = $"{data.costosPorNivel[nivelActual]} DATOS";
        }

        // Segmentos de nivel
        for (int i = 0; i < segmentos.Length; i++)
        {
            if (segmentos[i] == null) continue;
            if (i < data.nivelMaximo)
            {
                segmentos[i].gameObject.SetActive(true);
                segmentos[i].sprite = (i < nivelActual) ? spriteSegLleno : spriteSegVacio;
            }
            else
            {
                segmentos[i].gameObject.SetActive(false);
            }
        }

        // Botón Mejorar
        bool puedeComprar = nivelActual < data.nivelMaximo &&
                            datos >= data.costosPorNivel[nivelActual];
        Debug.Log($"[SlotUI] {data.nombreMejora} | datos={datos} costo={( nivelActual < data.nivelMaximo ? data.costosPorNivel[nivelActual] : 0)} puedeComprar={puedeComprar}");
        if (botonMejorar != null)
            botonMejorar.interactable = puedeComprar;
        if (imagenBotonMejorar != null)
        {
            Color c = imagenBotonMejorar.color;
            c.a = puedeComprar ? 1f : 0.3f;
            imagenBotonMejorar.color = c;
        }
        if (textoBotonMejorar != null)
        {
            Color c = textoBotonMejorar.color;
            c.a = puedeComprar ? 1f : 0.3f;
            textoBotonMejorar.color = c;
        }
    }

    /// <summary>Llamado al presionar el botón Mejorar de este slot.</summary>
    public void OnMejorar()
    {
        if (UpgradeSystem.Mejorar(data))
        {
            _manager.RefrescarTodo();
        }
    }
}
