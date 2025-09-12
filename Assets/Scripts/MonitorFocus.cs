// Imports.
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Seagull.Interior_01;

public class MonitorFocus : MonoBehaviour
{
    // cameras.
    [Header("cameras ----------------------------------------------")]
    [SerializeField] Camera monitorCamera; 
    [SerializeField] Camera playerCamera;          
    [SerializeField] Camera threeDCamera;
    [SerializeField] Camera houseCamera;

    // focus target.  
    [Header("focus -----------------------------------------------")]           
    [SerializeField] Transform focusTarget;  
    [SerializeField] GameObject focusPrompt;  
    

    // canvases.
    [SerializeField] Canvas monitorCanvas;         
    [SerializeField] Canvas turnOnPromptCanvas;    
    [SerializeField] Canvas cursorCanvas;     
    // custom cursor.
    [SerializeField] GameObject customCursor;          
    
    [SerializeField] MonitorCamLook monitorCamLook;      
    [SerializeField] GameObject playerObject;      
    [SerializeField] ComputerInteraction computerInteraction; 
    
    [SerializeField] MonitorButton monitorButton;  
    
    // canvas background.
    [SerializeField] Sprite monitorOffSprite;      
    [SerializeField] Sprite monitorOnSprite;     
    [SerializeField] Image canvasBackground;       
    
    // game integration.
    [SerializeField] SkateGame skateGame;    

    // focus settings.
    private bool isFocused = false; 
    private bool isTransitioning = false; 
    private bool isMonitorOn = false; 
    float focusSpeed = 2f;         
    float focusDistance = 2f;     
    float focusFOV = 30f;         

    private Vector3 originalCameraPosition; 
    private Quaternion originalCameraRotation; 
    private float originalFOV; 

    private bool isHoveringLastFrame = false; 
    private RectTransform cursorRectTransform; 
    private Vector2 cursorPosition = new Vector2(0.5f, 0.5f); 
    private Movement movementScript; 
    

    // -------------------------------------------------------- Before First Frame.
    
    void Start()
    {
        // store original camera state.
        originalCameraPosition = monitorCamera.transform.position;
        originalCameraRotation = monitorCamera.transform.rotation;
        originalFOV = monitorCamera.fieldOfView;
        
        // get movement script.
        movementScript = playerObject.GetComponent<Movement>();
        
        // set up UI for prompts.
        InitializeUI();
        
        // check if collider exists.
        Collider col = GetComponent<Collider>();        
    }
    
    void InitializeUI()
    {
        // hide focus prompt at start.
        focusPrompt.SetActive(false);
        
        // set up canvas to render to monitor cam.
        monitorCanvas.worldCamera = monitorCamera;
        monitorCanvas.renderMode = RenderMode.ScreenSpaceCamera;

        // get rect transform for custom cursor.
        cursorRectTransform = customCursor.GetComponent<RectTransform>();

        // start cursor hidden.
        customCursor.SetActive(false);
        
        // set monitor to off state.
        SetMonitorState(false);
    }
    
    void Update()
    {
        // check for mouse hover.
        CheckMouseHover();
        
        // if focused & p key pressed, toggle monitor.
        if (isFocused && Keyboard.current != null && Keyboard.current.pKey.wasPressedThisFrame)
        {
            ToggleMonitor();
        }
        
        // update custom cursor when focused & monitor is on.
        if (isFocused && isMonitorOn)
        {
            UpdateCustomCursor();
        }
    }
    
    void CheckMouseHover()
    {   
        // if focused, check for escape key press.
        if (isFocused)
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
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
        
        // create a ray from the center of the monitor camera.
        Ray ray = monitorCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;
        
        // check if raycast hits collider.
        bool isHovering = Physics.Raycast(ray, out hit) && hit.collider == GetComponent<Collider>();
        
        // if hovering & not focused & not transitioning, 
        if (isHovering && !isFocused && !isTransitioning)
        {
            // if we weren't hovering, show focus prompt.
            if (!isHoveringLastFrame) focusPrompt.SetActive(true);
            
            // check for mouse click while hovering.
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                // if we're not focused & not transitioning, start focus.
                if (!isFocused && !isTransitioning)
                {
                    StartFocus();
                }
                // if we're focused & not transitioning, stop focus.
                else if (isFocused && !isTransitioning)
                {
                    StopFocus();
                }
            }
        }
        // if we're not hovering & we were hovering last frame, hide focus prompt.
        else if (!isHovering && isHoveringLastFrame)
        {
            focusPrompt.SetActive(false);
        }
        
        // update hovering last frame.
        isHoveringLastFrame = isHovering;
    }
    
    bool CheckAppIconClick()
    {
        // Only check if game is not active and skateGame is assigned
        if (skateGame == null || skateGame.IsGameActive())
        {
            return false;
        }
        
        // Create a ray from the center of the monitor camera
        Ray ray = monitorCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;
        
        // Check if raycast hits the app icon collider
        if (Physics.Raycast(ray, out hit))
        {
            // Check if the hit object has the SkateGame component (app icon)
            if (hit.collider.GetComponent<SkateGame>() != null)
            {
                Debug.Log("App icon clicked!");
                skateGame.OnAppIconClick();
                return true;
            }
        }
        
        return false;
    }
    
    void StartFocus()
    {  
        // hide focus prompt.
        focusPrompt.SetActive(false);
        
        // show turn on prompt if monitor is off.
        turnOnPromptCanvas.gameObject.SetActive(!isMonitorOn);
        
        // show custom cursor only when monitor is on.
        customCursor.SetActive(isMonitorOn);

        // set focus to true & start transition.
        isFocused = true;
        isTransitioning = true;
        
        threeDCamera.enabled = false;
        threeDCamera.gameObject.SetActive(false);

        monitorCamera.enabled = true;

        movementScript.enabled = false;
        
        // disable monitor camera movement when focused.
        monitorCamLook.SetAllowLook(false);
        
        // store current camera state.
        originalCameraPosition = monitorCamera.transform.position;
        originalCameraRotation = monitorCamera.transform.rotation;
        originalFOV = monitorCamera.fieldOfView;
        
        // smooth focus to target.
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
        
        // Re-enable 3D Camera and disable monitor camera when stopping focus
        threeDCamera.enabled = true;
        threeDCamera.gameObject.SetActive(true);
        
        monitorCamera.enabled = false;
        
        
        // Re-enable Movement Script when not focused
        movementScript.enabled = true;
        
        
        // Re-enable monitor camera movement when not focused. (Can Look Around In Monitor View.)
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
        
        // update canvas background image
        canvasBackground.sprite = on ? monitorOnSprite : monitorOffSprite;
        
        // update monitor button state
        monitorButton.SetMonitorState(on);
        
        // update UI elements
        turnOnPromptCanvas.gameObject.SetActive(isFocused && !on);
        customCursor.SetActive(isFocused && on);
    }
}
