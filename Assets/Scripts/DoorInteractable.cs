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
        // Get required components
        door = GetComponent<Door>();
        interactable = GetComponent<Interactable>();
        
        if (door == null || interactable == null)
        {
            Debug.LogError("DoorInteractable requires both Door and Interactable components!");
            return;
        }
        
        // Subscribe to events
        interactable.OnInteractionTriggered += OnInteractionTriggered;
        door.OnDoorStateChanged += OnDoorStateChanged;
    }
    
    private void OnInteractionTriggered()
    {
        // Toggle the door
        door.ToggleDoor();
    }
    
    private void OnDoorStateChanged(bool isOpen)
    {
        if (isOpen && autoClose)
        {
            // Start auto-close timer
            if (autoCloseCoroutine != null)
                StopCoroutine(autoCloseCoroutine);
            autoCloseCoroutine = StartCoroutine(AutoCloseDoor());
        }
        else if (!isOpen)
        {
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
        yield return new WaitForSeconds(autoCloseDelay);
        
        if (door.IsOpen && !door.IsAnimating)
        {
            door.CloseDoor();
        }
        
        autoCloseCoroutine = null;
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
