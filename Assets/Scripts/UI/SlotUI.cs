using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controla un slot individual en la pantalla de selección de partida.
/// Muestra sprite de VACÍO o el sprite del nivel correspondiente.
/// </summary>
public class SlotUI : MonoBehaviour
{
    [Header("Referencias")]
    public Button boton;
    public Image imagenSlot;

    [Header("Sprites")]
    public Sprite spriteVacio;
    public Sprite spriteNivel1;
    public Sprite spriteNivel2;
    public Sprite spriteNivel3;
    public Sprite spriteNivel4;
    public Sprite spriteNivel5;

    [HideInInspector] public int indiceSlot;

    private SaveSlotManager _manager;

    public void Inicializar(int indice, SaveSlotManager manager)
    {
        indiceSlot = indice;
        _manager = manager;
        Refrescar();
    }

    /// <summary>Actualiza el sprite según el estado del slot.</summary>
    public void Refrescar()
    {
        if (!SaveSystem.EstaOcupado(indiceSlot))
        {
            imagenSlot.sprite = spriteVacio;
        }
        else
        {
            int nivel = SaveSystem.ObtenerNivel(indiceSlot);
            imagenSlot.sprite = ObtenerSpriteNivel(nivel);
        }
    }

    private Sprite ObtenerSpriteNivel(int nivel)
    {
        switch (nivel)
        {
            case 1:  return spriteNivel1;
            case 2:  return spriteNivel2;
            case 3:  return spriteNivel3;
            case 4:  return spriteNivel4;
            case 5:  return spriteNivel5;
            default: return spriteNivel1;
        }
    }

    /// <summary>Llamado al presionar el botón del slot.</summary>
    public void OnPresionar()
    {
        _manager.OnSlotPresionado(indiceSlot);
    }
}
