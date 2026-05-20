using UnityEngine;
using UnityEngine.Events;

namespace CODEX.Systems
{
    public class DataCollectible : MonoBehaviour
    {
        [System.Serializable]
        public class DataCollectionEvent : UnityEvent<int> { }
        
        [Header("Configuración del Dato")]
        [SerializeField] private int dataValue = 1;
        [SerializeField] private float collectRadius = 0.5f;
        [SerializeField] private LayerMask playerLayer;
        
        [Header("Efectos Visuales")]
        [SerializeField] private GameObject collectEffectPrefab;
        [SerializeField] private float floatAmplitude = 0.1f;
        [SerializeField] private float floatSpeed = 2f;
        [SerializeField] private float rotationSpeed = 90f;
        
        [Header("Audio")]
        [SerializeField] private AudioClip collectSound;
#pragma warning disable CS0414
        [SerializeField] private float collectVolume = 0.7f;
#pragma warning restore CS0414
        
        [Header("Estado")]
        private Vector3 startPosition;
        private bool isCollected;
        private Transform visualPart;
        private DataManager dataManager;
        
        public int DataValue => dataValue;
        public bool IsCollected => isCollected;
        
        private void Awake()
        {
            startPosition = transform.position;
            
            // Buscar la parte visual
            foreach (Transform child in transform)
            {
                if (child.name == "Visual" || child.GetComponent<SpriteRenderer>() != null)
                {
                    visualPart = child;
                    break;
                }
            }
            
            // Buscar DataManager
            dataManager = FindAnyObjectByType<DataManager>();
            if (dataManager == null)
            {
                GameObject managerObj = new GameObject("DataManager");
                dataManager = managerObj.AddComponent<DataManager>();
            }
        }
        
        private void Start()
        {
            if (isCollected)
            {
                gameObject.SetActive(false);
            }
        }
        
        private void Update()
        {
            if (!isCollected && visualPart != null)
            {
                // Animación de flotación
                float yOffset = Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
                visualPart.localPosition = new Vector3(0, yOffset, 0);
                
                // Rotación suave
                visualPart.Rotate(0, 0, rotationSpeed * Time.deltaTime);
            }
        }
        
        private void FixedUpdate()
        {
            if (isCollected) return;
            
            // Chequear si el jugador está cerca
            Collider2D player = Physics2D.OverlapCircle(transform.position, collectRadius, playerLayer);
            if (player != null)
            {
                Collect(player.gameObject);
            }
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isCollected) return;
            
            if ((playerLayer.value & (1 << other.gameObject.layer)) != 0)
            {
                Collect(other.gameObject);
            }
        }
        
        public void Collect(GameObject collector)
        {
            if (isCollected) return;
            
            isCollected = true;
            
            // Notificar al DataManager local
            if (dataManager != null)
                dataManager.AddData(dataValue);

            // Notificar al TutorialManager global
            CODEX.Tutorial.TutorialManager.Instance?.AddData(dataValue);
            
            // Efecto visual
            if (collectEffectPrefab != null)
            {
                Instantiate(collectEffectPrefab, transform.position, Quaternion.identity);
            }
            
            // Sonido
            if (collectSound != null)
            {
                // TODO: Implementar sistema de audio
                // AudioSource.PlayClipAtPoint(collectSound, transform.position, collectVolume);
            }
            
            // Desactivar o destruir
            gameObject.SetActive(false);
            
            // Log para depuración
            Debug.Log($"Dato recolectado! Valor: {dataValue}");
        }
        
        public void ResetCollectible()
        {
            isCollected = false;
            gameObject.SetActive(true);
            transform.position = startPosition;
        }
        
        public void SetDataValue(int newValue)
        {
            dataValue = newValue;
        }
        
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, collectRadius);
        }
    }
    
    public class DataManager : MonoBehaviour
    {
        [System.Serializable]
        public class DataUpdateEvent : UnityEvent<int, int> { } // current, total
        
        [Header("Configuración de Datos")]
        [SerializeField] private int totalDataRequired = 80;
        [SerializeField] private int currentData = 0;
        
        [Header("Narrativa")]
        [SerializeField, TextArea(2, 5)] private string[] narrativeMessages;
        [SerializeField] private int[] narrativeThresholds = { 10, 25, 50, 75, 80 };
        private int lastNarrativeThreshold = 0;
        
        [Header("Eventos")]
        public DataUpdateEvent OnDataUpdated = new DataUpdateEvent();
        public UnityEvent OnDataGoalReached = new UnityEvent();
        public UnityEvent<string> OnNarrativeMessage = new UnityEvent<string>();
        
        // === PROPIEDADES PÚBLICAS ===
        public int CurrentData => currentData;
        public int TotalRequired => totalDataRequired;
        public float ProgressPercentage => (float)currentData / totalDataRequired * 100f;
        public bool GoalReached => currentData >= totalDataRequired;
        
        private void Start()
        {
            // Actualizar UI inicial
            OnDataUpdated.Invoke(currentData, totalDataRequired);
        }
        
        // === MÉTODOS PÚBLICOS ===
        
        public void AddData(int amount = 1)
        {
            if (GoalReached) return;
            
            currentData = Mathf.Min(totalDataRequired, currentData + amount);
            
            // Disparar eventos
            OnDataUpdated.Invoke(currentData, totalDataRequired);
            
            // Chequear mensajes narrativos
            CheckNarrativeMessages();
            
            // Chequear si se alcanzó el objetivo
            if (GoalReached)
            {
                OnDataGoalReached.Invoke();
                OnNarrativeMessage.Invoke($"<color=#00FFFF>Objetivo cumplido! Has recolectado {totalDataRequired} datos.</color>");
            }
        }
        
        public void RemoveData(int amount = 1)
        {
            currentData = Mathf.Max(0, currentData - amount);
            OnDataUpdated.Invoke(currentData, totalDataRequired);
        }
        
        public void ResetData()
        {
            currentData = 0;
            lastNarrativeThreshold = 0;
            OnDataUpdated.Invoke(currentData, totalDataRequired);
        }
        
        public void SetData(int amount)
        {
            currentData = Mathf.Clamp(amount, 0, totalDataRequired);
            OnDataUpdated.Invoke(currentData, totalDataRequired);
            
            CheckNarrativeMessages();
        }
        
        public void SetGoal(int newGoal)
        {
            totalDataRequired = Mathf.Max(1, newGoal);
            OnDataUpdated.Invoke(currentData, totalDataRequired);
            
            if (GoalReached)
            {
                OnDataGoalReached.Invoke();
            }
        }
        
        // === MÉTODOS DE NARRATIVA ===
        
        private void CheckNarrativeMessages()
        {
            if (narrativeMessages == null || narrativeMessages.Length == 0) return;
            if (narrativeThresholds == null || narrativeThresholds.Length == 0) return;
            
            for (int i = narrativeThresholds.Length - 1; i >= 0; i--)
            {
                if (currentData >= narrativeThresholds[i] && lastNarrativeThreshold < narrativeThresholds[i])
                {
                    lastNarrativeThreshold = narrativeThresholds[i];
                    
                    // Mostrar mensaje narrativo
                    int messageIndex = Mathf.Min(i, narrativeMessages.Length - 1);
                    string message = narrativeMessages[messageIndex];
                    OnNarrativeMessage.Invoke(message);
                    
                    break;
                }
            }
        }
        
        public void SetNarrativeMessages(string[] messages, int[] thresholds)
        {
            narrativeMessages = messages;
            narrativeThresholds = thresholds;
            lastNarrativeThreshold = 0;
        }
        
        // === MÉTODOS PARA ARRAYS DE OBJETOS ===
        
        public DataCollectible[] FindAllDataInScene()
        {
            return FindObjectsByType<DataCollectible>();
        }
        
        public int CountCollectedData()
        {
            DataCollectible[] allData = FindAllDataInScene();
            int collected = 0;
            
            foreach (var data in allData)
            {
                if (data.IsCollected)
                {
                    collected++;
                }
            }
            
            return collected;
        }
        
        public int CountTotalDataInScene()
        {
            DataCollectible[] allData = FindAllDataInScene();
            int total = 0;
            
            foreach (var data in allData)
            {
                total += data.DataValue;
            }
            
            return total;
        }
        
        // === MÉTODOS DE DEPURACIÓN ===
        
        private void OnGUI()
        {
            if (Application.isEditor)
            {
                GUILayout.BeginArea(new Rect(Screen.width - 210, 10, 200, 150));
                GUILayout.Label("<b>SISTEMA DE DATOS</b>", new GUIStyle(GUI.skin.label) { richText = true });
                
                GUILayout.Label($"Datos: {currentData}/{totalDataRequired}");
                GUILayout.Label($"{ProgressPercentage:F1}% completado");
                
                GUILayout.Label($"Objetivo: {(GoalReached ? "<color=green>COMPLETADO</color>" : "<color=yellow>ESPERANDO</color>")}", 
                              new GUIStyle(GUI.skin.label) { richText = true });
                
                if (GUILayout.Button("+10 Datos"))
                {
                    AddData(10);
                }
                
                if (GUILayout.Button("Reset"))
                {
                    ResetData();
                }
                
                GUILayout.EndArea();
            }
        }
        
        private void OnDrawGizmos()
        {
            // Visualización del progreso en editor (opcional)
        }
    }
}