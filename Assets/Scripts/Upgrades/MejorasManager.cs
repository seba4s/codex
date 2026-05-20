using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controlador principal de la escena Mejoras.
/// Inicializa los 5 slots de mejora y muestra los datos disponibles.
/// </summary>
public class MejorasManager : MonoBehaviour
{
    [Header("Slots de mejora")]
    public UpgradeSlotUI[] slots; // Asignar los 5 slots en el Inspector

    [Header("ScriptableObjects de mejoras")]
    public UpgradeData mejora_ArmaPurga;
    public UpgradeData mejora_EscudoIntegridad;
    public UpgradeData mejora_ModuloEscaneo;
    public UpgradeData mejora_ProtocoloVelocidad;
    public UpgradeData mejora_NanoReparacion;

    [Header("HUD Datos")]
    public TextMeshProUGUI textoDatos;
    public Image iconoDatos;

    [Header("Botón Volver")]
    public Button btnVolver;

    void Start()
    {
        // Asignar datos a cada slot
        if (slots.Length >= 5)
        {
            slots[0].Inicializar(mejora_ArmaPurga, this);
            slots[1].Inicializar(mejora_EscudoIntegridad, this);
            slots[2].Inicializar(mejora_ModuloEscaneo, this);
            slots[3].Inicializar(mejora_ProtocoloVelocidad, this);
            slots[4].Inicializar(mejora_NanoReparacion, this);
        }

        ActualizarDatos();
    }

    /// <summary>Refresca todos los slots y el contador de datos.</summary>
    public void RefrescarTodo()
    {
        foreach (var slot in slots)
            slot.Refrescar();

        ActualizarDatos();
    }

    private void ActualizarDatos()
    {
        if (textoDatos != null)
            textoDatos.text = $"{UpgradeSystem.ObtenerDatos()} DATOS";
    }

    /// <summary>Botón Volver → regresa al Menú Principal.</summary>
    public void OnVolver()
    {
        SceneTransition.Instance.CargarEscena(0);
    }

    // ─── Solo para pruebas en el Editor ──────────────────────────────────────
    [ContextMenu("Agregar 500 Datos de prueba")]
    private void AgregarDatosDePrueba()
    {
        UpgradeSystem.AgregarDatos(500);
        RefrescarTodo();
    }

    [ContextMenu("Resetear TODO (datos y mejoras)")]
    private void ResetearTodo()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        RefrescarTodo();
        Debug.Log("[MejorasManager] PlayerPrefs borrado completamente.");
    }
}
