using UnityEngine;
using UnityEditor;

namespace CODEX.Editor
{
    /// <summary>
    /// Crea el prefab de CODIGO-7 y el prefab del proyectil desde cero.
    /// Menú: CODEX > Crear Prefabs del Jugador
    /// </summary>
    public static class PlayerPrefabCreator
    {
        [MenuItem("CODEX/Crear Prefabs del Jugador")]
        public static void CreateAll()
        {
            var projectilePrefab = CreateProjectilePrefab();
            CreatePlayerPrefab(projectilePrefab);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Prefabs creados",
                "Se crearon en:\n\n" +
                "• Assets/Prefabs/Player/CODIGO7.prefab\n" +
                "• Assets/Prefabs/Projectiles/Proyectil_Purga.prefab\n\n" +
                "El proyectil ya está asignado al ShootingSystem del jugador.\n\n" +
                "PENDIENTE:\n" +
                "• En el prefab del jugador, asigna un Sprite en SpriteRenderer\n" +
                "• Verifica que los layers 'Player', 'ground' y 'Enemy' existen en Project Settings > Tags & Layers",
                "OK");
        }

        // ─────────────────────────────────────────────────────────────────
        //  PROYECTIL DE PURGA
        // ─────────────────────────────────────────────────────────────────
        private static GameObject CreateProjectilePrefab()
        {
            var go = new GameObject("Proyectil_Purga");

            // Visual: cuadrado cian pequeño (placeholder hasta tener sprite)
            var sr = go.AddComponent<SpriteRenderer>();
            sr.color = new Color(0f, 1f, 0.9f);   // cian
            sr.sprite = MakeSquareSprite();
            go.transform.localScale = new Vector3(0.2f, 0.2f, 1f);

            // Física
            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.5f;
            col.isTrigger = true;

            // Lógica
            go.AddComponent<CODEX.Systems.Projectile>();

            // Layer "Projectile" si existe, si no usa Default
            int projLayer = LayerMask.NameToLayer("Projectile");
            go.layer = projLayer >= 0 ? projLayer : 0;

            // Guardar como prefab
            string path = "Assets/Prefabs/Projectiles/Proyectil_Purga.prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);

            Debug.Log($"[PlayerPrefabCreator] Proyectil creado: {path}");
            return prefab;
        }

        // ─────────────────────────────────────────────────────────────────
        //  JUGADOR: CODIGO-7
        // ─────────────────────────────────────────────────────────────────
        private static void CreatePlayerPrefab(GameObject projectilePrefab)
        {
            // ── Raíz ──────────────────────────────────────────────────────
            var root = new GameObject("CODIGO7");
            root.tag = "Player";
            int playerLayer = LayerMask.NameToLayer("Player");
            root.layer = playerLayer >= 0 ? playerLayer : 0;

            // Rigidbody2D
            var rb = root.AddComponent<Rigidbody2D>();
            rb.gravityScale = 3f;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;

            // Collider del cuerpo — cápsula para mejor movimiento
            var col = root.AddComponent<CapsuleCollider2D>();
            col.size = new Vector2(0.6f, 1.8f);
            col.offset = new Vector2(0f, 0f);

            // ── Visual (hijo "Body") ──────────────────────────────────────
            var body = new GameObject("Body");
            body.transform.SetParent(root.transform);
            body.transform.localPosition = Vector3.zero;

            var sr = body.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 1;

            // Cargar el primer frame idle de CODIGO-7 (2048x512, celdas 256x256)
            var codigo7Assets = AssetDatabase.LoadAllAssetsAtPath(
                "Assets/Sprites/UI/AssetsPersonajes/CODIGO-7/CODIGO-7.png");
            Sprite idleSprite = null;
            foreach (var asset in codigo7Assets)
                if (asset is Sprite sp) { idleSprite = sp; break; }

            if (idleSprite != null)
            {
                sr.sprite = idleSprite;
                sr.color  = Color.white;
            }
            else
            {
                sr.sprite = MakeSquareSprite();
                sr.color  = new Color(0.3f, 0.7f, 1f);
                Debug.LogWarning("[PlayerPrefabCreator] CODIGO-7.png no está cortado aún. " +
                                 "Ejecuta CODEX > Configurar Sprites primero.");
            }

            // ── Gafas (hijo "Goggles") — para el feedback de salud ────────
            var goggles = new GameObject("Goggles");
            goggles.transform.SetParent(body.transform);
            goggles.transform.localPosition = new Vector3(0f, 0.35f, 0f);
            goggles.transform.localScale = new Vector3(0.8f, 0.2f, 1f);

            var gogglesSR = goggles.AddComponent<SpriteRenderer>();
            gogglesSR.color = new Color(0.2f, 0.6f, 1f);
            gogglesSR.sprite = MakeSquareSprite();
            gogglesSR.sortingOrder = 2;

            // ── Animator (vacío, para conectar después) ───────────────────
            var animator = root.AddComponent<Animator>();

            // ── Punto de detección de suelo ───────────────────────────────
            var groundCheck = new GameObject("GroundCheck");
            groundCheck.transform.SetParent(root.transform);
            groundCheck.transform.localPosition = new Vector3(0f, -0.95f, 0f); // fondo del capsule

            // ── Punto de disparo ──────────────────────────────────────────
            var firePoint = new GameObject("FirePoint");
            firePoint.transform.SetParent(root.transform);
            firePoint.transform.localPosition = new Vector3(0.5f, 0.2f, 0f);

            // ── PlayerController ──────────────────────────────────────────
            var pc = root.AddComponent<CODEX.Player.PlayerController>();
            var pcSO = new SerializedObject(pc);
            pcSO.FindProperty("groundCheckPoint").objectReferenceValue = groundCheck.transform;
            pcSO.FindProperty("groundCheckRadius").floatValue = 0.15f;
            pcSO.FindProperty("groundCheckOffset").vector2Value = new Vector2(0f, -0.95f);
            pcSO.FindProperty("groundLayer").intValue = GetGroundLayerMask();
            pcSO.FindProperty("moveSpeed").floatValue = 8f;
            pcSO.FindProperty("jumpForce").floatValue = 14f;
            pcSO.FindProperty("dashSpeed").floatValue = 20f;
            pcSO.FindProperty("showDebugLogs").boolValue = true;
            pcSO.FindProperty("animator").objectReferenceValue = animator;
            pcSO.ApplyModifiedProperties();

            // ── ShootingSystem ────────────────────────────────────────────
            var ss = root.AddComponent<CODEX.Player.ShootingSystem>();
            var ssSO = new SerializedObject(ss);
            ssSO.FindProperty("projectilePrefab").objectReferenceValue = projectilePrefab;
            ssSO.FindProperty("firePoint").objectReferenceValue = firePoint.transform;
            ssSO.FindProperty("fireRate").floatValue = 0.15f;
            ssSO.FindProperty("projectileSpeed").floatValue = 20f;
            ssSO.FindProperty("projectileDamage").intValue = 1;
            ssSO.FindProperty("targetLayers").intValue = GetEnemyLayerMask();
            ssSO.ApplyModifiedProperties();

            // ── PlayerHealth ──────────────────────────────────────────── // REFACTOR: era HealthSystem
            var ph = root.AddComponent<CODEX.Player.PlayerHealth>();
            var phSO = new SerializedObject(ph);
            phSO.FindProperty("gogglesRenderer").objectReferenceValue = gogglesSR;
            phSO.ApplyModifiedProperties();

            // ── AudioSource (para disparos) ───────────────────────────────
            root.AddComponent<AudioSource>();

            // ── Guardar prefab ────────────────────────────────────────────
            string path = "Assets/Prefabs/Player/CODIGO7.prefab";
            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);

            Debug.Log($"[PlayerPrefabCreator] Jugador creado: {path}");
        }

        // ─────────────────────────────────────────────────────────────────
        //  HELPERS
        // ─────────────────────────────────────────────────────────────────

        private static Sprite MakeSquareSprite()
        {
            return CODEX.Editor.EditorSpriteHelper.GetWhiteSprite();
        }

        private static int GetGroundLayerMask()
        {
            int idx = LayerMask.NameToLayer("ground");
            return idx >= 0 ? (1 << idx) : 1;
        }

        private static int GetEnemyLayerMask()
        {
            int idx = LayerMask.NameToLayer("Enemy");
            return idx >= 0 ? (1 << idx) : 1;
        }
    }
}
