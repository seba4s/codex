using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

public static class AnimatorSetup
{
    [MenuItem("CODEX/Setup Animator/Configurar Animator del Jugador")]
    public static void Setup()
    {
        // 1. Cargar clips
        var clipIdle   = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Movimiento/Idle.anim");
        var clipCorrer = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Movimiento/Correr.anim");
        var clipSaltar = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Movimiento/Saltar.anim");

        if (clipIdle == null || clipCorrer == null || clipSaltar == null)
        {
            EditorUtility.DisplayDialog("CODEX – Error",
                "No se encontraron los clips en Assets/Movimiento/.\n\n" +
                "Asegúrate de que existan:\n• Idle.anim\n• Correr.anim\n• Saltar.anim",
                "OK");
            return;
        }

        // 2. Crear o sobreescribir el Animator Controller
        string path = "Assets/Movimiento/Player_Tutorial.controller";
        var controller = AnimatorController.CreateAnimatorControllerAtPath(path);

        // 3. Parámetros
        controller.AddParameter("Speed",      AnimatorControllerParameterType.Float);
        controller.AddParameter("IsGrounded", AnimatorControllerParameterType.Bool);
        controller.AddParameter("YVelocity",  AnimatorControllerParameterType.Float);
        controller.AddParameter("Jump",       AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Dash",       AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Hurt",       AnimatorControllerParameterType.Trigger);

        var root = controller.layers[0].stateMachine;

        // 4. Estados
        var stateIdle   = root.AddState("Idle",   new Vector3(250, 0,   0));
        var stateCorrer = root.AddState("Correr", new Vector3(250, 120, 0));
        var stateSaltar = root.AddState("Saltar", new Vector3(250, 240, 0));

        stateIdle.motion   = clipIdle;
        stateCorrer.motion = clipCorrer;
        stateSaltar.motion = clipSaltar;

        root.defaultState = stateIdle;

        // 5. Transiciones

        // Idle → Correr  (Speed > 0.1)
        var t1 = stateIdle.AddTransition(stateCorrer);
        t1.hasExitTime = false;
        t1.duration    = 0.05f;
        t1.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");

        // Correr → Idle  (Speed < 0.1)
        var t2 = stateCorrer.AddTransition(stateIdle);
        t2.hasExitTime = false;
        t2.duration    = 0.05f;
        t2.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");

        // Idle → Saltar  (Jump trigger)
        var t3 = stateIdle.AddTransition(stateSaltar);
        t3.hasExitTime = false;
        t3.duration    = 0.05f;
        t3.AddCondition(AnimatorConditionMode.If, 0, "Jump");

        // Correr → Saltar  (Jump trigger)
        var t4 = stateCorrer.AddTransition(stateSaltar);
        t4.hasExitTime = false;
        t4.duration    = 0.05f;
        t4.AddCondition(AnimatorConditionMode.If, 0, "Jump");

        // Saltar → Idle  (IsGrounded = true)
        var t5 = stateSaltar.AddTransition(stateIdle);
        t5.hasExitTime = false;
        t5.duration    = 0.05f;
        t5.AddCondition(AnimatorConditionMode.If, 0, "IsGrounded");

        // 6. Guardar
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // 7. Asignar al jugador en la escena
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            var animator = player.GetComponent<Animator>();
            if (animator == null)
                animator = player.AddComponent<Animator>();

            animator.runtimeAnimatorController = controller;
            EditorUtility.SetDirty(player);
        }

        EditorUtility.DisplayDialog("CODEX – Animator Listo",
            "Animator configurado correctamente.\n\n" +
            "Estados: Idle → Correr → Saltar\n" +
            "Parámetros: Speed, IsGrounded, YVelocity, Jump, Dash, Hurt\n\n" +
            "Dale Play para probar.", "OK");
    }
}
