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
        
        if (playerCamera == null) return;
        
        // Find all interactables in range
        Collider[] colliders = Physics.OverlapSphere(playerCamera.transform.position, maxInteractionDistance, interactableLayer);
        
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
                }
            }
        }
        
        if (showDebugInfo && nearbyInteractables.Count > 0)
        {
            Debug.Log($"Found {nearbyInteractables.Count} interactables nearby. Closest: {closestInteractable?.name}");
        }
    }
    
    private void UpdateInteractionUI()
    {
        if (interactionPromptUI == null) return;
        
        bool shouldShowUI = closestInteractable != null && closestInteractable.IsPlayerInRange();
        
        if (interactionPromptUI.activeSelf != shouldShowUI)
        {
            interactionPromptUI.SetActive(shouldShowUI);
        }
        
        if (shouldShowUI && promptText != null)
        {
            // You can customize this text based on the interactable type
            promptText.text = "Press E to interact";
        }
    }
    
    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        if (closestInteractable != null && closestInteractable.IsPlayerInRange())
        {
            closestInteractable.TriggerInteraction();
            
            if (showDebugInfo)
                Debug.Log($"Interacting with: {closestInteractable.name}");
        }
    }
    
    private void OnInteractStarted(InputAction.CallbackContext context)
    {
        // Optional: Add visual feedback when interaction starts
        if (showDebugInfo)
            Debug.Log("Interaction started");
    }
    
    private void OnInteractCanceled(InputAction.CallbackContext context)
    {
        // Optional: Add visual feedback when interaction is canceled
        if (showDebugInfo)
            Debug.Log("Interaction canceled");
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
