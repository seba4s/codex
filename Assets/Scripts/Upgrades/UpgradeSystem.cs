using UnityEngine;

/// <summary>
/// Sistema global de mejoras. Guarda/carga niveles y datos con PlayerPrefs.
/// Acceder desde cualquier script con UpgradeSystem.ObtenerNivel(id), etc.
/// </summary>
public static class UpgradeSystem
{
    // ─── Claves PlayerPrefs ───────────────────────────────────────────────────
    private static string KeyNivel(string id)  => $"Upgrade_{id}_Nivel";
    private static string KeyDatos()           => "Datos_Total";

    // ─── Datos (moneda) ───────────────────────────────────────────────────────

    public static int ObtenerDatos()
    {
        return PlayerPrefs.GetInt(KeyDatos(), 0);
    }

    public static void AgregarDatos(int cantidad)
    {
        int total = ObtenerDatos() + cantidad;
        PlayerPrefs.SetInt(KeyDatos(), total);
        PlayerPrefs.Save();
    }

    public static bool GastarDatos(int cantidad)
    {
        int total = ObtenerDatos();
        if (total < cantidad) return false;
        PlayerPrefs.SetInt(KeyDatos(), total - cantidad);
        PlayerPrefs.Save();
        return true;
    }

    // ─── Niveles de mejora ────────────────────────────────────────────────────

    public static int ObtenerNivel(string id)
    {
        return PlayerPrefs.GetInt(KeyNivel(id), 0);
    }

    /// <summary>
    /// Intenta comprar el siguiente nivel de la mejora.
    /// Retorna true si la compra fue exitosa.
    /// </summary>
    public static bool Mejorar(UpgradeData data)
    {
        int nivelActual = ObtenerNivel(data.id);

        if (nivelActual >= data.nivelMaximo)
        {
            Debug.Log($"[UpgradeSystem] {data.nombreMejora} ya está al máximo.");
            return false;
        }

        int costo = data.costosPorNivel[nivelActual];

        if (!GastarDatos(costo))
        {
            Debug.Log($"[UpgradeSystem] Datos insuficientes. Necesitas {costo}, tienes {ObtenerDatos()}.");
            return false;
        }

        PlayerPrefs.SetInt(KeyNivel(data.id), nivelActual + 1);
        PlayerPrefs.Save();

        Debug.Log($"[UpgradeSystem] {data.nombreMejora} → Nivel {nivelActual + 1}");
        return true;
    }

    /// <summary>
    /// Resetea todas las mejoras y devuelve el 50% de los datos gastados.
    /// Solo disponible en Modo Difícil.
    /// </summary>
    public static void ResetearMejoras(UpgradeData[] todasLasMejoras)
    {
        int datosDevueltos = 0;

        foreach (var data in todasLasMejoras)
        {
            int nivel = ObtenerNivel(data.id);
            for (int i = 0; i < nivel; i++)
                datosDevueltos += data.costosPorNivel[i];

            PlayerPrefs.DeleteKey(KeyNivel(data.id));
        }

        AgregarDatos(datosDevueltos / 2);
        PlayerPrefs.Save();
        Debug.Log($"[UpgradeSystem] Reset completo. Datos devueltos: {datosDevueltos / 2}");
    }
}
