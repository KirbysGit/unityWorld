using UnityEngine;

[RequireComponent(typeof(Door))]
[RequireComponent(typeof(Interactable))]
public class DoorInteractable : MonoBehaviour
{
    [Header("Door Interaction")]
    [SerializeField] private bool autoClose = false;
    [SerializeField] private float autoCloseDelay = 3f;
    
    private Door door;
    private Interactable interactable;
    private Coroutine autoCloseCoroutine;
    
    private void Start()
    {
        Debug.Log($"[DOOR INTERACTABLE DEBUG] Initializing DoorInteractable for {gameObject.name}");
        
        // Get required components
        door = GetComponent<Door>();
        interactable = GetComponent<Interactable>();
        
        if (door == null || interactable == null)
        {
            Debug.LogError($"[DOOR INTERACTABLE DEBUG] {gameObject.name}: Missing required components! Door: {(door != null ? "✓" : "✗")}, Interactable: {(interactable != null ? "✓" : "✗")}");
            return;
        }
        
        Debug.Log($"[DOOR INTERACTABLE DEBUG] {gameObject.name}: All components found ✓");
        
        // Subscribe to events
        interactable.OnInteractionTriggered += OnInteractionTriggered;
        door.OnDoorStateChanged += OnDoorStateChanged;
        
        Debug.Log($"[DOOR INTERACTABLE DEBUG] {gameObject.name}: Event subscription complete. DoorInteractable ready!");
    }
    
    private void OnInteractionTriggered()
    {
        Debug.Log($"[DOOR INTERACTABLE DEBUG] {gameObject.name}: Interaction triggered! Calling door.ToggleDoor()");
        
        // Toggle the door
        door.ToggleDoor();
    }
    
    private void OnDoorStateChanged(bool isOpen)
    {
        Debug.Log($"[DOOR INTERACTABLE DEBUG] {gameObject.name}: Door state changed to {(isOpen ? "OPEN" : "CLOSED")}");
        
        if (isOpen && autoClose)
        {
            Debug.Log($"[DOOR INTERACTABLE DEBUG] {gameObject.name}: Door opened with auto-close enabled. Starting {autoCloseDelay}s timer...");
            
            // Start auto-close timer
            if (autoCloseCoroutine != null)
                StopCoroutine(autoCloseCoroutine);
            autoCloseCoroutine = StartCoroutine(AutoCloseDoor());
        }
        else if (!isOpen)
        {
            Debug.Log($"[DOOR INTERACTABLE DEBUG] {gameObject.name}: Door closed. Canceling auto-close timer if active.");
            
            // Cancel auto-close if door is closed
            if (autoCloseCoroutine != null)
            {
                StopCoroutine(autoCloseCoroutine);
                autoCloseCoroutine = null;
            }
        }
    }
    
    private System.Collections.IEnumerator AutoCloseDoor()
    {
        Debug.Log($"[DOOR INTERACTABLE DEBUG] {gameObject.name}: Auto-close timer started. Waiting {autoCloseDelay}s...");
        
        yield return new WaitForSeconds(autoCloseDelay);
        
        Debug.Log($"[DOOR INTERACTABLE DEBUG] {gameObject.name}: Auto-close timer finished. Checking if door should close...");
        
        if (door.IsOpen && !door.IsAnimating)
        {
            Debug.Log($"[DOOR INTERACTABLE DEBUG] {gameObject.name}: Auto-closing door...");
            door.CloseDoor();
        }
        else
        {
            Debug.Log($"[DOOR INTERACTABLE DEBUG] {gameObject.name}: Door not closed automatically - IsOpen: {door.IsOpen}, IsAnimating: {door.IsAnimating}");
        }
        
        autoCloseCoroutine = null;
        Debug.Log($"[DOOR INTERACTABLE DEBUG] {gameObject.name}: Auto-close coroutine finished");
    }
    
    private void OnDestroy()
    {
        // Clean up event subscriptions
        if (interactable != null)
            interactable.OnInteractionTriggered -= OnInteractionTriggered;
        if (door != null)
            door.OnDoorStateChanged -= OnDoorStateChanged;
            
        if (autoCloseCoroutine != null)
            StopCoroutine(autoCloseCoroutine);
    }
}
