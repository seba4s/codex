using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransition : MonoBehaviour
{
    public static SceneTransition Instance { get; private set; }

    [Header("Referencias")]
    [Tooltip("RawImage que cubre toda la pantalla para el efecto de pixelación")]
    public RawImage panelGlitch;

    [Tooltip("Panel negro para fade")]
    public Image panelOverlay;

    [Header("Configuración")]
    public float duracion = 1.0f;
    public int pasosPixelacion = 8;

    private bool _enTransicion = false;
    private RenderTexture _rt;
    private Camera _cam;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }

        // Forzar que sea root
        if (transform.parent != null)
            transform.SetParent(null);

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SetAlpha(panelOverlay, 0f);
        if (panelGlitch != null)
        {
            panelGlitch.gameObject.SetActive(false);
            panelGlitch.raycastTarget = false;
        }
    }

    public void CargarEscena(string nombreEscena)
    {
        if (_enTransicion) return;
        StartCoroutine(TransicionPixelGlitch(nombreEscena));
    }

    public void CargarEscena(int indiceEscena)
    {
        if (_enTransicion) return;
        StartCoroutine(TransicionPixelGlitch(indiceEscena));
    }

    private IEnumerator TransicionPixelGlitch(object escena)
    {
        Debug.Log($"[SceneTransition] Iniciando transición → {escena}");
        _enTransicion = true;

        _cam = Camera.main;

        _rt = new RenderTexture(Screen.width, Screen.height, 24);
        _cam.targetTexture = _rt;
        _cam.Render();
        _cam.targetTexture = null;

        if (panelGlitch != null)
        {
            panelGlitch.texture = _rt;
            panelGlitch.color = Color.white;
            panelGlitch.gameObject.SetActive(true);
            panelGlitch.raycastTarget = false;
        }

        float tiempoFase1 = duracion * 0.5f;
        float intervalo = tiempoFase1 / pasosPixelacion;

        int[] tamanos = { 4, 8, 12, 20, 32, 48, 72, 96, 128 };

        for (int i = 0; i < tamanos.Length; i++)
        {
            AplicarPixelacion(tamanos[i]);
            if (panelGlitch != null)
            {
                panelGlitch.rectTransform.anchoredPosition = new Vector2(
                    Random.Range(-8f, 8f),
                    Random.Range(-8f, 8f)
                );
            }
            yield return new WaitForSeconds(intervalo);
        }

        float t = 0f;
        float tiempoFade = duracion * 0.25f;
        while (t < tiempoFade)
        {
            t += Time.deltaTime;
            SetAlpha(panelOverlay, Mathf.Clamp01(t / tiempoFade));
            yield return null;
        }
        SetAlpha(panelOverlay, 1f);

        if (panelGlitch != null) panelGlitch.gameObject.SetActive(false);
        if (_rt != null) _rt.Release();

        if (escena is string nombre) SceneManager.LoadScene(nombre);
        else if (escena is int indice) SceneManager.LoadScene(indice);

        yield return new WaitForSeconds(0.1f);

        t = 0f;
        while (t < tiempoFade * 1.5f)
        {
            t += Time.deltaTime;
            SetAlpha(panelOverlay, 1f - Mathf.Clamp01(t / (tiempoFade * 1.5f)));
            yield return null;
        }
        SetAlpha(panelOverlay, 0f);

        _enTransicion = false;
    }

    private void AplicarPixelacion(int tamanoBloque)
    {
        if (_rt == null || panelGlitch == null) return;

        int w = Mathf.Max(1, Screen.width / tamanoBloque);
        int h = Mathf.Max(1, Screen.height / tamanoBloque);

        RenderTexture temp = RenderTexture.GetTemporary(w, h, 0);
        temp.filterMode = FilterMode.Point;

        // Hacer shrink a baja resolución
        Graphics.Blit(_rt, temp);
        // Volver a pantalla completa (point filter para mantener pixelado)
        Graphics.Blit(temp, _rt);

        panelGlitch.texture = _rt;
        if (tamanoBloque >= 64) panelGlitch.color = new Color(0.8f, 1f, 1f, 1f); // Tinte azulado

        RenderTexture.ReleaseTemporary(temp);
    }

    private void SetAlpha(Image img, float alpha)
    {
        if (img == null) return;
        Color c = img.color;
        c.a = alpha;
        img.color = c;
    }
}