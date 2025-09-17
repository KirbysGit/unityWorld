using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerActions : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private GameObject interactionPrompt;    
    [SerializeField] private Transform playerCamera;         
    [SerializeField] private LayerMask useLayers;            
    [SerializeField] private LayerMask hoverLayers = -1;      
    
    private float hoverRange = 50f;         
    private TextMeshPro promptText;                          
    private IInteractable currentInteractable;               
    private OutlineHover currentHoveredObject;               

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
        
        // check if raycast hits any objects in hoverLayers.
        if (Physics.Raycast(ray, out hit, hoverRange, hoverLayers))
        {
            // check for outline hover component.
            OutlineHover outlineHover = hit.collider.GetComponent<OutlineHover>();

            // update outline hover if not already hovered.
            if (outlineHover != null && currentHoveredObject != outlineHover)
            {
                // remove hover from previous object if not null.
                if (currentHoveredObject != null)
                    currentHoveredObject.SetHovered(false);
                
                // add hover to new object.
                currentHoveredObject = outlineHover;
                currentHoveredObject.SetHovered(true);
            }
            
            // check if hit object has interactable component.
            if (hit.collider.TryGetComponent<IInteractable>(out IInteractable interactable))
            {
                // if different interactable, update current interactable.
                if (currentInteractable != interactable)
                {
                    if (currentInteractable != null && currentInteractable is Door previousDoor) 
                    {
                        previousDoor.HidePrompt();
                    }

                    currentInteractable = interactable;

                    if (interactable is Door door)
                    {
                        door.ShowPrompt();
                    }
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
        Debug.Log($"SHOW PROMPT: {text}");
        promptText.SetText(text);
        interactionPrompt.SetActive(true);
    }

    // -------------------------------------------------------- hide interaction prompt.
    void HidePrompt()
    {
        interactionPrompt.SetActive(false);
        currentInteractable = null;
    }
    
    // -------------------------------------------------------- remove hover effect.
    void RemoveHoverEffect()
    {
        if (currentHoveredObject != null) {
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