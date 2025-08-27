using UnityEngine;
using UnityEngine.InputSystem;

public class MonitorCamLook : MonoBehaviour
{
    [SerializeField] float mouseSensitivity = 3.5f;
    [SerializeField] float mouseSmoothTime = 0.03f;
    [SerializeField] float maxRotationDistance = 55f; // Maximum rotation distance from center
    [SerializeField] float slowdownStrength = 3f; // How strong the slowdown effect is
    
    private float cameraCap;                    // Stores Camera Angle (How Far Up / Down)
    private float cameraYaw;                    // Stores Camera Angle (How Far Left / Right)
    private Vector2 currentMouseDelta;          // Stores Smoothed Mouse Since Last Frame
    private Vector2 currentMouseDeltaVelocity;  // Used For SmoothDamp
    private bool hasInteracted = false;         // Only allow movement after first interaction

    void Update()
    {
        if (!gameObject.activeInHierarchy) return;  // only runs when monitorCam is active
        if (Mouse.current == null) return;
        if (!hasInteracted) return;  // Don't move until player has interacted

        // Gets Mouse Delta (Reads How Far Mouse Has Moved Since Last Frame)
        Vector2 targetMouseDelta = new Vector2(Mouse.current.delta.ReadValue().x, Mouse.current.delta.ReadValue().y);

        // Smooths Mouse Delta (Just So No Jittery Movement)
        currentMouseDelta = Vector2.SmoothDamp(currentMouseDelta, targetMouseDelta, ref currentMouseDeltaVelocity, mouseSmoothTime);
    
        // Camera Cap (How Far Up / Down)
        cameraCap -= currentMouseDelta.y * mouseSensitivity;

        // Apply continuous slowdown to Camera Cap (Up / Down)
        cameraCap = ApplyContinuousSlowdown(cameraCap, -27.5f, 27.5f);

        // Camera Yaw (How Far Left / Right)
        cameraYaw += currentMouseDelta.x * mouseSensitivity;

        // Apply continuous slowdown to Camera Yaw (Left / Right)
        cameraYaw = ApplyContinuousSlowdown(cameraYaw, -80f, 35f);

        // Apply Both Rotations To Monitor Camera
        transform.localEulerAngles = new Vector3(cameraCap, cameraYaw, 0f);
    }
    
    // Continuous slowdown method - creates gradual deceleration as you turn
    private float ApplyContinuousSlowdown(float value, float min, float max)
    {
        // Calculate how far from center we are (0 = center, 1 = at max distance)
        float range = max - min;
        float center = (max + min) * 0.5f;
        float distanceFromCenter = Mathf.Abs(value - center);
        float maxDistance = range * 0.5f;
        
        // Calculate how far into the rotation we are (0 = center, 1 = at max)
        float rotationProgress = Mathf.Clamp01(distanceFromCenter / maxDistance);
        
        // Apply exponential slowdown (gets slower the further you turn)
        float slowdownFactor = Mathf.Pow(rotationProgress, slowdownStrength);
        
        // Calculate the maximum allowed rotation based on slowdown
        float maxAllowed = maxDistance * (1f - slowdownFactor * 0.8f); // Can still reach limits but very slowly
        
        // Apply the slowdown limit
        if (value > center) {
            return Mathf.Min(value, center + maxAllowed);
        } else {
            return Mathf.Max(value, center - maxAllowed);
        }
    }
    
    // Public method to enable camera movement after interaction
    public void EnableMovement()
    {
        hasInteracted = true;
    }
    
    // Public method to reset camera movement (optional)
    public void ResetMovement()
    {
        hasInteracted = false;
        cameraCap = 0f;
        cameraYaw = 0f;
        currentMouseDelta = Vector2.zero;
        currentMouseDeltaVelocity = Vector2.zero;
    }
}
