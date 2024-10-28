using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerInventory))]
[RequireComponent(typeof(EntityPhysics))]
[RequireComponent(typeof(PlayerActions))]
[RequireComponent(typeof(PlayerConfiguration))]
public class PlayerBehaviour : MonoBehaviour, IAttackable
{
    //
    [Header("Player Components")]
    public PlayerCamera playerCamera;
    public Transform holdArea;

    // Cached components
    internal PlayerInventory playerInventory;
    internal PlayerMovement playerMovement;
    internal PlayerConfiguration playerConfiguration;
    internal CharacterController characterController;
    internal PlayerControl.GroundMovementActions groundMovement;

    // Internal Fields
    internal PlayerActions playerActions;
    internal Vector2 mouseInput;
    private Vector2 horizontalInput;

    // Player Attributes
    internal float playerSmell = 0f;
    private bool isAttackable = true;

    // Properties
    public bool IsAttackable
    {
        get => isAttackable;
        set => isAttackable = value;
    }

    private void Awake()
    {
        CacheComponents();
        SetupPlayerActions();
    }

    private void CacheComponents()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerConfiguration = GetComponent<PlayerConfiguration>();
        characterController = GetComponent<CharacterController>();
        playerInventory = GetComponent<PlayerInventory>();
        playerActions = GetComponent<PlayerActions>();
        groundMovement = MainManager.Instance.controls.GroundMovement;

        // Link configurations to components
        LinkConfigurations();
    }

    private void LinkConfigurations()
    {
        playerMovement.playerConfiguration = playerConfiguration;
        playerCamera.playerBehav = this;
        playerCamera.playerMovement = playerMovement;
        playerActions.playerConfiguration = playerConfiguration;
        playerActions.playerMovement = playerMovement;
        playerActions.playerCam = playerCamera.gameObject.transform;
        playerActions.holdArea = holdArea;
    }

    private void SetupPlayerActions()
    {
        // Subscribe to input actions
        groundMovement.HorizonalMovement.performed += ctx => horizontalInput = ctx.ReadValue<Vector2>();
        groundMovement.Jump.performed += _ => playerMovement.jump = true;
        groundMovement.Sprint.performed += _ => playerMovement.sprint = !playerMovement.sprint;
        groundMovement.MouseX.performed += ctx => mouseInput.x = ctx.ReadValue<float>();
        groundMovement.MouseY.performed += ctx => mouseInput.y = ctx.ReadValue<float>();
        groundMovement.GrabAndUse.performed += _ => playerActions.grab = !playerActions.grab;
        groundMovement.Fire1.performed += _ => playerActions.FireAction(false);
        groundMovement.Fire1.canceled += _ => playerActions.FireAction(true);
        groundMovement.Fire2.performed += _ => playerActions.Fire2Action(false);
        groundMovement.Fire2.canceled += _ => playerActions.Fire2Action(true);
        groundMovement.Crouch.performed += _ => playerMovement.crouch = !playerMovement.crouch;
        groundMovement.Flashlight.performed += _ => playerInventory.UseFlashlight();
    }

    private void FixedUpdate()
    {
        // Physics-related updates
        playerMovement.JumpAndGravity(characterController); // Handle jumping and gravity effects
        playerMovement.CrouchAction(characterController);   // Manage crouching state
        playerMovement.Move(characterController, horizontalInput); // Handle movement
    }

    private void Update()
    {
        // Input handling and non-physics updates
        playerActions.GrabAndUseAction(); // Check for grabbing/using actions
        playerMovement.Look(mouseInput, playerCamera.gameObject.transform); // Handle camera rotation and mouse look
    }

    public Transform GetTransform()
    {
        return transform;
    }

    public void TakeHit()
    {
        EndOfLife();
        Debug.Log("Dead");
    }

    public void EndOfLife()
    {
        gameObject.SetActive(false);
    }

    public GameObject destroyedObject { get; set; }
    public short life { get; set; }
}