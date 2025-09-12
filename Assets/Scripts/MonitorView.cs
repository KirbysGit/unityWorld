// Imports.
using UnityEngine;
using UnityEngine.InputSystem;

public class MonitorCamLook : MonoBehaviour
{
    // camera look.
    float sensitivity = 0.5f; 
    float leftLimit = -60f;   
    float rightLimit = 60f;   
    float bottomLimit = -60f;   
    float topLimit = 60f;   
    bool allowLook = false;   
    float smoothTime = 0.1f;

    // cursor control.
    bool showCursorWhenActive = true; 
    bool keepCursorCentered = true;   

    // camera rotation.
    float currentYaw = 0f;
    float targetYaw = 0f;
    float yawVelocity = 0f;
    float currentPitch = 0f;
    float targetPitch = 0f;
    float pitchVelocity = 0f;
    
    // focused state.
    private bool isFocused = false;

    // -------------------------------------------------------- Every Frame.
    void Update()
    {
        // return if not allowing look or mouse is not active.
        if (!allowLook || Mouse.current == null) return;
        
        // check if camera is enabled
        Camera cam = GetComponent<Camera>();
        if (cam != null && !cam.enabled) return;

        // check if we're in focused mode (looking only at monitor)
        if (isFocused) return;

        // get mouse input.
        float mouseX = Mouse.current.delta.ReadValue().x;
        float mouseY = Mouse.current.delta.ReadValue().y;
        
        // apply sensitivity & update target.
        targetYaw += mouseX * sensitivity;
        targetPitch -= mouseY * sensitivity;
        
        // clamp target to left/right limits.
        targetYaw = Mathf.Clamp(targetYaw, leftLimit, rightLimit);
        targetPitch = Mathf.Clamp(targetPitch, bottomLimit, topLimit);

        // smoothly move current yaw towards target.
        currentYaw = Mathf.SmoothDamp(currentYaw, targetYaw, ref yawVelocity, smoothTime);
        currentPitch = Mathf.SmoothDamp(currentPitch, targetPitch, ref pitchVelocity, smoothTime);
        
        // apply rotation.
        transform.localRotation = Quaternion.Euler(currentPitch, currentYaw, 0f);
    }

    // sets allow look.
    public void SetAllowLook(bool on) 
    {
        allowLook = on;
        
        // handle cursor visibility
        if (showCursorWhenActive)
        {
            if (on)
            {
                // show cursor when monitor is active
                if (keepCursorCentered)
                {
                    // lock cursor when the monitor is active.
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = true;
                }
                else
                {
                    // unlock cursor when not focused.
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
            }
            else
            {
                // hide cursor when not focused.
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }
}