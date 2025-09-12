
// handles movement and animation of player.

// Imports.
using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{                                         
    // ground detection.
    [SerializeField] Transform groundCheck;                                  
    [SerializeField] LayerMask ground = 8;

    // cameras.           
    [SerializeField] Transform threeDCam;  

    // body parts.
    [SerializeField] Transform leftArm;
    [SerializeField] Transform rightArm;
    [SerializeField] Transform leftLeg;
    [SerializeField] Transform rightLeg;

    // ground detection.
    float groundCheckRadius = 0.2f; 

    // movement.
    private float Speed = 6.0f;                              
    private float sprintMultiplier = 1.8f;                    
    private Key sprintKey = Key.LeftShift;                    
    private float moveSmoothTime = 0.3f;  
    private float gravity = -30f;                             
    private float jumpHeight = 6f;  

    // animation.    
    private float armSwingAmount = 25f;
    private float legSwingAmount = 15f;
    private float animationSpeed = 6f;
    private float animationTime;

    // velocity.
    float velocityY;   
    bool isGrounded;   
    float jumpVelocity;

    // movement & cursor.
    bool cursorLock = true;     
    CharacterController controller;
    Vector2 currentDir;
    Vector2 currentDirVelocity;
    float currentSpeed;

    // -------------------------------------------------------- Before First Frame.
    private void Start()
    {
        // get the character controller.
        controller = GetComponent<CharacterController>();

        // initialize current speed to base speed.
        currentSpeed = Speed;

        // set 3D cam.
        threeDCam.gameObject.SetActive(true);

        // locks cursor.
        if (cursorLock)
        {
            Cursor.lockState = CursorLockMode.Locked; // middle of screen.
            Cursor.visible = false;                   // not visible.
        }
    }

    // -------------------------------------------------------- Every Frame.
    private void Update()
    {
        UpdateMove();
    }

    // -------------------------------------------------------- update movement.
    void UpdateMove()
    {   
        // casting a small invisible sphere to check if touching any colliders on ground
        bool previousGrounded = isGrounded;
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, ground);
            
        // input values.
        float horizontalInput = 0f;
        float verticalInput = 0f;
        bool jump = false;
        bool isSprinting = false;
        
        // get input values.
        if (Keyboard.current != null)
        {
            if (Keyboard.current.dKey.isPressed) horizontalInput += 1f; // right.
            if (Keyboard.current.aKey.isPressed) horizontalInput -= 1f; // Left.
            if (Keyboard.current.wKey.isPressed) verticalInput += 1f;   // forward.
            if (Keyboard.current.sKey.isPressed) verticalInput -= 1f;   // backward.

            jump = Keyboard.current.spaceKey.wasPressedThisFrame;       // jump.
            isSprinting = Keyboard.current[sprintKey].isPressed;        // sprint.
        }

        // target direction w/ smoothing.
        Vector2 targetDir = new Vector2(horizontalInput, verticalInput);
        targetDir.Normalize();
        currentDir = Vector2.SmoothDamp(currentDir, targetDir, ref currentDirVelocity, moveSmoothTime);

        // handling sprinting.
        if (isSprinting && (Mathf.Abs(horizontalInput) > 0.1f || Mathf.Abs(verticalInput) > 0.1f))
        {
            currentSpeed = Speed * sprintMultiplier; // sprint multiplier.
        }
        else
        {
            currentSpeed = Speed; // return to base speed.
        }

        // gravity. & velocity.
        velocityY += gravity * Time.deltaTime;
        Vector3 velocity = (transform.forward * currentDir.y + transform.right * currentDir.x) * currentSpeed;
        velocity.y = velocityY;
        
        // move player.
        controller.Move(velocity * Time.deltaTime);

        // jump.
        if (isGrounded && jump)
        {
            jumpVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            velocityY = jumpVelocity;
        }

        // caps downward velocity while grounded.
        if(isGrounded && velocityY < -1f)
        {
            velocityY = -8f;
        }
        
        // simple animation - only animate when actually moving
        if (Mathf.Abs(horizontalInput) > 0.1f || Mathf.Abs(verticalInput) > 0.1f)
        {
            UpdateSimpleAnimation();
        }
        else
        {
            ResetAnimationToIdle();
        }
    }


    // -------------------------------------------------------- animated body movement.
    void UpdateSimpleAnimation()
    {
        // animation time update.
        animationTime += Time.deltaTime * animationSpeed;

        // swings arms & legs while moving.
        float leftArmSwing = Mathf.Sin(animationTime) * armSwingAmount;
        leftArm.localEulerAngles = new Vector3(0, 0, leftArmSwing);
    
        float rightArmSwing = Mathf.Sin(animationTime + Mathf.PI) * armSwingAmount;
        rightArm.localEulerAngles = new Vector3(0, 0, rightArmSwing);

        float leftLegSwing = Mathf.Sin(animationTime + Mathf.PI) * legSwingAmount; 
        leftLeg.localEulerAngles = new Vector3(0, 0, leftLegSwing);
    
        float rightLegSwing = Mathf.Sin(animationTime) * legSwingAmount;
        rightLeg.localEulerAngles = new Vector3(0, 0, rightLegSwing);
    }
    
    void ResetAnimationToIdle()
    {
        // reset all body parts to neutral position.
        leftArm.localEulerAngles = Vector3.zero;
        rightArm.localEulerAngles = Vector3.zero;
        leftLeg.localEulerAngles = Vector3.zero;
        rightLeg.localEulerAngles = Vector3.zero;
    }
}
