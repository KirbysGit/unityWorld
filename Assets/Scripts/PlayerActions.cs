using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerActions : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private GameObject interactionPrompt;    // UI prompt object.
    [SerializeField] private Transform playerCamera;          // camera for raycast.
    [SerializeField] private LayerMask useLayers;             // layers that can be interacted with.
    [SerializeField] private LayerMask hoverLayers = -1;      // layers for hover detection (default: all layers).
    
    private float hoverRange = 50f;          // max hover detection range (increased for testing).
    private TextMeshPro promptText;                           // text component for prompts.
    private IInteractable currentInteractable;                // currently hovered interactable.
    private OutlineHover currentHoveredObject;                // currently hovered object for outline.

    // -------------------------------------------------------- before first frame.
    void Start()
    {
        // get text component from prompt object.
        promptText = interactionPrompt.GetComponent<TextMeshPro>();
        
        // hide prompt at start.
        interactionPrompt.SetActive(false);
    }

    // -------------------------------------------------------- every frame.
    void Update()
    {
        CheckForInteractables();
    }

    // -------------------------------------------------------- check for interactables in range.
    void CheckForInteractables()
    {
        // raycast from cursor position for hover effects using new Input System.
        Vector2 mousePosition = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
        Ray ray = playerCamera.GetComponent<Camera>().ScreenPointToRay(mousePosition);
        RaycastHit hit;
        
        
        if (Physics.Raycast(ray, out hit, hoverRange, hoverLayers))
        {
            // check for outline hover component.
            OutlineHover outlineHover = hit.collider.GetComponent<OutlineHover>();
            // update outline hover.
            if (outlineHover != null && currentHoveredObject != outlineHover)
            {
                // remove hover from previous object.
                if (currentHoveredObject != null)
                    currentHoveredObject.SetHovered(false);
                
                // add hover to new object.
                currentHoveredObject = outlineHover;
                currentHoveredObject.SetHovered(true);
            }
            
            // check if hit object has interactable component.
            if (hit.collider.TryGetComponent<IInteractable>(out IInteractable interactable))
            {
                // if different interactable, update current.
                if (currentInteractable != interactable)
                {
                    currentInteractable = interactable;
                    ShowPrompt(interactable.GetPromptText());
                }
            }
            else
            {
                // no interactable found, hide prompt.
                HidePrompt();
            }
        }
        else
        {
            // nothing hit, remove hover and hide prompt.
            RemoveHoverEffect();
            HidePrompt();
        }
    }

    // -------------------------------------------------------- show interaction prompt.
    void ShowPrompt(string text)
    {
        if (promptText != null && interactionPrompt != null)
        {
            promptText.SetText(text);
            interactionPrompt.SetActive(true);
        }
        else
        {
            Debug.LogWarning("PlayerActions: promptText or interactionPrompt is null. Cannot show prompt.");
        }
    }

    // -------------------------------------------------------- hide interaction prompt.
    void HidePrompt()
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
        currentInteractable = null;
    }
    
    // -------------------------------------------------------- remove hover effect.
    void RemoveHoverEffect()
    {
        if (currentHoveredObject != null)
        {
            currentHoveredObject.SetHovered(false);
            currentHoveredObject = null;
        }
    }

    // -------------------------------------------------------- called when use key is pressed.
    public void OnUse()
    {
        currentInteractable.Interact(transform.position);
    }
}

// -------------------------------------------------------- interface for all interactable objects.
public interface IInteractable
{
    void Interact(Vector3 playerPosition);
    string GetPromptText();
}