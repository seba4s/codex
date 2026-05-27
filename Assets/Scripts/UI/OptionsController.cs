using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// Controlador de la escena de Opciones.
/// Maneja volumen de música, volumen de efectos, pantalla completa y resolución.
/// </summary>
public class OptionsController : MonoBehaviour
{
    [Header("Audio")]
    [Tooltip("AudioMixer principal del juego")]
    public AudioMixer audioMixer;

    [Tooltip("Slider de volumen de música (0 a 1)")]
    public Slider sliderMusica;

    [Tooltip("Slider de volumen de efectos (0 a 1)")]
    public Slider sliderEfectos;

    [Header("Pantalla")]
    [Tooltip("Toggle de pantalla completa")]
    public Toggle togglePantallaCompleta;

    [Tooltip("Dropdown de resolución")]
    public TMP_Dropdown dropdownResolucion;

    [Header("Navegación")]
    [Tooltip("Nombre de la escena del menú principal")]
    private const string ESCENA_MENU = "MainMenu";

    // Resoluciones disponibles
    private Resolution[] _resoluciones;

    // Claves PlayerPrefs
    private const string KEY_MUSICA   = "VolumenMusica";
    private const string KEY_EFECTOS  = "VolumenEfectos";
    private const string KEY_FULLSCREEN = "PantallaCompleta";
    private const string KEY_RESOLUCION = "IndiceResolucion";

    void Start()
    {
        CargarResoluciones();
        CargarOpciones();
    }

    // ─── Resoluciones ────────────────────────────────────────────────────────

    void CargarResoluciones()
    {
        if (dropdownResolucion == null) return;

        _resoluciones = Screen.resolutions;
        dropdownResolucion.ClearOptions();

        List<string> opciones = new List<string>();
        int indiceActual = 0;

        for (int i = 0; i < _resoluciones.Length; i++)
        {
            string opcion = $"{_resoluciones[i].width} x {_resoluciones[i].height} @ {_resoluciones[i].refreshRateRatio.numerator}Hz";
            opciones.Add(opcion);

            if (_resoluciones[i].width  == Screen.currentResolution.width &&
                _resoluciones[i].height == Screen.currentResolution.height)
            {
                indiceActual = i;
            }
        }

        dropdownResolucion.AddOptions(opciones);
        dropdownResolucion.SetValueWithoutNotify(PlayerPrefs.GetInt(KEY_RESOLUCION, indiceActual));
        dropdownResolucion.RefreshShownValue();
    }

    // ─── Cargar / Guardar ────────────────────────────────────────────────────

    void CargarOpciones()
    {
        // Volumen música
        float volMusica = PlayerPrefs.GetFloat(KEY_MUSICA, 0.8f);
        if (sliderMusica != null)
        {
            sliderMusica.value = volMusica;
            AplicarVolumenMusica(volMusica);
        }

        // Volumen efectos
        float volEfectos = PlayerPrefs.GetFloat(KEY_EFECTOS, 1f);
        if (sliderEfectos != null)
        {
            sliderEfectos.value = volEfectos;
            AplicarVolumenEfectos(volEfectos);
        }

        // Pantalla completa
        bool fullscreen = PlayerPrefs.GetInt(KEY_FULLSCREEN, 1) == 1;
        if (togglePantallaCompleta != null)
            togglePantallaCompleta.SetIsOnWithoutNotify(fullscreen);
    }

    void GuardarOpciones()
    {
        if (sliderMusica           != null) PlayerPrefs.SetFloat(KEY_MUSICA,    sliderMusica.value);
        if (sliderEfectos          != null) PlayerPrefs.SetFloat(KEY_EFECTOS,   sliderEfectos.value);
        if (togglePantallaCompleta != null) PlayerPrefs.SetInt(KEY_FULLSCREEN,  togglePantallaCompleta.isOn ? 1 : 0);
        // D3 GAP FIX: índice de resolución no se guardaba — al reiniciar GameBootstrap leía el valor viejo
        if (dropdownResolucion     != null) PlayerPrefs.SetInt(KEY_RESOLUCION,  dropdownResolucion.value);
        PlayerPrefs.Save();
#if UNITY_EDITOR
        Debug.Log("[Opciones] Opciones guardadas.");
#endif
    }

    // ─── Callbacks de UI ─────────────────────────────────────────────────────

    /// <summary>Asignar al evento OnValueChanged del Slider de música.</summary>
    public void OnCambiarVolumenMusica(float valor)
    {
        AplicarVolumenMusica(valor);
    }

    /// <summary>Asignar al evento OnValueChanged del Slider de efectos.</summary>
    public void OnCambiarVolumenEfectos(float valor)
    {
        AplicarVolumenEfectos(valor);
    }

    /// <summary>Asignar al evento OnValueChanged del Toggle de pantalla completa.</summary>
    public void OnCambiarPantallaCompleta(bool activo)
    {
        if (activo)
            CODEX.Systems.GameBootstrap.ApplyResolution();
        else
            Screen.SetResolution(1280, 720, FullScreenMode.Windowed);

        Debug.Log($"[Opciones] Pantalla completa: {activo}");
    }

    /// <summary>Asignar al evento OnValueChanged del Dropdown de resolución.</summary>
    public void OnCambiarResolucion(int indice)
    {
        if (_resoluciones == null || indice >= _resoluciones.Length) return;
        Resolution res = _resoluciones[indice];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
        Debug.Log($"[Opciones] Resolución: {res.width}x{res.height}");
    }

    /// <summary>Botón Guardar.</summary>
    public void OnGuardar()
    {
        GuardarOpciones();
    }

    /// <summary>Botón Volver → regresa al Menú Principal sin guardar.</summary>
    public void OnVolver()
    {
        SceneTransition.Instance.CargarEscena(ESCENA_MENU);
    }

    // ─── Helpers de audio ────────────────────────────────────────────────────

    void AplicarVolumenMusica(float valor)
    {
        if (audioMixer == null) return;
        // Convertir de lineal (0-1) a decibeles
        float db = valor > 0.0001f ? Mathf.Log10(valor) * 20f : -80f;
        audioMixer.SetFloat("VolumenMusica", db);
    }

    void AplicarVolumenEfectos(float valor)
    {
        if (audioMixer == null) return;
        float db = valor > 0.0001f ? Mathf.Log10(valor) * 20f : -80f;
        audioMixer.SetFloat("VolumenEfectos", db);
    }
}
