using UnityEngine;
using UnityEngine.InputSystem;

public class MonitorCamLook : MonoBehaviour
{
    [Header("Simple Settings")]
    [SerializeField] float sensitivity = 0.5f;  // much lower sensitivity
    [SerializeField] float leftLimit = -60f;   // look left limit
    [SerializeField] float rightLimit = 60f;   // look right limit
    [SerializeField] float bottomLimit = -60f;   // look down limit
    [SerializeField] float topLimit = 60f;   // look up limit
    [SerializeField] bool allowLook = false;   // enable/disable looking
    
    [Header("Smoothing")]
    [SerializeField] float smoothTime = 0.1f;  // much faster response
    
    [Header("Cursor Control")]
    [SerializeField] bool showCursorWhenActive = true;  // Show cursor when monitor is active
    [SerializeField] bool keepCursorCentered = true;    // Keep cursor centered when monitor is active

    float currentYaw = 0f;
    float targetYaw = 0f;
    float yawVelocity = 0f;
    float currentPitch = 0f;
    float targetPitch = 0f;
    float pitchVelocity = 0f;
    
    private bool isFocused = false;

    void Update()
    {
        // Return If Not Allowing Look Or Mouse Is Not Active.
        if (!allowLook || Mouse.current == null) return;
        
        // Check if the camera is enabled - don't accept input if camera is disabled
        Camera cam = GetComponent<Camera>();
        if (cam != null && !cam.enabled) return;

        // Check if we're in focused mode (camera movement disabled)
        if (isFocused)
        {
            // In focused mode, don't move the camera - cursor can move freely
            return;
        }

        // Get Mouse Input.
        float mouseX = Mouse.current.delta.ReadValue().x;
        float mouseY = Mouse.current.delta.ReadValue().y;
        
        // Apply Sensitivity & Update Target.
        targetYaw += mouseX * sensitivity;
        targetPitch -= mouseY * sensitivity;  // Inverted vertical movement
        
        // Clamp Target To Left/Right Limits.
        targetYaw = Mathf.Clamp(targetYaw, leftLimit, rightLimit);
        targetPitch = Mathf.Clamp(targetPitch, bottomLimit, topLimit);

        // Smoothly Move Current Yaw Towards Target.
        currentYaw = Mathf.SmoothDamp(currentYaw, targetYaw, ref yawVelocity, smoothTime);
        currentPitch = Mathf.SmoothDamp(currentPitch, targetPitch, ref pitchVelocity, smoothTime);
        
        // Apply Rotation.
        transform.localRotation = Quaternion.Euler(currentPitch, currentYaw, 0f);
    }

    // Sets Allow Look.
    public void SetAllowLook(bool on) 
    {
        allowLook = on;
        
        // Handle cursor visibility
        if (showCursorWhenActive)
        {
            if (on)
            {
                // Show cursor when monitor is active
                if (keepCursorCentered)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = true;
                }
                else
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
            }
            else
            {
                // Hide cursor when monitor is inactive
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }
    
    // Sets focused state (called by MonitorFocus script)
    public void SetFocused(bool focused)
    {
        isFocused = focused;
    }
}
