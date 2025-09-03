using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Seagull.Interior_01;

public class MonitorFocus : MonoBehaviour
{
    [Header("Camera Focus Settings")]
    [SerializeField] Camera monitorCamera;          // The Monitor Cam.
    [SerializeField] Transform focusTarget;         // Where To Look Based On Mouse.
    [SerializeField] float focusSpeed = 2f;         // How Fast To Focus.
    [SerializeField] float focusDistance = 2f;      // How Far Back To Move Camera.
    [SerializeField] float focusFOV = 30f;          // FOV During Focus.
    
    [Header("Focus Behavior")]
    [SerializeField] public bool startInFocusMode = false; // If You Want To Start In Focus Mode.
    
    [Header("UI Elements")]
    [SerializeField] GameObject focusPrompt;        // UI Prompt To Show On Hover On Monitor, In MonitorView.
    [SerializeField] Canvas monitorCanvas;          // Canvas That Renders To Monitor Cam.
    [SerializeField] Canvas turnOnPromptCanvas;     // Canvas with "Press P to turn on monitor" message
    
    [Header("Custom Cursor")]
    [SerializeField] GameObject customCursor;       // Custom Cursor GameObject (2D Sprite)
    [SerializeField] bool showCustomCursor = true;  // Enable/Disable Custom Cursor. ? 
    [SerializeField] Canvas cursorCanvas;           // Canvas For Cursor Positioning.
    
    [Header("Camera Control")]
    [SerializeField] MonitorCamLook monitorCamLook; // Reference To Monitor Camera Look Script.
    [SerializeField] Camera playerCamera;           // Reference To Player Camera.
    [SerializeField] Camera threeDCamera;           // Reference To 3D Camera.
    [SerializeField] GameObject playerObject;       // Reference To Player GameObject (Has Movement Script).
    [SerializeField] ComputerInteraction computerInteraction; // Reference To Computer Interaction Script.
    
    [Header("Monitor State")]
    [SerializeField] MonitorButton monitorButton;   // Reference to monitor button
    
    [Header("Canvas Background")]
    [SerializeField] Sprite monitorOffSprite;       // Black/dark sprite for monitor off
    [SerializeField] Sprite monitorOnSprite;        // Background sprite for monitor on
    [SerializeField] Image canvasBackground;        // Reference to the Image component on canvas
    

    private bool isFocused = false; // If You Are Focused On The Monitor.
    private bool isTransitioning = false; // If You Are Transitioning Between Focus And Not Focus.
    private bool isMonitorOn = false; // If The Monitor Is Turned On.

    private Vector3 originalCameraPosition; // Original Camera Position.
    private Quaternion originalCameraRotation; // Original Camera Rotation.
    private float originalFOV; // Original FOV.


    private bool isHoveringLastFrame = false; // If You Are Hovering On The Monitor Last Frame.
    private RectTransform cursorRectTransform; // Rect Transform For Cursor.
    private Vector2 cursorPosition = new Vector2(0.5f, 0.5f); // Start At Center.
    private Movement movementScript; // Cached Reference To Movement Component.
    
    void Start()
    {
        // Get Monitor Cam.
        if (monitorCamera == null)
        {
            monitorCamera = Camera.main;
        }
        
        // Store Original Camera State. (Like the Center Position, Rotation, And FOV.)
        if (monitorCamera != null)
        {
            originalCameraPosition = monitorCamera.transform.position;
            originalCameraRotation = monitorCamera.transform.rotation;
            originalFOV = monitorCamera.fieldOfView;
        }
        
        // Get Movement Component From Player GameObject. (So You Can Disable It When Focused.)
        if (playerObject != null)
        {
            movementScript = playerObject.GetComponent<Movement>();
        }
        
        // Initialize UI. (For Focus Prompt.)
        InitializeUI();
        
        // Check If Collider Exists. (So You Can Hover On The Monitor.)
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            // No Collider Found - Mouse Hover Won't Work Without A Collider.
        }
        
        // Start In Focus Mode If Enabled.
        if (startInFocusMode)
        {
            StartFocusModeImmediately();
        }
    }
    
    void InitializeUI()
    {
        // Hide Focus Prompt At Start.
        focusPrompt.SetActive(false);
        
        // Set Up Canvas To Render To Monitor Cam.
        monitorCanvas.worldCamera = monitorCamera;
        monitorCanvas.renderMode = RenderMode.ScreenSpaceCamera;

        // Get Rect Transform For Custom Cursor.
        cursorRectTransform = customCursor.GetComponent<RectTransform>();

        // Start Cursor Hidden.
        customCursor.SetActive(false);
        
        // Set monitor to off state
        SetMonitorState(false);
    }
    
    void Update()
    {
        // Check For Mouse Hover Using Raycasting (More Reliable With Locked Cursor).
        CheckMouseHover();
        
        // Handle P key input when focused
        if (isFocused && Keyboard.current != null && Keyboard.current.pKey.wasPressedThisFrame)
        {
            ToggleMonitor();
        }
        
        // Update Custom Cursor When Focused And Monitor Is On.
        if (isFocused && isMonitorOn)
        {
            UpdateCustomCursor();
        }
    }
    
    void CheckMouseHover()
    {   
        // If We're Focused, Don't Check For Hover - Cursor Can Move Freely.
        if (isFocused)
        {
            // Check For Click To Unfocus.
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                if (isFocused)
                {
                    StopFocus();
                } else {
                    StartFocus();
                }
            }
            return;
        }
        
        // Create A Ray From The Center Of The Monitor Camera (Since Cursor Is Locked To Center).
        Ray ray = monitorCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;
        
        // Check If Raycast Hits Collider.
        bool isHovering = Physics.Raycast(ray, out hit) && hit.collider == GetComponent<Collider>();
        
        // If We're Hovering And Not Focused And Not Transitioning, Do Stuff.
        if (isHovering && !isFocused && !isTransitioning)
        {
            // If We're Not Hovering Last Frame, Show The Focus Prompt. "Click To Focus Monitor."
            if (!isHoveringLastFrame)
            {
                focusPrompt.SetActive(true);
            }
            
            // Check For Mouse Click While Hovering.
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                // If We're Not Focused And Not Transitioning, Start Focus.
                if (!isFocused && !isTransitioning)
                {
                    // Start Focus.
                    StartFocus();
                }
                // If We're Focused And Not Transitioning, Stop Focus.
                else if (isFocused && !isTransitioning)
                {
                    // Stop Focus.
                    StopFocus();
                }
            }
        }
        // If We're Not Hovering And We Were Hovering Last Frame, Hide The Focus Prompt. "Click To Unfocus Monitor."
        else if (!isHovering && isHoveringLastFrame)
        {
            focusPrompt.SetActive(false);
        }
        
        isHoveringLastFrame = isHovering;
    }
    
    void StartFocus()
    {  
        // Hide The Focus Prompt On Focus.
        focusPrompt.SetActive(false);
        
        // Show turn on prompt if monitor is off
        turnOnPromptCanvas.gameObject.SetActive(!isMonitorOn);
        
        // Show Custom Cursor Only When Monitor Is On.
        customCursor.SetActive(isMonitorOn);

        // Set Focus To True & Start Transition.
        isFocused = true;
        isTransitioning = true;
        
        // Disable Monitor Camera Movement When Focused. (Can't Look Around In Monitor View.)
        monitorCamLook.SetFocused(true);
        
        // Store Current Camera State. (Like the Center Position, Rotation, And FOV.)
        originalCameraPosition = monitorCamera.transform.position;
        originalCameraRotation = monitorCamera.transform.rotation;
        originalFOV = monitorCamera.fieldOfView;
        
        // Smooth Focus To Target.
        StartCoroutine(SmoothFocusToTarget());
    }
    
    void StopFocus()
    {   
        // Hide Custom Cursor When Unfocused.
        customCursor.SetActive(false);
        
        // Hide turn on prompt when unfocused
        turnOnPromptCanvas.gameObject.SetActive(false);
        
        // Set Focus To False & Start Transition.
        isFocused = false;
        isTransitioning = true;
        
        // Re-enable monitor camera movement when not focused. (Can Look Around In Monitor View.)
        monitorCamLook.SetFocused(false);
        monitorCamLook.SetAllowLook(true);
        
        // Smooth Return To Original View.
        StartCoroutine(SmoothReturnToOriginal());
    }
    
    System.Collections.IEnumerator SmoothFocusToTarget()
    {
        // Store Current Camera State. (Like the Center Position, Rotation, And FOV.)
        Vector3 startPosition = monitorCamera.transform.position;
        Quaternion startRotation = monitorCamera.transform.rotation;
        float startFOV = monitorCamera.fieldOfView;
        
        // Calculate Target Position.
        Vector3 direction = (focusTarget.position - startPosition).normalized;
        Vector3 targetPosition = focusTarget.position - direction * focusDistance;
        Quaternion targetRotation = Quaternion.LookRotation(focusTarget.position - targetPosition);
        
        float elapsedTime = 0f;
        
        // While Elapsed Time Is Less Than 1/Focus Speed, Interpolate Between Start And Target.
        while (elapsedTime < 1f / focusSpeed)
        {
            // Increment Elapsed Time By Time.DeltaTime.
            elapsedTime += Time.deltaTime;
            float t = elapsedTime * focusSpeed;
            
            // Smooth Interpolation Between Start And Target.
            monitorCamera.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            monitorCamera.transform.rotation = Quaternion.Lerp(startRotation, targetRotation, t);
            monitorCamera.fieldOfView = Mathf.Lerp(startFOV, focusFOV, t);
            
            yield return null;
        }
        
        // Ensure Final Values For Position, Rotation, And FOV.
        monitorCamera.transform.position = targetPosition;
        monitorCamera.transform.rotation = targetRotation;
        monitorCamera.fieldOfView = focusFOV;
        
        // Set Transitioning To False.
        isTransitioning = false;
    }
    
    System.Collections.IEnumerator SmoothReturnToOriginal()
    {
        // Store Current Camera State. (Like the Center Position, Rotation, And FOV.)
        Vector3 startPosition = monitorCamera.transform.position;
        Quaternion startRotation = monitorCamera.transform.rotation;
        float startFOV = monitorCamera.fieldOfView;
        
        // While Elapsed Time Is Less Than 1/Focus Speed, Interpolate Between Start And Original.
        float elapsedTime = 0f;
        
        // While Elapsed Time Is Less Than 1/Focus Speed, Interpolate Between Start And Original.
        while (elapsedTime < 1f / focusSpeed)
        {
            // Increment Elapsed Time By Time.DeltaTime.
            elapsedTime += Time.deltaTime;
            float t = elapsedTime * focusSpeed;
            
            // Smooth Interpolation Back To Original.
            monitorCamera.transform.position = Vector3.Lerp(startPosition, originalCameraPosition, t);
            monitorCamera.transform.rotation = Quaternion.Lerp(startRotation, originalCameraRotation, t);
            monitorCamera.fieldOfView = Mathf.Lerp(startFOV, originalFOV, t);
            
            yield return null;
        }
        
        // Ensure Final Values For Position, Rotation, And FOV.
        monitorCamera.transform.position = originalCameraPosition;
        monitorCamera.transform.rotation = originalCameraRotation;
        monitorCamera.fieldOfView = originalFOV;
        
        // Set Transitioning To False.
        isTransitioning = false;
    }
    
    
    void UpdateCustomCursor()
    {
        // Use Mouse Delta For Movement. (Works Better With Locked Cursor.)
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        
        // Update Cursor Position Based On Mouse Delta.
        // Fix Horizontal Inversion By Subtracting Instead Of Adding.
        cursorPosition.x += mouseDelta.x / Screen.width;  // Inverted Horizontal Movement.
        cursorPosition.y += mouseDelta.y / Screen.height; // Normal Vertical Movement.
        
        // Get Canvas Size
        RectTransform canvasRect = cursorCanvas.GetComponent<RectTransform>();
        Vector2 canvasSize = canvasRect.sizeDelta;
        
        // Convert Cursor Position To Canvas Coordinates
        // Map 0-1 Cursor Position To Canvas Size
        Vector2 canvasPosition = new Vector2(
            (cursorPosition.x - 0.5f) * canvasSize.x,  // -0.5 To 0.5 Range
            (cursorPosition.y - 0.5f) * canvasSize.y   // -0.5 To 0.5 Range
        );
        
        // Clamp Cursor Position To Canvas Bounds (accounting for cursor size)
        float halfWidth = canvasSize.x * 0.5f;
        float halfHeight = canvasSize.y * 0.5f;
        
        // Get cursor size to prevent pivot from going outside canvas
        Vector2 cursorSize = cursorRectTransform.sizeDelta;
        float cursorHalfWidth = cursorSize.x * 0.5f;
        float cursorHalfHeight = cursorSize.y * 0.5f;
        
        // Clamp so cursor pivot stays within canvas bounds
        canvasPosition.x = Mathf.Clamp(canvasPosition.x, -halfWidth + cursorHalfWidth, halfWidth - cursorHalfWidth);
        canvasPosition.y = Mathf.Clamp(canvasPosition.y, -halfHeight + cursorHalfHeight, halfHeight - cursorHalfHeight);
        
        // Set Cursor Position On Canvas
        cursorRectTransform.anchoredPosition = canvasPosition;
    }
    
    void ToggleMonitor()
    {
        SetMonitorState(!isMonitorOn);
    }
    
    void SetMonitorState(bool on)
    {
        isMonitorOn = on;
        
        // Update canvas background image
        canvasBackground.sprite = on ? monitorOnSprite : monitorOffSprite;
        
        // Update monitor button state
        monitorButton.SetMonitorState(on);
        
        // Update UI elements
        turnOnPromptCanvas.gameObject.SetActive(isFocused && !on);
        customCursor.SetActive(isFocused && on);
    }
    
    void StartFocusModeImmediately()
    {
        // Disable Player Cameras And Enable Monitor Camera.
        playerCamera.enabled = false;
        playerCamera.gameObject.SetActive(false);
        threeDCamera.enabled = false;
        threeDCamera.gameObject.SetActive(false);
        monitorCamera.enabled = true;
        
        // Disable Movement Script
        movementScript.enabled = false;
        
        // Set Up ComputerInteraction
        computerInteraction.SetSittingState(true);
        
        // Enable Monitor Camera Look Controls
        monitorCamLook.SetAllowLook(true);
        
        // Start focus mode automatically
        StartFocus();
    }
    
    System.Collections.IEnumerator StartInFocusMode()
    {
        yield return null;
        
        playerCamera.enabled = false;
        threeDCamera.enabled = false;
        monitorCamera.enabled = true;
        movementScript.enabled = false;
        
        StartFocus();
    }

}
