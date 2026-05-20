using UnityEngine;

/// <summary>
/// Define los datos estáticos de una mejora (nombre, descripción, costos por nivel).
/// Crear un ScriptableObject por cada mejora en el proyecto.
/// </summary>
[CreateAssetMenu(fileName = "NuevaMejora", menuName = "CODE7/Mejora")]
public class UpgradeData : ScriptableObject
{
    [Header("Identificación")]
    public string id;           // clave única para PlayerPrefs
    public string nombreMejora;
    [TextArea(2, 4)]
    public string descripcion;
    public Sprite icono;

    [Header("Niveles")]
    public int nivelMaximo = 5;

    [Tooltip("Costo de cada nivel. El array debe tener 'nivelMaximo' entradas.")]
    public int[] costosPorNivel; // ej: {40, 60, 80, 100, 120}

    [TextArea(1, 3)]
    public string[] descripcionPorNivel; // descripción de cada nivel
}
