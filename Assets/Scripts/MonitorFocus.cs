// handles the focus of the monitor. so the behavior between monitorView & focusedView.

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
    
    // scripts.
    [Header("scripts ---------------------------------------------")]
    [SerializeField] MonitorCamLook monitorCamLook;          
    [SerializeField] ComputerInteraction computerInteraction; 
    [SerializeField] ComputerScreen computerScreen;
    [SerializeField] SkateGame skateGame;    
    [SerializeField] Movement movementScript;

    // transition variables.
    private bool isTransitioning = false; 
    private bool isHoveringLastFrame = false; 

    // focus variables.
    private bool isFocused = false; 
    float focusSpeed = 2f;         
    float focusDistance = 2f;     
    float focusFOV = 30f;
    
    // sitting state check.
    private bool isSitting = false;         

    // original camera state.
    private Vector3 originalCameraPosition; 
    private Quaternion originalCameraRotation; 
    private float originalFOV; 
    
    // ----------------------------------------------------------- before first frame.
    
    void Start()
    {
        // store original camera state.
        originalCameraPosition = monitorCamera.transform.position;
        originalCameraRotation = monitorCamera.transform.rotation;
        originalFOV = monitorCamera.fieldOfView;
        
        // get movement script.
        movementScript = movementScript.GetComponent<Movement>();
        
        // hide focus prompt at start.
        focusPrompt.SetActive(true);   
    }
    
    void Update()
    {
        // check for mouse hover.
        CheckMouseHover();
        
        // update computer screen focus state.
        computerScreen.SetFocused(isFocused);
        
        // update sitting state from computer interaction.
        isSitting = computerInteraction.IsSitting();
    }
    
    // ----------------------------------------------------------- check for hover of monitor.

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
        
        // check if hovering.
        Ray ray = monitorCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;
        bool isHovering = Physics.Raycast(ray, out hit) && hit.collider == GetComponent<Collider>();
        
        // if hovering & not focused & not transitioning, 
        if (isHovering && !isFocused && !isTransitioning)
        {
            // if we weren't hovering, show focus prompt.
            if (!isHoveringLastFrame) focusPrompt.SetActive(true);
            
            // check for mouse click while hovering.
            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame && isSitting)
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
    
    // ----------------------------------------------------------- start & stop focus functions.

    void StartFocus()
    {  
        // hide focus prompt.
        focusPrompt.SetActive(false);

        // set focus to true & start transition.
        isFocused = true;
        isTransitioning = true;
        
        // disable 3D camera.
        threeDCamera.enabled = false;
        threeDCamera.gameObject.SetActive(false);

        // enable monitor camera.
        monitorCamera.enabled = true;

        // disable movement script.
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
        // set focus to false & start transition.
        isFocused = false;
        isTransitioning = true;
        
        // re-enable 3D camera and disable monitor camera when stopping focus
        threeDCamera.enabled = true;
        threeDCamera.gameObject.SetActive(true);
        
        // disable monitor camera.
        monitorCamera.enabled = false;
        
        // re-enable movement script when not focused.
        movementScript.enabled = true;
        
        // re-enable monitor camera movement when not focused. (can look around in monitor view.)
        monitorCamLook.SetAllowLook(true);
        
        // reset cursor position when unfocusing
        computerScreen.ResetCursor();
        
        // smooth return to original view.
        StartCoroutine(SmoothReturnToOriginal());
    }

    // ----------------------------------------------------------- coroutines for smooth focus.
    
    System.Collections.IEnumerator SmoothFocusToTarget()
    {
        // store current camera state. (like the center position, rotation, and FOV.)
        Vector3 startPosition = monitorCamera.transform.position;
        Quaternion startRotation = monitorCamera.transform.rotation;
        float startFOV = monitorCamera.fieldOfView;
        
        // calculate target position.
        Vector3 direction = (focusTarget.position - startPosition).normalized;
        Vector3 targetPosition = focusTarget.position - direction * focusDistance;
        Quaternion targetRotation = Quaternion.LookRotation(focusTarget.position - targetPosition);
        
        float elapsedTime = 0f;
        
        // while elapsed time is less than 1/focus speed, interpolate between start and target.
        while (elapsedTime < 1f / focusSpeed)
        {
            // increment elapsed time by time.deltaTime.
            elapsedTime += Time.deltaTime;
            float t = elapsedTime * focusSpeed;
            
            // smooth interpolation between start and target.
            monitorCamera.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            monitorCamera.transform.rotation = Quaternion.Lerp(startRotation, targetRotation, t);
            monitorCamera.fieldOfView = Mathf.Lerp(startFOV, focusFOV, t);
            
            yield return null;
        }
        
        // ensure final values for position, rotation, and FOV.
        monitorCamera.transform.position = targetPosition;
        monitorCamera.transform.rotation = targetRotation;
        monitorCamera.fieldOfView = focusFOV;
        
        // set transitioning to false.
        isTransitioning = false;
    }
    
    System.Collections.IEnumerator SmoothReturnToOriginal()
    {
        // store current camera state.
        Vector3 startPosition = monitorCamera.transform.position;
        Quaternion startRotation = monitorCamera.transform.rotation;
        float startFOV = monitorCamera.fieldOfView;
        
        // while elapsed time is less than 1/focus speed, interpolate between start and original.
        float elapsedTime = 0f;
        
        while (elapsedTime < 1f / focusSpeed)
        {
            // increment elapsed time by time.deltaTime.
            elapsedTime += Time.deltaTime;
            float t = elapsedTime * focusSpeed;
            
            // smooth interpolation back to original.
            monitorCamera.transform.position = Vector3.Lerp(startPosition, originalCameraPosition, t);
            monitorCamera.transform.rotation = Quaternion.Lerp(startRotation, originalCameraRotation, t);
            monitorCamera.fieldOfView = Mathf.Lerp(startFOV, originalFOV, t);
            
            yield return null;
        }
        
        // ensure final values for position, rotation, and FOV.
        monitorCamera.transform.position = originalCameraPosition;
        monitorCamera.transform.rotation = originalCameraRotation;
        monitorCamera.fieldOfView = originalFOV;
        
        // set transitioning to false.
        isTransitioning = false;
    }
    
}
