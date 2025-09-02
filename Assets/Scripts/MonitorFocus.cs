using UnityEngine;
using UnityEngine.InputSystem;

public class MonitorFocus : MonoBehaviour
{
    [Header("Camera Focus Settings")]
    [SerializeField] Camera monitorCamera;          // The monitor camera (not player camera)
    [SerializeField] Transform focusTarget;         // Where to look at (the monitor/computer)
    [SerializeField] float focusSpeed = 2f;         // How fast to focus
    [SerializeField] float focusDistance = 2f;      // How far back to move camera
    [SerializeField] float focusFOV = 30f;          // Field of view when focused
    
    [Header("Focus Behavior")]
    [SerializeField] bool smoothFocus = true;       // Smooth transition vs instant
    
    [Header("UI Elements")]
    [SerializeField] GameObject focusPrompt;        // UI prompt to show when hovering
    [SerializeField] Canvas monitorCanvas;          // Canvas that renders to monitor camera
    
    [Header("Custom Cursor")]
    [SerializeField] GameObject customCursor;       // Custom cursor GameObject (2D Sprite)
    [SerializeField] bool showCustomCursor = true;  // Enable/disable custom cursor
    [SerializeField] Canvas cursorCanvas;           // Canvas for cursor positioning
    
    [Header("Camera Control")]
    [SerializeField] MonitorCamLook monitorCamLook; // Reference to monitor camera look script
    
    private bool isFocused = false;
    private bool isTransitioning = false;
    private Vector3 originalCameraPosition;
    private Quaternion originalCameraRotation;
    private float originalFOV;
    private bool isHoveringLastFrame = false;
    private RectTransform cursorRectTransform;
    private Vector2 cursorPosition = new Vector2(0.5f, 0.5f); // Start at center
    
    void Start()
    {
        // Find monitor camera if not assigned
        if (monitorCamera == null)
        {
            monitorCamera = Camera.main;
        }
        
        // Store original camera state
        if (monitorCamera != null)
        {
            originalCameraPosition = monitorCamera.transform.position;
            originalCameraRotation = monitorCamera.transform.rotation;
            originalFOV = monitorCamera.fieldOfView;
        }
        
        // Initialize UI
        InitializeUI();
        
        // Check if collider exists
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogError("MonitorFocus: No Collider found on " + gameObject.name + "! Mouse hover won't work without a Collider.");
        }
    }
    
    void InitializeUI()
    {
        if (focusPrompt != null)
        {
            focusPrompt.SetActive(false);
        }
        
        // Set up canvas to render to monitor camera
        if (monitorCanvas != null && monitorCamera != null)
        {
            monitorCanvas.worldCamera = monitorCamera;
            monitorCanvas.renderMode = RenderMode.ScreenSpaceCamera;
        }
        
        // Set up custom cursor
        if (customCursor != null)
        {
            cursorRectTransform = customCursor.GetComponent<RectTransform>();
            if (cursorRectTransform == null)
            {
                Debug.LogError("MonitorFocus: customCursor needs a RectTransform component! " +
                    "Make sure your cursor GameObject is a UI element (Image, Button, etc.) or add a RectTransform component.");
            }
            else
            {
                Debug.Log("MonitorFocus: Custom cursor setup complete - RectTransform found");
            }
            customCursor.SetActive(false); // Start hidden
            Debug.Log("MonitorFocus: Custom cursor setup complete - cursor is hidden");
        }
        else
        {
            Debug.LogWarning("MonitorFocus: customCursor is null! Please assign a cursor GameObject.");
        }
    }
    
    void Update()
    {
        // Check for mouse hover using raycasting (more reliable with locked cursor)
        CheckMouseHover();
        
        // Update custom cursor when focused
        if (isFocused && showCustomCursor)
        {
            Debug.Log("MonitorFocus: UpdateCustomCursor called - isFocused: " + isFocused + ", showCustomCursor: " + showCustomCursor);
            UpdateCustomCursor();
        }
        else
        {
            Debug.Log("MonitorFocus: Not updating cursor - isFocused: " + isFocused + ", showCustomCursor: " + showCustomCursor);
        }
    }
    
    void CheckMouseHover()
    {
        if (monitorCamera == null) return;
        
        // If we're focused, don't check for hover - cursor can move freely
        if (isFocused)
        {
            // Check for click to unfocus
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                OnMouseDown();
            }
            return;
        }
        
        // Create a ray from the center of the monitor camera (since cursor is locked to center)
        Ray ray = monitorCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;
        
        bool isHovering = Physics.Raycast(ray, out hit) && hit.collider == GetComponent<Collider>();
        
        if (isHovering && !isFocused && !isTransitioning)
        {
            if (!isHoveringLastFrame)
            {
                OnMouseEnter();
            }
            
            // Check for mouse click while hovering
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                OnMouseDown();
            }
        }
        else if (!isHovering && isHoveringLastFrame)
        {
            OnMouseExit();
        }
        
        isHoveringLastFrame = isHovering;
    }
    
    void OnMouseEnter()
    {
        // Show focus prompt when hovering
        ShowFocusPrompt();
    }
    
    void OnMouseExit()
    {
        // Hide focus prompt when not hovering
        HideFocusPrompt();
    }
    
    void OnMouseDown()
    {
        if (!isFocused && !isTransitioning)
        {
            // Click to start focus
            StartFocus();
        }
        else if (isFocused && !isTransitioning)
        {
            // Click to stop focus
            StopFocus();
        }
    }
    
    void StartFocus()
    {
        if (monitorCamera == null || focusTarget == null) return;
        
        HideFocusPrompt();
        
        // Show custom cursor when focused
        if (showCustomCursor && customCursor != null)
        {
            customCursor.SetActive(true);
            Debug.Log("MonitorFocus: Custom cursor activated");
        }

        isFocused = true;
        isTransitioning = true;
        
        Debug.Log("MonitorFocus: StartFocus called - isFocused set to: " + isFocused);
        
        // Disable monitor camera movement when focused
        if (monitorCamLook != null)
        {
            monitorCamLook.SetFocused(true);
        }
        
        // Store current camera state
        originalCameraPosition = monitorCamera.transform.position;
        originalCameraRotation = monitorCamera.transform.rotation;
        originalFOV = monitorCamera.fieldOfView;
        
        if (smoothFocus)
        {
            StartCoroutine(SmoothFocusToTarget());
        }
        else
        {
            FocusInstantly();
        }
        

    }
    
    void StopFocus()
    {
        if (monitorCamera == null) return;
        
        // Hide custom cursor when unfocused
        if (customCursor != null)
        {
            customCursor.SetActive(false);
        }
        
        isFocused = false;
        isTransitioning = true;
        
        // Re-enable monitor camera movement when not focused
        if (monitorCamLook != null)
        {
            monitorCamLook.SetFocused(false);
        }
        
        if (smoothFocus)
        {
            StartCoroutine(SmoothReturnToOriginal());
        }
        else
        {
            ReturnInstantly();
        }
        

    }
    
    void FocusInstantly()
    {
        // Calculate focus position
        Vector3 direction = (focusTarget.position - monitorCamera.transform.position).normalized;
        Vector3 focusPosition = focusTarget.position - direction * focusDistance;
        
        // Set camera position and rotation
        monitorCamera.transform.position = focusPosition;
        monitorCamera.transform.LookAt(focusTarget);
        monitorCamera.fieldOfView = focusFOV;
        
        isTransitioning = false;
    }
    
    void ReturnInstantly()
    {
        // Return to original state
        monitorCamera.transform.position = originalCameraPosition;
        monitorCamera.transform.rotation = originalCameraRotation;
        monitorCamera.fieldOfView = originalFOV;
        
        isTransitioning = false;
    }
    
    System.Collections.IEnumerator SmoothFocusToTarget()
    {
        Vector3 startPosition = monitorCamera.transform.position;
        Quaternion startRotation = monitorCamera.transform.rotation;
        float startFOV = monitorCamera.fieldOfView;
        
        // Calculate target position
        Vector3 direction = (focusTarget.position - startPosition).normalized;
        Vector3 targetPosition = focusTarget.position - direction * focusDistance;
        Quaternion targetRotation = Quaternion.LookRotation(focusTarget.position - targetPosition);
        
        float elapsedTime = 0f;
        
        while (elapsedTime < 1f / focusSpeed)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime * focusSpeed;
            
            // Smooth interpolation
            monitorCamera.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            monitorCamera.transform.rotation = Quaternion.Lerp(startRotation, targetRotation, t);
            monitorCamera.fieldOfView = Mathf.Lerp(startFOV, focusFOV, t);
            
            yield return null;
        }
        
        // Ensure final values
        monitorCamera.transform.position = targetPosition;
        monitorCamera.transform.rotation = targetRotation;
        monitorCamera.fieldOfView = focusFOV;
        
        isTransitioning = false;
    }
    
    System.Collections.IEnumerator SmoothReturnToOriginal()
    {
        Vector3 startPosition = monitorCamera.transform.position;
        Quaternion startRotation = monitorCamera.transform.rotation;
        float startFOV = monitorCamera.fieldOfView;
        
        float elapsedTime = 0f;
        
        while (elapsedTime < 1f / focusSpeed)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime * focusSpeed;
            
            // Smooth interpolation back to original
            monitorCamera.transform.position = Vector3.Lerp(startPosition, originalCameraPosition, t);
            monitorCamera.transform.rotation = Quaternion.Lerp(startRotation, originalCameraRotation, t);
            monitorCamera.fieldOfView = Mathf.Lerp(startFOV, originalFOV, t);
            
            yield return null;
        }
        
        // Ensure final values
        monitorCamera.transform.position = originalCameraPosition;
        monitorCamera.transform.rotation = originalCameraRotation;
        monitorCamera.fieldOfView = originalFOV;
        
        isTransitioning = false;
    }
    
    void ShowFocusPrompt()
    {
        
        if (focusPrompt != null)
        {
            focusPrompt.SetActive(true);
        }
    }
    
    void HideFocusPrompt()
    {
        if (focusPrompt != null)
        {
            focusPrompt.SetActive(false);
        }
    }
    
    void UpdateCustomCursor()
    {
        if (cursorRectTransform == null)
        {
            Debug.LogWarning("MonitorFocus: cursorRectTransform is null!");
            return;
        }
        if (Mouse.current == null)
        {
            Debug.LogWarning("MonitorFocus: Mouse.current is null!");
            return;
        }
        if (cursorCanvas == null)
        {
            Debug.LogWarning("MonitorFocus: cursorCanvas is null!");
            return;
        }
        
        // Use mouse delta for movement (works better with locked cursor)
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        
        // Update cursor position based on mouse delta
        // Fix horizontal inversion by subtracting instead of adding
        cursorPosition.x -= mouseDelta.x / Screen.width;  // Inverted horizontal movement
        cursorPosition.y += mouseDelta.y / Screen.height; // Normal vertical movement
        
        // Clamp cursor position to screen bounds (0 to 1)
        cursorPosition.x = Mathf.Clamp01(cursorPosition.x);
        cursorPosition.y = Mathf.Clamp01(cursorPosition.y);
        
        // Debug output (only when mouse is moving to reduce spam)
        if (mouseDelta.magnitude > 0.1f)
        {
            Debug.Log($"Mouse Delta: {mouseDelta}, Cursor Position: {cursorPosition}");
        }
        
        // Get canvas size
        RectTransform canvasRect = cursorCanvas.GetComponent<RectTransform>();
        Vector2 canvasSize = canvasRect.sizeDelta;
        
        // Convert cursor position to canvas coordinates
        // Map 0-1 cursor position to canvas size
        Vector2 canvasPosition = new Vector2(
            (cursorPosition.x - 0.5f) * canvasSize.x,  // -0.5 to 0.5 range
            (cursorPosition.y - 0.5f) * canvasSize.y   // -0.5 to 0.5 range
        );
        
        // Set cursor position on canvas
        cursorRectTransform.anchoredPosition = canvasPosition;
        
        Debug.Log($"MonitorFocus: Cursor moved to canvas position {canvasPosition} (canvas size: {canvasSize})");
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw focus target in scene view
        if (focusTarget != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(focusTarget.position, 0.2f);
            Gizmos.DrawLine(transform.position, focusTarget.position);
        }
    }
}
