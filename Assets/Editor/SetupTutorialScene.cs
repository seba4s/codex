using UnityEngine;
using UnityEditor;

public static class SetupTutorialScene
{
    [MenuItem("Tools/Setup Tutorial Scene Structure")]
    public static void Setup()
    {
        // Root groups
        GameObject setup   = CreateEmpty("[SETUP]", Vector3.zero, null);
        GameObject bgs     = CreateEmpty("[BACKGROUNDS]", Vector3.zero, null);
        GameObject b1      = CreateEmpty("[BLOQUE_1] Materializacion y Movimiento", Vector3.zero, null);
        GameObject b2      = CreateEmpty("[BLOQUE_2] Disparo", Vector3.zero, null);
        GameObject b3      = CreateEmpty("[BLOQUE_3] Recoleccion Datos", Vector3.zero, null);
        GameObject b4      = CreateEmpty("[BLOQUE_4] Daño y Esquive", Vector3.zero, null);
        GameObject b5      = CreateEmpty("[BLOQUE_5] Terminal 1", Vector3.zero, null);
        GameObject b6      = CreateEmpty("[BLOQUE_6] Plataformas que Caen", Vector3.zero, null);
        GameObject b7      = CreateEmpty("[BLOQUE_7] Enemigos Combinados", Vector3.zero, null);
        GameObject b8      = CreateEmpty("[BLOQUE_8] Puerto de Salida", Vector3.zero, null);

        float blockWidth = 20f;

        // SETUP children
        CreateEmpty("Main Camera", V3(0, 0, -10), setup);
        CreateEmpty("EventSystem", Vector3.zero, setup);
        CreateEmpty("GameManagers", Vector3.zero, setup);

        // BACKGROUNDS
        string[] bgNames = { "BG_Bloque1","BG_Bloque2","BG_Bloque3","BG_Bloque4","BG_Bloque5","BG_Bloque6","BG_Bloque7" };
        for (int i = 0; i < bgNames.Length; i++)
        {
            GameObject bg = CreateEmpty(bgNames[i], V3(i * blockWidth + blockWidth * 0.5f, 0, 0), bgs);
            var sr = bg.AddComponent<SpriteRenderer>();
            sr.sortingOrder = -10;
        }

        // BLOQUE 1 - X: 0-20
        b1.transform.position = V3(blockWidth * 0, 0, 0);
        CreateEmpty("SpawnPoint_Player",   V3(2,  2, 0), b1);
        CreateEmpty("Plataforma_Inicio",   V3(5,  0, 0), b1);
        CreateEmpty("Plataforma_Salto_1",  V3(10, 1, 0), b1);
        CreateEmpty("Plataforma_Salto_2",  V3(15, 2, 0), b1);
        CreateEmpty("Checkpoint_1",        V3(18, 0, 0), b1);
        CreateEmpty("TriggerDialogo_LUMA", V3(0,  0, 0), b1);

        // BLOQUE 2 - X: 20-40
        b2.transform.position = V3(blockWidth * 1, 0, 0);
        CreateEmpty("Enemy_EstaticoBloqueaPuerta", V3(25, 1, 0), b2);
        CreateEmpty("Puerta_Bloqueada",            V3(27, 0, 0), b2);
        CreateEmpty("TriggerDialogo_Disparo",      V3(21, 0, 0), b2);
        CreateEmpty("Checkpoint_2",                V3(38, 0, 0), b2);

        // BLOQUE 3 - X: 40-60
        b3.transform.position = V3(blockWidth * 2, 0, 0);
        CreateEmpty("TriggerDialogo_Datos",        V3(41, 0, 0), b3);
        CreateEmpty("DataPickup_1",  V3(43, 1, 0), b3);
        CreateEmpty("DataPickup_2",  V3(44, 1, 0), b3);
        CreateEmpty("DataPickup_3",  V3(45, 1, 0), b3);
        CreateEmpty("DataPickup_4",  V3(46, 1, 0), b3);
        CreateEmpty("DataPickup_5",  V3(47, 1, 0), b3);
        CreateEmpty("DataPickup_Plataforma_1", V3(50, 3, 0), b3);
        CreateEmpty("DataPickup_Plataforma_2", V3(53, 4, 0), b3);
        CreateEmpty("DataPickup_Plataforma_3", V3(56, 5, 0), b3);
        CreateEmpty("DataPickup_Dificil",      V3(58, 6, 0), b3);
        CreateEmpty("Plataforma_Media",        V3(50, 2, 0), b3);
        CreateEmpty("Plataforma_Alta",         V3(53, 3, 0), b3);
        CreateEmpty("Plataforma_Saliente",     V3(57, 5, 0), b3);

        // BLOQUE 4 - X: 60-80
        b4.transform.position = V3(blockWidth * 3, 0, 0);
        CreateEmpty("TriggerDialogo_Daño",    V3(61, 0, 0), b4);
        CreateEmpty("Enemy_Dispara",          V3(67, 1, 0), b4);
        CreateEmpty("Trampa_Suelo_1",         V3(70, 0, 0), b4);
        CreateEmpty("Trampa_Suelo_2",         V3(73, 0, 0), b4);
        CreateEmpty("Checkpoint_3_post_dano", V3(78, 0, 0), b4);

        // BLOQUE 5 - X: 80-100
        b5.transform.position = V3(blockWidth * 4, 0, 0);
        CreateEmpty("TriggerDialogo_Terminal", V3(81, 0, 0), b5);
        CreateEmpty("Enemy_Terminal_1",        V3(85, 1, 0), b5);
        CreateEmpty("Enemy_Terminal_2",        V3(87, 1, 0), b5);
        CreateEmpty("Terminal_01",             V3(90, 1, 0), b5);
        CreateEmpty("Puerta_Sector",           V3(95, 0, 0), b5);
        CreateEmpty("Checkpoint_3",            V3(98, 0, 0), b5);

        // BLOQUE 6 - X: 100-120
        b6.transform.position = V3(blockWidth * 5, 0, 0);
        CreateEmpty("TriggerDialogo_Plataformas", V3(101, 0, 0), b6);
        CreateEmpty("Plataforma_Cae_1", V3(104, 1, 0), b6);
        CreateEmpty("Plataforma_Cae_2", V3(107, 1, 0), b6);
        CreateEmpty("Plataforma_Cae_3", V3(110, 2, 0), b6);
        CreateEmpty("Plataforma_Cae_4", V3(113, 2, 0), b6);
        CreateEmpty("Plataforma_Firme", V3(116, 1, 0), b6);
        CreateEmpty("DataPickup_Opcional_x5", V3(118, 3, 0), b6);
        CreateEmpty("Checkpoint_4",    V3(119, 0, 0), b6);

        // BLOQUE 7 - X: 120-140
        b7.transform.position = V3(blockWidth * 6, 0, 0);
        CreateEmpty("TriggerDialogo_Corredor", V3(121, 0, 0), b7);
        CreateEmpty("Enemy_TipoA_1",  V3(124, 1, 0), b7);
        CreateEmpty("Enemy_TipoB_1",  V3(128, 1, 0), b7);
        CreateEmpty("Enemy_TipoC_1",  V3(132, 2, 0), b7);
        CreateEmpty("Enemy_TipoA_2",  V3(135, 1, 0), b7);
        CreateEmpty("DataPickups_x8", V3(129, 0, 0), b7);
        CreateEmpty("Tramo_Respiro",  V3(136, 0, 0), b7);
        CreateEmpty("Terminal_02",    V3(138, 1, 0), b7);
        CreateEmpty("Checkpoint_5",   V3(139, 0, 0), b7);

        // BLOQUE 8 - X: 140-160
        b8.transform.position = V3(blockWidth * 7, 0, 0);
        CreateEmpty("TriggerDialogo_Final", V3(141, 0, 0), b8);
        CreateEmpty("Terminal_03",          V3(145, 1, 0), b8);
        CreateEmpty("ArchivoKeySebastion",  V3(150, 2, 0), b8);
        CreateEmpty("PuertoSalida_USB",     V3(155, 1, 0), b8);

        Debug.Log("Estructura del tutorial creada. Asigna los prefabs a cada placeholder.");
        EditorUtility.DisplayDialog("Listo",
            "Estructura creada.\n\nAhora:\n1. Asigna fondo1-7 a los BG_BloqueX\n2. Reemplaza los GameObjects vacios con tus prefabs\n3. Agrega colliders/plataformas segun el diseño",
            "OK");
    }

    static GameObject CreateEmpty(string name, Vector3 pos, GameObject parent)
    {
        GameObject go = new GameObject(name);
        go.transform.position = pos;
        if (parent != null)
            go.transform.SetParent(parent.transform);
        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        return go;
    }

    static Vector3 V3(float x, float y, float z) => new Vector3(x, y, z);
}
