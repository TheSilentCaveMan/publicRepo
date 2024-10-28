using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    //
    // Movement settings
    internal float speed;
    internal float moveSpeed;
    internal float sprintSpeed;
    internal float jumpHeight;
    internal float height;
    internal float crouchHeight;
    internal float crouchSpeed;
    internal float gravity;
    internal float clampX;
    internal float acceleration;

    // Camera settings
    internal float sensitivityX;
    internal float sensitivityY;
    internal bool invertMouse;

    // Stamina settings
    internal float maxStamina = 100f;
    internal float currentStamina = 100f;
    internal float staminaFallRate = 0f;

    // State flags
    internal bool jump;
    internal bool sprint = false;
    internal bool crouch = false;
    internal bool allowRun = false;
    internal bool staminaRest;
    internal bool menuOn = false;
    internal bool radialMenuOn = false;

    // Animation and character components
    public Animator anim;
    internal PlayerConfiguration playerConfiguration;
    internal Transform characterBody;
    internal Transform characterHead;
    internal Transform characterChest;
    private float headRotationX;

    // Internal state management
    private bool crouchState = false;
    private bool jumpControl = false;
    private bool crouchControl;
    private float verticalVelocity;
    private float currentHorizontalSpeed;
    private float rotationX = 0f;

    private const int environmentLayer = 8;

    // Player/Non-player move, if called by non-player vectors must range between -1 & 1
    internal void Move(CharacterController control, Vector2 horizontalInput)
    {
        if (menuOn)
        {
            horizontalInput = Vector2.zero; // Disable movement if menu is open
        }

        UpdateStamina();
        float targetSpeed = DetermineTargetSpeed(horizontalInput);
        float speedOffset = 0.1f;

        UpdateAnimationParameters(horizontalInput);
        AdjustStaminaFallRate(horizontalInput);
        speed = CalculateSpeed(currentHorizontalSpeed, targetSpeed, speedOffset);
        Vector3 direction = GetMovementDirection(horizontalInput);
        Vector3 moveVector = speed * Time.deltaTime * direction.normalized + Vector3.up * verticalVelocity * Time.deltaTime;
        control.Move(moveVector);
        PositionCharacterBody(control);
        anim.SetFloat("speed", speed / sprintSpeed);
    }

    private void UpdateStamina()
    {
        currentStamina = Mathf.Clamp(currentStamina + staminaFallRate, 0f, maxStamina);
        staminaRest = currentStamina <= 0f;
    }

    private float DetermineTargetSpeed(Vector2 horizontalInput)
    {
        allowRun = !staminaRest && horizontalInput.y > 0.1f && !crouchState;
        if (horizontalInput.y == 0 && horizontalInput.x == 0) 
        {
            return 0f;
        }
        return allowRun ? (sprint ? sprintSpeed : moveSpeed) : moveSpeed;
    }

    private void AdjustStaminaFallRate(Vector2 horizontalInput)
    {
        staminaFallRate = horizontalInput == Vector2.zero ? 0.1f : (sprint && allowRun) ? -0.1f : 0.05f;
    }

    private float CalculateSpeed(float currentSpeed, float targetSpeed, float offset)
    {
        if (Mathf.Abs(currentSpeed - targetSpeed) > offset)
        {
            return Mathf.Round(Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * acceleration) * 1000f) / 1000f;
        }
        return targetSpeed;
    }

    private void UpdateAnimationParameters(Vector2 horizontalInput)
    {
        anim.SetFloat("x", horizontalInput.x);
        anim.SetFloat("y", horizontalInput.y);
    }

    private Vector3 GetMovementDirection(Vector2 horizontalInput)
    {
        if (horizontalInput == Vector2.zero)
            return Vector3.zero;

        return transform.right * horizontalInput.x + transform.forward * horizontalInput.y;
    }

    private void PositionCharacterBody(CharacterController control)
    {
        Vector3 newPosition = control.transform.position;
        newPosition.y -= control.height / 2;
        characterBody.position = newPosition;
    }

    private float GetCurrentHorizontalSpeed(CharacterController control)
    {
        float horizontalSpeed = new Vector3(control.velocity.x, 0.0f, control.velocity.z).magnitude;
        return horizontalSpeed;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == environmentLayer)
        {
            SetControls(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == environmentLayer)
        {
            SetControls(false);
        }
    }

    private void SetControls(bool state)
    {
        crouchControl = state;
        jumpControl = state;
    }

    internal void CrouchAction(CharacterController control)
    {
        if (menuOn) return; // prevent action if menu is open

        if (crouch)
        {
            HandleCrouch(control);
        }
        else
        {
            HandleStandUp(control);
        }
    }

    private void HandleCrouch(CharacterController control)
    {
        currentHorizontalSpeed = GetCurrentHorizontalSpeed(control);
        crouchState = true;
        control.height = Mathf.Lerp(control.height, crouchHeight, crouchSpeed);
        anim.SetBool("isCrouch", true);
    }

    private void HandleStandUp(CharacterController control)
    {
        if (crouchState && crouchControl)
        {
            control.height = crouchHeight; // Condition to remain crouched
        }
        else
        {
            crouchState = false;
            // Smooth transition for height
            float heightBefore = control.height;
            control.height = Mathf.Lerp(control.height, height, crouchSpeed);
            // Move upwards to correct position without affecting lateral movement
            float delta = control.height - heightBefore;
            Vector3 moveUp = Vector3.up * (delta / 2);
            // Use Move to handle upward movement separately
            currentHorizontalSpeed = GetCurrentHorizontalSpeed(control);
            control.Move(moveUp);           
            // Ensure standing state when height is within a threshold
            if (Mathf.Abs(control.height - height) < 0.1f)
            {
                anim.SetBool("isCrouch", false);
            }
        }
    }

    internal void JumpAndGravity(CharacterController control)
    {
        if (control.isGrounded)
        {
            HandleGroundedState();
        }
        else
        {
            HandleAirborneState();
        }

        if (control.isGrounded && !menuOn && jump && !jumpControl)
        {
            PerformJump();
        }

        verticalVelocity += gravity * Time.deltaTime;
    }

    private void HandleGroundedState()
    {
        anim.SetBool("isOnGround", true);
        anim.SetBool("isInAir", false);
        verticalVelocity = -0.5f; // Keep grounded
    }

    private void HandleAirborneState()
    {
        jump = false; // Prevent jump input while in air
        anim.SetBool("isOnGround", false);
    }

    private void PerformJump()
    {
        anim.SetTrigger("isJump");
        anim.SetBool("isInAir", true);
        verticalVelocity += Mathf.Sqrt(-3.0f * jumpHeight * gravity);
        jump = false; // Reset jump input
    }

    internal void Look(Vector2 mouseInput, Transform cameraUsed)
    {
        if (menuOn || radialMenuOn) return; // Prevent looking while menu is open
        HandleHorizontalRotation(mouseInput.x);
        HandleVerticalRotation(mouseInput.y, cameraUsed);

        ApplyCharacterBodyRotation();
        ApplyHeadAndChestRotation();
    }

    private void HandleHorizontalRotation(float mouseX)
    {
        transform.Rotate(Vector3.up, mouseX * sensitivityX * Time.deltaTime);
    }

    private void HandleVerticalRotation(float mouseY, Transform cameraUsed)
    {
        rotationX += (invertMouse ? mouseY : -mouseY) * sensitivityY * Time.deltaTime;
        rotationX = Mathf.Clamp(rotationX, -clampX, clampX);

        cameraUsed.rotation = Quaternion.Euler(rotationX, transform.eulerAngles.y, 0);
    }

    private void ApplyCharacterBodyRotation()
    {
        characterBody.rotation = transform.rotation;
    }

    private void ApplyHeadAndChestRotation()
    {
        headRotationX = Mathf.Clamp(rotationX, -10f, 10f);
        characterHead.localRotation = Quaternion.Euler(new Vector3(0, 0, headRotationX));

        if (Mathf.Abs(rotationX) > 10f)
        {
            float chestRotation = Mathf.Clamp(rotationX - headRotationX, -20f, 20f);
            characterChest.localRotation = Quaternion.Euler(new Vector3(0, 0, chestRotation));
        }
    }

    private void Awake()
    {
        characterBody = anim.gameObject.transform;
        characterHead = anim.GetBoneTransform(HumanBodyBones.Head);
        characterChest = anim.GetBoneTransform(HumanBodyBones.Chest);
    }

    private void Start()
    {
        invertMouse = MainManager.systemConfig.invertMouse;
        sensitivityX = MainManager.systemConfig.sensitivityX;
        sensitivityY = MainManager.systemConfig.sensitivityY;
        if (sensitivityX == 0 || sensitivityY == 0)
        {
            sensitivityX = 4.0f;
            sensitivityY = 4.0f;
        }

        if (playerConfiguration != null)
        {
            playerConfiguration.SetControllerData(gameObject);
            playerConfiguration.SetMovementData(gameObject);
            playerConfiguration.SetPhysicsData(gameObject);
        }
        else
        {
            Debug.LogError("PlayerConfiguration is not set.");
        }
    }
}