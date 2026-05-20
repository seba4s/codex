using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

public static class AnimatorCompleteSetup
{
    const string SPRITE_BASE = "Assets/Sprites/UI/AssetsPersonajes/CODIGO-7/";
    const string CLIP_BASE   = "Assets/Movimiento/";
    const float  FPS         = 10f;

    [MenuItem("CODEX/Setup Animator/Crear Todo (Clips + Animator)")]
    public static void SetupAll()
    {
        AssetDatabase.Refresh();

        // ── Crear clips ────────────────────────────────────────────────
        string srPath = GetSpriteRendererPath();

        var clipIdle    = CreateClip("Idle",    "quieto.png",  srPath, loop: true);
        var clipSaltar  = CreateClip("Saltar",  "saltar.png",  srPath, loop: false);
        var clipDash    = CreateClip("Dash",    "dash.png",    srPath, loop: false);
        var clipHurt    = CreateClip("Hurt",    "daño.png",    srPath, loop: false);
        var clipDisparo = CreateClip("Disparo", "disparo.png", srPath, loop: false);

        // Correr: usa dash como placeholder si no existe correr.png
        AnimationClip clipCorrer = LoadExistingClip("correr.png", srPath)
                                ?? CreateClip("Correr", "dash.png", srPath, loop: true);

        if (clipIdle == null || clipSaltar == null)
        {
            EditorUtility.DisplayDialog("CODEX – Error",
                "No se encontraron quieto.png o saltar.png en:\n" + SPRITE_BASE +
                "\n\nAsegúrate de que Unity los haya importado.", "OK");
            return;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // ── Crear Animator Controller ──────────────────────────────────
        string ctrlPath = CLIP_BASE + "Player_Tutorial.controller";
        if (AssetDatabase.LoadAssetAtPath<AnimatorController>(ctrlPath) != null)
            AssetDatabase.DeleteAsset(ctrlPath);

        var ctrl = AnimatorController.CreateAnimatorControllerAtPath(ctrlPath);

        // Parámetros
        ctrl.AddParameter("Speed",      AnimatorControllerParameterType.Float);
        ctrl.AddParameter("IsGrounded", AnimatorControllerParameterType.Bool);
        ctrl.AddParameter("YVelocity",  AnimatorControllerParameterType.Float);
        ctrl.AddParameter("Jump",       AnimatorControllerParameterType.Trigger);
        ctrl.AddParameter("Dash",       AnimatorControllerParameterType.Trigger);
        ctrl.AddParameter("Hurt",       AnimatorControllerParameterType.Trigger);
        ctrl.AddParameter("Shoot",      AnimatorControllerParameterType.Trigger);

        var sm = ctrl.layers[0].stateMachine;

        // Estados
        var sIdle    = AddState(sm, "Idle",    clipIdle,    new Vector3(300,   0));
        var sCorrer  = AddState(sm, "Correr",  clipCorrer,  new Vector3(300, 120));
        var sSaltar  = AddState(sm, "Saltar",  clipSaltar,  new Vector3(300, 240));
        var sDash    = AddState(sm, "Dash",    clipDash,    new Vector3(550, 120));
        var sHurt    = AddState(sm, "Hurt",    clipHurt,    new Vector3(550,   0));
        var sDisparo = AddState(sm, "Disparo", clipDisparo, new Vector3(550, 240));

        sm.defaultState = sIdle;

        // Transiciones de movimiento
        AddTransition(sIdle,   sCorrer, "Speed", AnimatorConditionMode.Greater, 0.1f);
        AddTransition(sCorrer, sIdle,   "Speed", AnimatorConditionMode.Less,    0.1f);

        // Salto
        AddTransitionTrigger(sIdle,   sSaltar, "Jump");
        AddTransitionTrigger(sCorrer, sSaltar, "Jump");
        AddTransitionBool   (sSaltar, sIdle,   "IsGrounded", true);

        // Dash
        AddTransitionTrigger(sIdle,   sDash, "Dash");
        AddTransitionTrigger(sCorrer, sDash, "Dash");
        AddTransitionBool   (sDash,   sIdle, "IsGrounded", true);

        // Hurt
        AddTransitionTrigger(sIdle,   sHurt, "Hurt");
        AddTransitionTrigger(sCorrer, sHurt, "Hurt");
        AddTransitionTrigger(sSaltar, sHurt, "Hurt");
        var tHurtBack = sHurt.AddExitTransition();
        tHurtBack.hasExitTime = true;
        tHurtBack.exitTime    = 1f;
        tHurtBack.duration    = 0.05f;

        // Disparo (se puede disparar desde Idle o Correr; vuelve solo al terminar)
        AddTransitionTrigger(sIdle,   sDisparo, "Shoot");
        AddTransitionTrigger(sCorrer, sDisparo, "Shoot");
        var tDisparoBack = sDisparo.AddExitTransition();
        tDisparoBack.hasExitTime = true;
        tDisparoBack.exitTime    = 1f;
        tDisparoBack.duration    = 0.05f;

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Asignar al jugador (busca Animator en root y en hijos)
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            var anim = player.GetComponentInChildren<Animator>(true);
            if (anim == null)
                anim = player.AddComponent<Animator>();
            if (anim != null)
            {
                anim.runtimeAnimatorController = ctrl;
                EditorUtility.SetDirty(anim.gameObject);
            }
        }

        bool missingCorrer = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_BASE + "correr.png") == null;

        bool missingDisparo = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_BASE + "disparo.png") == null;

        EditorUtility.DisplayDialog("CODEX – Listo",
            "Clips y Animator configurados.\n\n" +
            "✓ Idle    (quieto.png)\n" +
            "✓ Saltar  (saltar.png)\n" +
            "✓ Dash    (dash.png)\n" +
            "✓ Hurt    (daño.png)\n" +
            (missingDisparo
                ? "⚠ Disparo: falta disparo.png"
                : "✓ Disparo (disparo.png)") + "\n" +
            (missingCorrer
                ? "⚠ Correr: falta correr.png — usando dash como placeholder."
                : "✓ Correr  (correr.png)") +
            "\n\nDale Play para probar.", "OK");
    }

    // ── Helpers ──────────────────────────────────────────────────────────

    static AnimationClip CreateClip(string clipName, string pngFile, string srPath, bool loop)
    {
        var sprites = AssetDatabase.LoadAllAssetsAtPath(SPRITE_BASE + pngFile)
                                   .OfType<Sprite>()
                                   .OrderBy(s => ExtractIndex(s.name))
                                   .ToArray();
        if (sprites.Length == 0) return null;

        var clip = new AnimationClip { frameRate = FPS };

        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        var binding = new EditorCurveBinding
        {
            type         = typeof(SpriteRenderer),
            path         = srPath,
            propertyName = "m_Sprite"
        };

        var keys = new ObjectReferenceKeyframe[sprites.Length];
        for (int i = 0; i < sprites.Length; i++)
        {
            keys[i] = new ObjectReferenceKeyframe
            {
                time  = i / FPS,
                value = sprites[i]
            };
        }

        AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);

        string path = CLIP_BASE + clipName + ".anim";
        if (AssetDatabase.LoadAssetAtPath<AnimationClip>(path) != null)
            AssetDatabase.DeleteAsset(path);

        AssetDatabase.CreateAsset(clip, path);
        return clip;
    }

    static AnimationClip LoadExistingClip(string pngFile, string srPath)
    {
        var sprites = AssetDatabase.LoadAllAssetsAtPath(SPRITE_BASE + pngFile)
                                   .OfType<Sprite>().ToArray();
        if (sprites.Length == 0) return null;
        return CreateClip("Correr", pngFile, srPath, loop: true);
    }

    static string GetSpriteRendererPath()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return "";
        var sr = player.GetComponentInChildren<SpriteRenderer>();
        if (sr == null || sr.gameObject == player) return "";

        string path = sr.gameObject.name;
        var parent = sr.transform.parent;
        while (parent != null && parent.gameObject != player)
        {
            path   = parent.gameObject.name + "/" + path;
            parent = parent.parent;
        }
        return path;
    }

    static int ExtractIndex(string name)
    {
        var parts = name.Split('_');
        return parts.Length > 0 && int.TryParse(parts[parts.Length - 1], out int n) ? n : 0;
    }

    static AnimatorState AddState(AnimatorStateMachine sm, string name,
                                   AnimationClip clip, Vector3 pos)
    {
        var s = sm.AddState(name, pos);
        s.motion = clip;
        return s;
    }

    static void AddTransition(AnimatorState from, AnimatorState to,
                               string param, AnimatorConditionMode mode, float threshold)
    {
        var t = from.AddTransition(to);
        t.hasExitTime = false;
        t.duration    = 0.05f;
        t.AddCondition(mode, threshold, param);
    }

    static void AddTransitionTrigger(AnimatorState from, AnimatorState to, string trigger)
    {
        var t = from.AddTransition(to);
        t.hasExitTime = false;
        t.duration    = 0.05f;
        t.AddCondition(AnimatorConditionMode.If, 0, trigger);
    }

    static void AddTransitionBool(AnimatorState from, AnimatorState to,
                                   string param, bool value)
    {
        var t = from.AddTransition(to);
        t.hasExitTime = false;
        t.duration    = 0.05f;
        t.AddCondition(value ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0, param);
    }
}
