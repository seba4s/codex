using UnityEngine;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer))]
public class VideoPlayerDebug : MonoBehaviour
{
    private VideoPlayer vp;
    private float timer = 0f;
    private bool started = false;

    void Awake()
    {
        vp = GetComponent<VideoPlayer>();
        vp.errorReceived += OnError;
        vp.prepareCompleted += OnPrepared;
        vp.started += OnStarted;
        Debug.Log($"[VideoDebug] Awake — clip: {(vp.clip != null ? vp.clip.name : "NULL")} | url: '{vp.url}' | renderMode: {vp.renderMode} | targetTexture: {(vp.targetTexture != null ? vp.targetTexture.name : "NULL")} | playOnAwake: {vp.playOnAwake}");
    }

    void Start()
    {
        Debug.Log($"[VideoDebug] Start — isPlaying: {vp.isPlaying} | isPrepared: {vp.isPrepared}");
        // Forzar asignacion por URL como fallback
        if (vp.clip == null && string.IsNullOrEmpty(vp.url))
        {
            Debug.LogError("[VideoDebug] CLIP Y URL son NULL — asignando por URL directa");
            vp.source = VideoSource.Url;
            vp.url = System.IO.Path.Combine(Application.dataPath, "Sprites/UI/MainMenu/FONDOCODEX.mp4");
            Debug.Log($"[VideoDebug] URL asignada: {vp.url}");
        }
        vp.Prepare();
    }

    void OnPrepared(VideoPlayer source)
    {
        Debug.Log($"[VideoDebug] Prepared! width:{source.width} height:{source.height} — calling Play()");
        source.Play();
        started = true;
    }

    void OnStarted(VideoPlayer source)
    {
        Debug.Log("[VideoDebug] Started playing!");
    }

    void OnError(VideoPlayer source, string message)
    {
        Debug.LogError($"[VideoDebug] ERROR: {message}");
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (Time.frameCount % 60 == 0)
            Debug.Log($"[VideoDebug] t:{timer:F1}s isPlaying:{vp.isPlaying} isPrepared:{vp.isPrepared} frame:{vp.frame}");

        // Si a los 5 segundos sigue sin reproducir, forzar Play directo
        if (!started && timer > 5f)
        {
            started = true;
            Debug.LogWarning("[VideoDebug] Timeout — forzando Play() directo sin Prepare");
            vp.Play();
        }
    }
}
