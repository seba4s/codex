using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controlador principal de la escena EscogerPartida.
/// Maneja los 5 slots, el botón Eliminar y el botón Volver.
/// </summary>
public class SaveSlotManager : MonoBehaviour
{
    [Header("Slots")]
    public SlotUI[] slots; // Asignar los 5 slots en el Inspector

    [Header("Botón Eliminar")]
    public Button btnEliminar;
    public Image imagenBtnEliminar;

    [Header("Botón Volver")]
    public Button btnVolver;

    [Header("Índices de escenas")]
    [Tooltip("Índice de la escena del Nivel 1 en Build Settings")]
    public int indiceNivel1 = 1; // Tutorial / Nivel 1

    // Slot seleccionado actualmente para eliminar
    private int _slotSeleccionado = -1;

    void Start()
    {
        // Inicializar todos los slots
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].Inicializar(i, this);
        }

        // Botón eliminar desactivado al inicio
        ActualizarBtnEliminar();
    }

    /// <summary>Llamado desde SlotUI al presionar un slot.</summary>
    public void OnSlotPresionado(int indice)
    {
        if (!SaveSystem.EstaOcupado(indice))
        {
            // Slot vacío → nueva partida desde nivel 1
            SaveSystem.SetSlotActivo(indice);
            SaveSystem.Guardar(indice, 1);
            Debug.Log($"[SaveSlotManager] Nueva partida en slot {indice} → Nivel 1");
            SceneTransition.Instance.CargarEscena(indiceNivel1);
        }
        else
        {
            // Slot ocupado → seleccionar para eliminar o continuar
            if (_slotSeleccionado == indice)
            {
                // Segunda pulsación → continuar partida
                SaveSystem.SetSlotActivo(indice);
                int nivel = SaveSystem.ObtenerNivel(indice);
                Debug.Log($"[SaveSlotManager] Continuar slot {indice} → Nivel {nivel}");
                SceneTransition.Instance.CargarEscena(indiceNivel1);
            }
            else
            {
                // Primera pulsación → seleccionar slot
                _slotSeleccionado = indice;
                ActualizarBtnEliminar();
                Debug.Log($"[SaveSlotManager] Slot {indice} seleccionado");
            }
        }
    }

    /// <summary>Botón Eliminar → borra el slot seleccionado.</summary>
    public void OnEliminar()
    {
        if (_slotSeleccionado < 0) return;

        SaveSystem.Eliminar(_slotSeleccionado);
        slots[_slotSeleccionado].Refrescar();

        _slotSeleccionado = -1;
        ActualizarBtnEliminar();

        Debug.Log("[SaveSlotManager] Slot eliminado.");
    }

    /// <summary>Botón Volver → regresa al Menú Principal.</summary>
    public void OnVolver()
    {
        SceneTransition.Instance.CargarEscena(0);
    }

    private void ActualizarBtnEliminar()
    {
        // Activar el botón eliminar solo si hay un slot ocupado seleccionado
        bool activar = _slotSeleccionado >= 0 && SaveSystem.EstaOcupado(_slotSeleccionado);
        btnEliminar.interactable = activar;

        if (imagenBtnEliminar != null)
        {
            Color c = imagenBtnEliminar.color;
            c.a = activar ? 1f : 0.4f;
            imagenBtnEliminar.color = c;
        }
    }
}
