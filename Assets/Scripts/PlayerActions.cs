using TMPro;
using UnityEngine;

public class PlayerActions : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private GameObject interactionPrompt;    // UI prompt object.
    [SerializeField] private Transform camera;                // camera for raycast.
    [SerializeField] private LayerMask useLayers;             // layers that can be interacted with.

    private float maxUseDistance = 5f;                        // max interaction range.
    private TextMeshPro promptText;                           // text component for prompts.
    private IInteractable currentInteractable;                // currently hovered interactable.

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
        // raycast from camera forward.
        if (Physics.Raycast(camera.position, camera.forward, out RaycastHit hit, maxUseDistance, useLayers))
        {
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
            // nothing hit, hide prompt.
            HidePrompt();
        }
    }

    // -------------------------------------------------------- show interaction prompt.
    void ShowPrompt(string text)
    {
        promptText.SetText(text);
        interactionPrompt.SetActive(true);
    }

    // -------------------------------------------------------- hide interaction prompt.
    void HidePrompt()
    {
        interactionPrompt.SetActive(false);
        currentInteractable = null;
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