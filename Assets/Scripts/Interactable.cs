using UnityEngine;
using UnityEngine.UI;

public class Interactable : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float interactionRange = 3f;
    [SerializeField] private LayerMask playerLayer = 1; // Default layer
    [SerializeField] private string interactionPrompt = "Press E to interact";
    
    [Header("UI References")]
    [SerializeField] private GameObject interactionUI;
    [SerializeField] private Text promptText;
    
    [Header("Visual Feedback")]
    [SerializeField] private bool showRangeGizmo = true;
    [SerializeField] private Color rangeColor = Color.cyan;
    
    private bool playerInRange = false;
    private Transform playerTransform;
    
    // Events
    public System.Action OnPlayerEnterRange;
    public System.Action OnPlayerExitRange;
    public System.Action OnInteractionTriggered;
    
    private void Start()
    {
        // Find player if not assigned
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerTransform = player.transform;
        }
        
        // Setup UI
        if (interactionUI != null)
        {
            interactionUI.SetActive(false);
            if (promptText != null)
                promptText.text = interactionPrompt;
        }
    }
    
    private void Update()
    {
        CheckPlayerDistance();
    }
    
    private void CheckPlayerDistance()
    {
        if (playerTransform == null) 
        {
            Debug.LogWarning($"[INTERACTION DEBUG] {gameObject.name}: No player transform assigned!");
            return;
        }
        
        float distance = Vector3.Distance(transform.position, playerTransform.position);
        bool wasInRange = playerInRange;
        
        // Debug distance every few frames
        if (Time.frameCount % 60 == 0) // Every 60 frames (about once per second at 60fps)
        {
            Debug.Log($"[INTERACTION DEBUG] {gameObject.name}: Player distance = {distance:F2}m, Range = {interactionRange:F2}m, InRange = {playerInRange}");
        }
        
        if (distance <= interactionRange)
        {
            if (!playerInRange)
            {
                playerInRange = true;
                Debug.Log($"[INTERACTION DEBUG] {gameObject.name}: Player ENTERED range at {Time.time:F2}s (distance: {distance:F2}m)");
                OnPlayerEnterRange?.Invoke();
                ShowInteractionUI(true);
            }
        }
        else
        {
            if (playerInRange)
            {
                playerInRange = false;
                Debug.Log($"[INTERACTION DEBUG] {gameObject.name}: Player EXITED range at {Time.time:F2}s (distance: {distance:F2}m)");
                OnPlayerExitRange?.Invoke();
                ShowInteractionUI(false);
            }
        }
    }
    
    public bool IsPlayerInRange()
    {
        return playerInRange;
    }
    
    public void TriggerInteraction()
    {
        Debug.Log($"[INTERACTION DEBUG] {gameObject.name}: TriggerInteraction called. PlayerInRange = {playerInRange}");
        
        if (playerInRange)
        {
            Debug.Log($"[INTERACTION DEBUG] {gameObject.name}: SUCCESS - Interaction triggered at {Time.time:F2}s!");
            OnInteractionTriggered?.Invoke();
        }
        else
        {
            Debug.LogWarning($"[INTERACTION DEBUG] {gameObject.name}: FAILED - Player not in range! Distance: {Vector3.Distance(transform.position, playerTransform?.position ?? Vector3.zero):F2}m");
        }
    }
    
    private void ShowInteractionUI(bool show)
    {
        if (interactionUI != null)
        {
            interactionUI.SetActive(show);
        }
    }
    
    // Public method to set the player transform (useful for dynamic assignment)
    public void SetPlayerTransform(Transform player)
    {
        playerTransform = player;
    }
    
    // Optional: Visual range indicator in the editor
    private void OnDrawGizmosSelected()
    {
        if (showRangeGizmo)
        {
            Gizmos.color = rangeColor;
            Gizmos.DrawWireSphere(transform.position, interactionRange);
        }
    }
    
    // Optional: Always show range when selected
    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        if (showRangeGizmo && UnityEditor.Selection.activeGameObject == gameObject)
        {
            Gizmos.color = rangeColor;
            Gizmos.DrawWireSphere(transform.position, interactionRange);
        }
#endif
    }
}
