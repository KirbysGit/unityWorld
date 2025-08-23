using UnityEngine;
using UnityEngine.UI;

public class InteractionDebugger : MonoBehaviour
{
    [Header("Debug Settings")]
    [SerializeField] private bool showDebugInfo = true;
    [SerializeField] private bool showOnScreen = true;
    [SerializeField] private KeyCode toggleKey = KeyCode.F1;
    
    [Header("UI References")]
    [SerializeField] private GameObject debugUI;
    [SerializeField] private Text debugText;
    
    private InteractionManager interactionManager;
    private Interactable[] allInteractables;
    private Door[] allDoors;
    
    private void Start()
    {
        // Find the interaction manager
        interactionManager = FindObjectOfType<InteractionManager>();
        
        // Find all interactables and doors in the scene
        allInteractables = FindObjectsOfType<Interactable>();
        allDoors = FindObjectsOfType<Door>();
        
        Debug.Log($"[DEBUGGER] Found {allInteractables.Length} interactables and {allDoors.Length} doors in scene");
        
        // Setup debug UI
        if (showOnScreen && debugUI != null)
        {
            debugUI.SetActive(true);
        }
    }
    
    private void Update()
    {
        // Toggle debug info with key
        if (Input.GetKeyDown(toggleKey))
        {
            showDebugInfo = !showDebugInfo;
            Debug.Log($"[DEBUGGER] Debug info toggled: {(showDebugInfo ? "ON" : "OFF")}");
        }
        
        if (showDebugInfo)
        {
            UpdateDebugInfo();
        }
    }
    
    private void UpdateDebugInfo()
    {
        if (interactionManager == null) return;
        
        // Get current interaction state
        Interactable closest = interactionManager.GetClosestInteractable();
        var nearby = interactionManager.GetNearbyInteractables();
        
        // Build debug string
        string debugInfo = $"=== INTERACTION DEBUG ===\n";
        debugInfo += $"Time: {Time.time:F1}s | Frame: {Time.frameCount}\n";
        debugInfo += $"Player Position: {transform.position}\n";
        debugInfo += $"Closest Interactable: {(closest != null ? closest.name : "None")}\n";
        debugInfo += $"Nearby Interactables: {nearby.Count}\n";
        
        // Show all interactables and their states
        debugInfo += $"\n--- All Interactables ---\n";
        foreach (var interactable in allInteractables)
        {
            if (interactable != null)
            {
                bool inRange = interactable.IsPlayerInRange();
                float distance = Vector3.Distance(transform.position, interactable.transform.position);
                debugInfo += $"{interactable.name}: {(inRange ? "IN RANGE" : "out of range")} ({distance:F1}m)\n";
            }
        }
        
        // Show all doors and their states
        debugInfo += $"\n--- All Doors ---\n";
        foreach (var door in allDoors)
        {
            if (door != null)
            {
                debugInfo += $"{door.name}: {(door.IsOpen ? "OPEN" : "CLOSED")} | Animating: {door.IsAnimating}\n";
            }
        }
        
        // Show input system status
        debugInfo += $"\n--- Input System ---\n";
        debugInfo += $"InteractionManager: {(interactionManager != null ? "✓" : "✗")}\n";
        
        // Update UI text
        if (debugText != null)
        {
            debugText.text = debugInfo;
        }
        
        // Log to console every few seconds
        if (Time.frameCount % 180 == 0) // Every 3 seconds at 60fps
        {
            Debug.Log($"[DEBUGGER] {debugInfo}");
        }
    }
    
    // Draw debug gizmos in scene view
    private void OnDrawGizmos()
    {
        if (!showDebugInfo) return;
        
        // Draw interaction ranges
        Gizmos.color = Color.cyan;
        foreach (var interactable in allInteractables)
        {
            if (interactable != null)
            {
                Gizmos.DrawWireSphere(interactable.transform.position, 3f); // Assuming 3m range
            }
        }
        
        // Draw lines to nearby interactables
        if (interactionManager != null)
        {
            var nearby = interactionManager.GetNearbyInteractables();
            Gizmos.color = Color.green;
            foreach (var interactable in nearby)
            {
                if (interactable != null)
                {
                    Gizmos.DrawLine(transform.position, interactable.transform.position);
                }
            }
        }
    }
    
    // Public methods for external debugging
    public void LogInteractionState()
    {
        UpdateDebugInfo();
    }
    
    public void ForceDebugUpdate()
    {
        showDebugInfo = true;
        UpdateDebugInfo();
    }
}
