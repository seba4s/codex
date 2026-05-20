using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.IO;

namespace CODEX.Editor
{
    /// <summary>
    /// Crea EnemyController.controller con los clips existentes del enemigo.
    /// Menú: CODEX > Tutorial > Conectar Animaciones Enemigo
    /// </summary>
    public static class T02_AnimationSetup
    {
        private const string ClipFolder     = "Assets/Animation/Enemy";
        private const string ControllerPath = "Assets/Animation/Enemy/EnemyController.controller";

        // Rutas de los clips (el usuario ya los creó manualmente)
        private const string ClipIdle  = "Assets/Animation/Enemy/Enemy_Idle.anim";
        private const string ClipRun   = "Assets/Animation/Enemy/Enemy_Run.anim";
        private const string ClipShoot = "Assets/Animation/Enemy/Enemy_Shoot.anim";
        private const string ClipDeath = "Assets/Animation/Enemy/Enemy_Death.anim";

        // El typo que dejó Unity al crearlo ("Idlet" en lugar de "Idle")
        private const string ClipIdleTypo = "Assets/Animation/Enemy/Enemy_Idlet.anim";

        [MenuItem("CODEX/Tutorial/Conectar Animaciones Enemigo")]
        public static void Setup()
        {
            // ── 1. Corregir typo del clip Idle ────────────────────────────
            FixIdleTypo();

            // ── 2. Cargar clips ────────────────────────────────────────────
            var idle  = AssetDatabase.LoadAssetAtPath<AnimationClip>(ClipIdle);
            var run   = AssetDatabase.LoadAssetAtPath<AnimationClip>(ClipRun);
            var shoot = AssetDatabase.LoadAssetAtPath<AnimationClip>(ClipShoot);
            var death = AssetDatabase.LoadAssetAtPath<AnimationClip>(ClipDeath);

            string missing = "";
            if (idle  == null) missing += $"\n• {ClipIdle}";
            if (run   == null) missing += $"\n• {ClipRun}";
            if (shoot == null) missing += $"\n• {ClipShoot}";
            if (death == null) missing += $"\n• {ClipDeath}";

            if (missing.Length > 0)
            {
                EditorUtility.DisplayDialog("Clips faltantes",
                    "No se encontraron estos clips:" + missing +
                    "\n\nVerifica que estén en Assets/Animation/Enemy/",
                    "OK");
                return;
            }

            // ── 3. Configurar loop ─────────────────────────────────────────
            SetLoop(idle,  true);
            SetLoop(run,   true);
            SetLoop(shoot, false);
            SetLoop(death, false);

            // ── 4. Crear/reemplazar AnimatorController ─────────────────────
            // Borrar el viejo para empezar limpio
            if (File.Exists(Path.GetFullPath(ControllerPath)))
                AssetDatabase.DeleteAsset(ControllerPath);

            var controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);

            // Parámetros
            controller.AddParameter("Speed",      AnimatorControllerParameterType.Float);
            controller.AddParameter("IsShooting", AnimatorControllerParameterType.Bool);
            controller.AddParameter("Die",        AnimatorControllerParameterType.Trigger);

            var sm = controller.layers[0].stateMachine;

            // Estados
            var stIdle  = sm.AddState("Idle",  new Vector3(250,   0));
            var stRun   = sm.AddState("Run",   new Vector3(250,  80));
            var stShoot = sm.AddState("Shoot", new Vector3(500,   0));
            var stDeath = sm.AddState("Death", new Vector3(500, 160));

            stIdle.motion  = idle;
            stRun.motion   = run;
            stShoot.motion = shoot;
            stDeath.motion = death;

            sm.defaultState = stIdle;

            // Idle → Run
            var t = stIdle.AddTransition(stRun);
            t.hasExitTime = false; t.duration = 0.05f;
            t.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");

            // Run → Idle
            t = stRun.AddTransition(stIdle);
            t.hasExitTime = false; t.duration = 0.05f;
            t.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");

            // Any → Shoot
            var anyShoot = sm.AddAnyStateTransition(stShoot);
            anyShoot.hasExitTime = false; anyShoot.duration = 0.05f;
            anyShoot.canTransitionToSelf = false;
            anyShoot.AddCondition(AnimatorConditionMode.If, 0, "IsShooting");

            // Shoot → Idle (al terminar el clip)
            t = stShoot.AddTransition(stIdle);
            t.hasExitTime = true; t.exitTime = 1f; t.duration = 0.05f;

            // Any → Death
            var anyDeath = sm.AddAnyStateTransition(stDeath);
            anyDeath.hasExitTime = false; anyDeath.duration = 0.05f;
            anyDeath.canTransitionToSelf = false;
            anyDeath.AddCondition(AnimatorConditionMode.If, 0, "Die");

            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // ── 5. Asignar controller al enemigo en la escena ──────────────
            int assigned = 0;
            foreach (var name in new[] { "InfectedFile_Blocker", "InfectedFile_Bloqueo" })
            {
                var enemy = GameObject.Find(name);
                if (enemy == null) continue;

                var anim = enemy.GetComponentInChildren<Animator>(true);
                if (anim == null) anim = enemy.AddComponent<Animator>();
                anim.runtimeAnimatorController = controller;
                EditorUtility.SetDirty(enemy);
                assigned++;
                Debug.Log($"[CODEX] EnemyController asignado a: {name}");
            }

            string assignedMsg = assigned > 0
                ? $"✓ Controller asignado a {assigned} enemigo(s) en la escena"
                : "⚠ No se encontró InfectedFile_Blocker/Bloqueo en la escena.\n  Asigna el controller manualmente al Animator.";

            EditorUtility.DisplayDialog("Animaciones conectadas",
                "EnemyController.controller creado con:\n" +
                "• Idle (loop)\n• Run (loop)\n• Shoot\n• Death\n\n" +
                "Parámetros: Speed (float), IsShooting (bool), Die (trigger)\n\n" +
                assignedMsg,
                "OK");
        }

        // ── Helpers ───────────────────────────────────────────────────────

        private static void FixIdleTypo()
        {
            // Si existe "Enemy_Idlet.anim" y no existe "Enemy_Idle.anim", renombrar
            bool typoExists  = File.Exists(Path.GetFullPath(ClipIdleTypo));
            bool correctExists = File.Exists(Path.GetFullPath(ClipIdle));

            if (typoExists && !correctExists)
            {
                var result = AssetDatabase.RenameAsset(ClipIdleTypo, "Enemy_Idle");
                if (string.IsNullOrEmpty(result))
                    Debug.Log("[CODEX] Enemy_Idlet.anim renombrado a Enemy_Idle.anim");
                else
                    Debug.LogWarning("[CODEX] No se pudo renombrar: " + result);
            }
        }

        private static void SetLoop(AnimationClip clip, bool loop)
        {
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            if (settings.loopTime == loop) return;
            settings.loopTime = loop;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            EditorUtility.SetDirty(clip);
        }
    }
}
