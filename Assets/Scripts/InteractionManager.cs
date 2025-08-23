using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class InteractionManager : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float maxInteractionDistance = 5f;
    [SerializeField] private LayerMask interactableLayer = 1; // Default layer
    
    [Header("UI References")]
    [SerializeField] private GameObject interactionPromptUI;
    [SerializeField] private TMPro.TextMeshProUGUI promptText;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;
    
    private List<Interactable> nearbyInteractables = new List<Interactable>();
    private Interactable closestInteractable;
    private Camera playerCamera;
    
    // Input System
    private PlayerInput playerInput;
    private InputAction interactAction;
    
    private void Start()
    {
        // Get player camera
        playerCamera = Camera.main;
        if (playerCamera == null)
        {
            // Try to find camera in children
            playerCamera = GetComponentInChildren<Camera>();
        }
        
        // Setup Input System
        SetupInputSystem();
        
        // Setup UI
        if (interactionPromptUI != null)
            interactionPromptUI.SetActive(false);
    }
    
    private void SetupInputSystem()
    {
        // Try to get PlayerInput component
        playerInput = GetComponent<PlayerInput>();
        
        if (playerInput != null && playerInput.actions != null)
        {
            // Get the interact action
            interactAction = playerInput.actions.FindAction("Interact");
            if (interactAction != null)
            {
                interactAction.performed += OnInteractPerformed;
                interactAction.started += OnInteractStarted;
                interactAction.canceled += OnInteractCanceled;
                Debug.Log("Input System: Interact action successfully configured!");
            }
            else
            {
                Debug.LogError("Interact action not found in Input Actions! Please ensure your Input Actions asset has an 'Interact' action configured.");
            }
        }
        else
        {
            Debug.LogError("PlayerInput component or Input Actions asset not found! Please add a PlayerInput component to your player and assign the Input Actions asset.");
        }
    }
    
    private void Update()
    {
        // Update nearby interactables
        UpdateNearbyInteractables();
        
        // Update UI
        UpdateInteractionUI();
    }
    
    private void UpdateNearbyInteractables()
    {
        nearbyInteractables.Clear();
        closestInteractable = null;
        
        if (playerCamera == null) 
        {
            Debug.LogWarning("[INTERACTION MANAGER DEBUG] No player camera found!");
            return;
        }
        
        // Find all interactables in range
        Collider[] colliders = Physics.OverlapSphere(playerCamera.transform.position, maxInteractionDistance, interactableLayer);
        
        // Debug every few frames to avoid spam
        if (Time.frameCount % 30 == 0) // Every 30 frames (about twice per second at 60fps)
        {
            Debug.Log($"[INTERACTION MANAGER DEBUG] Camera position: {playerCamera.transform.position}, Max distance: {maxInteractionDistance}m, Layer mask: {interactableLayer.value}");
            Debug.Log($"[INTERACTION MANAGER DEBUG] Found {colliders.Length} colliders in range");
        }
        
        float closestDistance = float.MaxValue;
        
        foreach (Collider col in colliders)
        {
            Interactable interactable = col.GetComponent<Interactable>();
            if (interactable != null)
            {
                float distance = Vector3.Distance(playerCamera.transform.position, col.transform.position);
                if (distance <= maxInteractionDistance)
                {
                    nearbyInteractables.Add(interactable);
                    
                    // Check if this is the closest
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestInteractable = interactable;
                    }
                    
                    if (Time.frameCount % 30 == 0)
                    {
                        Debug.Log($"[INTERACTION MANAGER DEBUG] Added interactable: {interactable.name} at {distance:F2}m");
                    }
                }
            }
            else
            {
                if (Time.frameCount % 30 == 0)
                {
                    Debug.LogWarning($"[INTERACTION MANAGER DEBUG] Collider {col.name} has no Interactable component!");
                }
            }
        }
        
        if (closestInteractable != null)
        {
            if (Time.frameCount % 30 == 0)
            {
                Debug.Log($"[INTERACTION MANAGER DEBUG] Closest interactable: {closestInteractable.name} at {closestDistance:F2}m");
            }
        }
        else
        {
            if (Time.frameCount % 30 == 0)
            {
                Debug.Log($"[INTERACTION MANAGER DEBUG] No interactables in range");
            }
        }
    }
    
    private void UpdateInteractionUI()
    {
        if (interactionPromptUI == null) 
        {
            Debug.LogWarning("[INTERACTION MANAGER DEBUG] No interaction prompt UI assigned!");
            return;
        }
        
        bool shouldShowUI = closestInteractable != null && closestInteractable.IsPlayerInRange();
        
        // Debug UI state changes
        if (interactionPromptUI.activeSelf != shouldShowUI)
        {
            if (shouldShowUI)
            {
                Debug.Log($"[INTERACTION MANAGER DEBUG] SHOWING UI for {closestInteractable?.name}");
            }
            else
            {
                Debug.Log("[INTERACTION MANAGER DEBUG] HIDING UI - no interactable in range");
            }
            
            interactionPromptUI.SetActive(shouldShowUI);
        }
        
        if (shouldShowUI && promptText != null)
        {
            // You can customize this text based on the interactable type
            promptText.text = "Press E to interact";
            
            if (Time.frameCount % 60 == 0) // Every second
            {
                Debug.Log($"[INTERACTION MANAGER DEBUG] UI is visible for {closestInteractable?.name}");
            }
        }
    }
    
    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        Debug.Log("[INTERACTION MANAGER DEBUG] Interact input received!");
        
        if (closestInteractable != null && closestInteractable.IsPlayerInRange())
        {
            Debug.Log($"[INTERACTION MANAGER DEBUG] SUCCESS - Triggering interaction with {closestInteractable.name}");
            closestInteractable.TriggerInteraction();
        }
        else
        {
            if (closestInteractable == null)
            {
                Debug.LogWarning("[INTERACTION MANAGER DEBUG] FAILED - No closest interactable found!");
            }
            else if (!closestInteractable.IsPlayerInRange())
            {
                Debug.LogWarning($"[INTERACTION MANAGER DEBUG] FAILED - {closestInteractable.name} is not in range!");
            }
        }
    }
    
    private void OnInteractStarted(InputAction.CallbackContext context)
    {
        Debug.Log("[INTERACTION MANAGER DEBUG] Interaction input started");
    }
    
    private void OnInteractCanceled(InputAction.CallbackContext context)
    {
        Debug.Log("[INTERACTION MANAGER DEBUG] Interaction input canceled");
    }
    
    // Public methods for other systems
    public Interactable GetClosestInteractable()
    {
        return closestInteractable;
    }
    
    public List<Interactable> GetNearbyInteractables()
    {
        return new List<Interactable>(nearbyInteractables);
    }
    
    // Optional: Visual debugging
    private void OnDrawGizmosSelected()
    {
        if (showDebugInfo && playerCamera != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(playerCamera.transform.position, maxInteractionDistance);
            
            if (closestInteractable != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(playerCamera.transform.position, closestInteractable.transform.position);
            }
        }
    }
    
    private void OnDestroy()
    {
        // Clean up input system
        if (interactAction != null)
        {
            interactAction.performed -= OnInteractPerformed;
            interactAction.started -= OnInteractStarted;
            interactAction.canceled -= OnInteractCanceled;
        }
    }
}
