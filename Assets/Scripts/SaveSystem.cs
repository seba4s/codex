using UnityEngine;

/// <summary>
/// Sistema de guardado global. Usa PlayerPrefs para guardar hasta 5 slots.
/// Cada slot guarda: si está ocupado, el nivel alcanzado y la fecha.
/// </summary>
public static class SaveSystem
{
    public const int MAX_SLOTS = 5;

    // ─── Claves PlayerPrefs ───────────────────────────────────────────────────
    private static string KeyOcupado(int slot)  => $"Slot_{slot}_Ocupado";
    private static string KeyNivel(int slot)     => $"Slot_{slot}_Nivel";
    private static string KeyFecha(int slot)     => $"Slot_{slot}_Fecha";

    // ─── Guardar ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Guarda el progreso en un slot específico.
    /// Llamar desde el juego cuando el jugador llega a un nuevo nivel.
    /// </summary>
    public static void Guardar(int slot, int nivel)
    {
        if (slot < 0 || slot >= MAX_SLOTS) return;

        PlayerPrefs.SetInt(KeyOcupado(slot), 1);
        PlayerPrefs.SetInt(KeyNivel(slot), nivel);
        PlayerPrefs.SetString(KeyFecha(slot), System.DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
        PlayerPrefs.Save();

        Debug.Log($"[SaveSystem] Slot {slot} guardado → Nivel {nivel}");
    }

    // ─── Cargar ───────────────────────────────────────────────────────────────

    /// <summary>Retorna true si el slot tiene una partida guardada.</summary>
    public static bool EstaOcupado(int slot)
    {
        return PlayerPrefs.GetInt(KeyOcupado(slot), 0) == 1;
    }

    /// <summary>Retorna el nivel guardado en el slot (-1 si está vacío).</summary>
    public static int ObtenerNivel(int slot)
    {
        if (!EstaOcupado(slot)) return -1;
        return PlayerPrefs.GetInt(KeyNivel(slot), 1);
    }

    /// <summary>Retorna la fecha de guardado del slot.</summary>
    public static string ObtenerFecha(int slot)
    {
        return PlayerPrefs.GetString(KeyFecha(slot), "");
    }

    // ─── Eliminar ─────────────────────────────────────────────────────────────

    /// <summary>Borra todos los datos de un slot.</summary>
    public static void Eliminar(int slot)
    {
        if (slot < 0 || slot >= MAX_SLOTS) return;

        PlayerPrefs.DeleteKey(KeyOcupado(slot));
        PlayerPrefs.DeleteKey(KeyNivel(slot));
        PlayerPrefs.DeleteKey(KeyFecha(slot));
        PlayerPrefs.Save();

        Debug.Log($"[SaveSystem] Slot {slot} eliminado.");
    }

    // ─── Slot activo ──────────────────────────────────────────────────────────

    /// <summary>Guarda qué slot está siendo usado actualmente en la partida.</summary>
    public static void SetSlotActivo(int slot)
    {
        PlayerPrefs.SetInt("SlotActivo", slot);
        PlayerPrefs.Save();
    }

    /// <summary>Retorna el slot activo actual (-1 si no hay ninguno).</summary>
    public static int GetSlotActivo()
    {
        return PlayerPrefs.GetInt("SlotActivo", -1);
    }
}
