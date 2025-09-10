using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{

    [SerializeField][Range(0.0f, 0.5f)] float mouseSmoothTime = 0.03f; // Smooth Time For Mouse.
    [SerializeField] bool cursorLock = true;                           // Lock / Hide Cursor For mouseLook.
    [SerializeField] float mouseSensitivity = 3.5f;                    // Multiplier For Mouse Sensitivity.
    [SerializeField] float Speed = 6.0f;                               // Movement Speed.
    [SerializeField] float sprintMultiplier = 1.8f;                    // Speed multiplier when sprinting.
    [SerializeField] Key sprintKey = Key.LeftShift;                    // Key to hold for sprinting.
    [SerializeField][Range(0.0f, 0.5f)] float moveSmoothTime = 0.3f;   // Smooth Time For Movement Input.
    [SerializeField] float gravity = -30f;                             // Downward Acceleration.
    [SerializeField] float jumpHeight = 6f;                            // Jump Height.

    // Ground Detection.
    [SerializeField] Transform groundCheck;          // A Point.                           
    [SerializeField] LayerMask ground = 8;           // A Layer For What Counts As Ground. (Layer 3 = 8 in binary)
    [SerializeField] float groundCheckRadius = 0.2f; // Sphere Radius For Checking Ground.

    // Cameras.
    // [SerializeField] Transform playerCam;  // Player Camera (First Person) - DISABLED.
    [SerializeField] Transform threeDCam;  // External Camera (Third Person).

    // Simple Animation System
    [Header("Simple Animation")]
    [SerializeField] Transform leftArm;
    [SerializeField] Transform rightArm;
    [SerializeField] Transform leftLeg;
    [SerializeField] Transform rightLeg;
    [SerializeField] float armSwingAmount = 25f;
    [SerializeField] float legSwingAmount = 15f;
    [SerializeField] float animationSpeed = 6f;
    private float animationTime;

    float velocityY;   // Stores Vertical Speed Across Frames.
    bool isGrounded;   // Checks If Player Is On The Ground.
    float jumpVelocity; // Stores calculated jump velocity
    
    // Camera.
    // float cameraCap;                    // Stores Camera Angle. (How Far Up / Down) - DISABLED.
    // bool isPlayerCamActive = false;     // Camera State Toggle - DISABLED (3D camera only).
    // Vector2 currentMouseDelta;          // Stores Smoothed Mouse Since Last Frame - DISABLED.
    // Vector2 currentMouseDeltaVelocity;  // Used For SmoothDamp - DISABLED.

    // Movement & Mouse.
    CharacterController controller; // "Capsule Mover"
    Vector2 currentDir;             // Smoothed Dir (WASD But Like Eased Out)
    Vector2 currentDirVelocity;     // Used For SmoothDamp.
    float currentSpeed;             // Current movement speed (base or sprint).

    // Debug.
    bool debug = false; // Set to true to enable jump debugging

    // Start. Called Before First Frame.
    private void Start()
    {
        // Gets GameObject The Script Is Attached To.
        controller = GetComponent<CharacterController>();

        // Initialize current speed to base speed.
        currentSpeed = Speed;

        // Initialize camera state - ensure player camera is active by default
        InitializeCameraState();

        // Locks Cursor.
        if (cursorLock)
        {
            Cursor.lockState = CursorLockMode.Locked; // Stuck In Middle Of Screen.
            Cursor.visible = false;                   // Pointer Is Not Visible.
        }

        if (debug)
        {
            Debug.Log($"=== STARTUP POSITIONS ===");
            Debug.Log($"Player capsule position: {transform.position}");
            if (groundCheck != null)
            {
                Debug.Log($"GroundCheck position: {groundCheck.position}");
            }
            else
            {
                Debug.LogWarning("GroundCheck is null at startup!");
            }
            Debug.Log($"Ground LayerMask value: {ground.value}");
            Debug.Log($"Ground LayerMask binary: {System.Convert.ToString(ground.value, 2)}");
            Debug.Log($"=========================");
        }
    }

    // Continuous. Once Every Frame.
    private void Update()
    {
        // UpdateMouse();          // Updates Mouse Look - DISABLED (3D camera only).
        UpdateMove();           // Updates Movement.
        // CheckCameraSwitch();    // Checks If Camera Should Be Switched - DISABLED (3D camera only).
    }

    // Updates Mouse Look - DISABLED (3D camera only).
    /*
    void UpdateMouse()
    {
        // Only allow mouse look if the player camera is ACTIVE (first person mode)
        if (!isPlayerCamActive || playerCam == null || !playerCam.gameObject.activeInHierarchy) {
            return; // Exit early if not in first person mode or camera is not active
        }
        
        // Debug camera state if there are issues
        if (playerCam != null) {
            Camera playerCamera = playerCam.GetComponent<Camera>();
            if (playerCamera != null && !playerCamera.enabled) {
                Debug.LogWarning($"Movement: PlayerCam is disabled but gameObject is active! Camera: {playerCamera.name}");
            }
        }
        
        // Gets Mouse Delta. (Reads How Far Mouse Has Moved Since Last Frame)
        Vector2 targetMouseDelta = new Vector2(Mouse.current.delta.ReadValue().x, Mouse.current.delta.ReadValue().y);

        // Smooths Mouse Delta. (Just So No Jittery Movement)
        currentMouseDelta = Vector2.SmoothDamp(currentMouseDelta, targetMouseDelta, ref currentMouseDeltaVelocity, mouseSmoothTime);
    
        // Camera Cap. (How Far Up / Down)
        cameraCap -= currentMouseDelta.y * mouseSensitivity;

        // Clamps Camera Cap. (So It Doesn't Go Too Far Up / Down)
        cameraCap = Mathf.Clamp(cameraCap, -90f, 90f);

        // Apply Vertical Rotation To PlayerCam.
        playerCam.localEulerAngles = Vector3.right * cameraCap;

        // Rotates Player Upon Horizontal Mouse Movement.
        transform.Rotate(Vector3.up * currentMouseDelta.x * mouseSensitivity);
    }
    */

    // Updates Movement.
    void UpdateMove()
    {
        // Movement works with 3D camera only (no first person mode)
        
        // Checks If GroundCheck Is Null.
        // (Casting A Small Invisible Sphere To Check If Touching Any Colliders On Ground)
        if (groundCheck != null)
        {
            bool previousGrounded = isGrounded;
            isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, ground);
            
            // Debug ground detection changes
            if (previousGrounded != isGrounded)
            {
                Debug.Log($"=== GROUND STATE CHANGED ===");
                Debug.Log($"Ground state changed from {previousGrounded} to {isGrounded}");
                Debug.Log($"GroundCheck position: {groundCheck.position}");
                Debug.Log($"GroundCheck radius: {groundCheckRadius}");
                Debug.Log($"Ground layer mask: {ground.value}");
            }
        }
        else
        {
            isGrounded = false;
            Debug.LogWarning("GroundCheck is null - player will never be grounded!");
        }

        // Input Values.
        float horizontalInput = 0f;
        float verticalInput = 0f;

        bool jump = false;
        bool isSprinting = false;
        
        // Gets Input Values.
        if (Keyboard.current != null)
        {
            if (Keyboard.current.dKey.isPressed) horizontalInput += 1f; // Right.
            if (Keyboard.current.aKey.isPressed) horizontalInput -= 1f; // Left.
            if (Keyboard.current.wKey.isPressed) verticalInput += 1f;   // Up.
            if (Keyboard.current.sKey.isPressed) verticalInput -= 1f;   // Down.

            jump = Keyboard.current.spaceKey.wasPressedThisFrame;
            isSprinting = Keyboard.current[sprintKey].isPressed; // Check if sprint key is held.
            
            // Debug jump input
            if (jump)
            {
                Debug.Log("=== JUMP INPUT DETECTED ===");
                Debug.Log($"Spacebar pressed - jump = {jump}");
                Debug.Log($"Current isGrounded state: {isGrounded}");
                Debug.Log($"GroundCheck exists: {groundCheck != null}");
                if (groundCheck != null)
                {
                    Debug.Log($"GroundCheck position: {groundCheck.position}");
                    Debug.Log($"GroundCheck radius: {groundCheckRadius}");
                    Debug.Log($"Ground layer mask: {ground.value}");
                }
            }
        }

        // Target Direction. (WASD)
        Vector2 targetDir = new Vector2(horizontalInput, verticalInput);

        // Smooths Target Direction.
        targetDir.Normalize();

        // Smooths Current Direction.
        currentDir = Vector2.SmoothDamp(currentDir, targetDir, ref currentDirVelocity, moveSmoothTime);

        // Handle sprinting - update current speed based on sprint state.
        if (isSprinting && (Mathf.Abs(horizontalInput) > 0.1f || Mathf.Abs(verticalInput) > 0.1f))
        {
            currentSpeed = Speed * sprintMultiplier; // Apply sprint multiplier when moving and sprinting.
        }
        else
        {
            currentSpeed = Speed; // Return to base speed when not sprinting or not moving.
        }

        // Gravity. (Each Frame Add Gravity To Vertical Velocity)
        velocityY += gravity * Time.deltaTime;

        // Velocity. (Direction * Speed)
        Vector3 velocity = (transform.forward * currentDir.y + transform.right * currentDir.x) * currentSpeed;
        velocity.y = velocityY;
        
        // Moves Player.
        if (controller != null)
        {
            controller.Move(velocity * Time.deltaTime);
        }

        // Checks If Jump Is Executed.
        if (isGrounded && jump)
        {
            Debug.Log("=== JUMP EXECUTING ===");
            Debug.Log($"Jump conditions met: isGrounded={isGrounded}, jump={jump}");
            Debug.Log($"Jump parameters: jumpHeight={jumpHeight}, gravity={gravity}");
            
            jumpVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            Debug.Log($"Calculated jumpVelocity: {jumpVelocity}");
            
            velocityY = jumpVelocity;
            Debug.Log($"Applied jumpVelocity to velocityY: {velocityY}");
            Debug.Log("=== JUMP COMPLETE ===");
        }
        else if (jump && !isGrounded)
        {
            Debug.Log("=== JUMP BLOCKED ===");
            Debug.Log($"Jump blocked: isGrounded={isGrounded}, jump={jump}");
            Debug.Log("Player is not grounded, cannot jump!");
        }

        // Caps Downward Velocity While Grounded.
        if(isGrounded && velocityY < -1f)
        {
            velocityY = -8f;
        }
        
        // Simple Animation - only animate when actually moving
        if (Mathf.Abs(horizontalInput) > 0.1f || Mathf.Abs(verticalInput) > 0.1f)
        {
            UpdateSimpleAnimation();
        }
        else
        {
            ResetAnimationToIdle();
        }
    }

    // Checks If Camera Should Be Switched - DISABLED (3D camera only).
    /*
    void CheckCameraSwitch()
    {
        // If C Is Pressed.
        if (Keyboard.current != null && Keyboard.current.cKey.wasPressedThisFrame)
        {
            SwitchCamera();
        }
    }
    */

    // Initialize camera state at startup - 3D camera only
    void InitializeCameraState()
    {
        // Always use 3D camera (no first person mode)
        if (threeDCam != null)
        {
            threeDCam.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Movement: threeDCam is not assigned! Please assign it in the inspector.");
        }
    }
    
    // Public method to reinitialize camera state (called by other scripts)
    public void ReinitializeCameraState()
    {
        // Re-enable the Movement script
        enabled = true;
        
        // Reinitialize camera state (3D camera only)
        InitializeCameraState();
        
        Debug.Log("Movement: Camera state reinitialized. Using 3D camera only.");
    }

    // Switches Camera - DISABLED (3D camera only).
    /*
    void SwitchCamera()
    {
        // Checks If PlayerCam And ThreeDCam Are Assigned.
        if (playerCam != null && threeDCam != null)
        {
            // Toggles Camera State.
            isPlayerCamActive = !isPlayerCamActive;
            
            // If PlayerCam Is Active.
            if (isPlayerCamActive)
            {
                // Switch to PlayerCam (Inside Player)
                playerCam.gameObject.SetActive(true);
                threeDCam.gameObject.SetActive(false);
                Debug.Log("Switched to Player Camera");
            }
            else
            {
                // Switch to 3DCam (Outside Player)
                playerCam.gameObject.SetActive(false);
                threeDCam.gameObject.SetActive(true);
                Debug.Log("Switched to 3D Camera");
            }
        }
        else
        {
            Debug.LogWarning("One or both cameras not assigned in Movement script!");
        }
    }
    */
    
    // Simple Animation Methods
    void UpdateSimpleAnimation()
    {
        // Only animate if we have body parts assigned
        if (leftArm == null && rightArm == null && leftLeg == null && rightLeg == null)
            return;
            
        // Increment animation time
        animationTime += Time.deltaTime * animationSpeed;
        
        // Simple arm swinging (opposite directions)
        if (leftArm != null)
        {
            float swing = Mathf.Sin(animationTime) * armSwingAmount;
            leftArm.localEulerAngles = new Vector3(0, 0, swing);
        }
        
        if (rightArm != null)
        {
            float swing = Mathf.Sin(animationTime + Mathf.PI) * armSwingAmount; // Opposite phase
            rightArm.localEulerAngles = new Vector3(0, 0, swing);
        }
        
        // Simple leg swinging (opposite directions)
        if (leftLeg != null)
        {
            float swing = Mathf.Sin(animationTime + Mathf.PI) * legSwingAmount; // Opposite phase
            leftLeg.localEulerAngles = new Vector3(0, 0, swing);
        }
        
        if (rightLeg != null)
        {
            float swing = Mathf.Sin(animationTime) * legSwingAmount;
            rightLeg.localEulerAngles = new Vector3(0, 0, swing);
        }
    }
    
    void ResetAnimationToIdle()
    {
        // Reset all body parts to neutral position
        if (leftArm != null)
            leftArm.localEulerAngles = Vector3.zero;
        if (rightArm != null)
            rightArm.localEulerAngles = Vector3.zero;
        if (leftLeg != null)
            leftLeg.localEulerAngles = Vector3.zero;
        if (rightLeg != null)
            rightLeg.localEulerAngles = Vector3.zero;
    }
}
