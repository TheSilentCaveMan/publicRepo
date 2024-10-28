using UnityEngine;
using System.Collections;
using TMPro;

public class PlayerActions : MonoBehaviour
{
    //
    private Animator anim;
    internal Transform holdArea;
    internal Transform playerCam;
    public PlayerGUI playerGUI;
    internal ItemHolder activeItem;
    public PlayerInventory playerInventory;
    private EntityPhysics _physics;
    private const float pickupRange = 1.5f;

    [SerializeField] private LayerMask pickupLayerMask;
    internal ObjectGrabbable objectGrabbable;
    private Coroutine touchRoutine;
    private ItemPickupable objectPickupable;
    public PlayerMovement playerMovement;

    internal bool grab = false;
    internal PlayerControl.GroundMovementActions groundMovement;
    internal bool menuAction = true;
    internal PlayerConfiguration playerConfiguration;
    public LayerMask ignoreRayCastMask;

    public void FireAction(bool isCancel)
    {
        // Exit if menus are active
        if (playerMovement.menuOn || playerGUI.radialMenu.radialMenu.activeSelf)
            return;

        // Handle tossing items
        if (activeItem != null || objectGrabbable != null)
        {
            if (!isCancel)
            {
                playerGUI.throwPowerBar.powerBar.SetActive(true); // Show power bar
            }
            else
            {
                HandleThrowCancel(); // Cancel toss
            }
        }
    }

    private void HandleThrowCancel()
    {
        if (playerGUI.throwPowerBar.powerBar.activeSelf)
        {
            if (objectGrabbable != null)
            {
                ThrowAction(false); // If grabbable object exists, attempt to throw
            }
            else if (activeItem.heldItem.GetComponent<UsableItem>().throwable)
            {
                ThrowAction(true); // If active item is throwable, throw it
            }
            else
            {
                UseAction(); // Otherwise, use the item
            }
            // Hide the power bar
            playerGUI.throwPowerBar.powerBar.SetActive(false);
        }
    }

    public void Fire2Action(bool isCancel)
    {
        // Exit early if the player's menu is active
        if (playerMovement.menuOn)
            return;

        // Hide the throw power bar if it's active
        if (playerGUI.throwPowerBar.powerBar.activeSelf)
        {
            HideThrowPowerBar();
            return;
        }

        // Process menu activation or deactivation based on the cancel state
        if (!isCancel)
        {
            ActivateRadialMenu(); // Show radial menu
        }
        else
        {
            DeactivateRadialMenu(); // Handle deactivation
        }
    }

    private void HideThrowPowerBar()
    {
        playerGUI.throwPowerBar.powerBar.SetActive(false);
        menuAction = false; // Reset menu action state
    }

    private void ActivateRadialMenu()
    {
        playerMovement.radialMenuOn = true;
        playerGUI.radialMenu.menuOn = true;
        playerGUI.radialMenu.radialMenu.SetActive(true);
        playerGUI.partsContainer.SetActive(true);
        playerGUI.questItemMenu.SetActive(true);
        Cursor.visible = true; // Show cursor when the menu is active
    }

    private void DeactivateRadialMenu()
    {
        playerGUI.itemDropMenu.gameObject.SetActive(false);
        playerMovement.radialMenuOn = false;

        // Select the item if not dragging
        if (!playerGUI.radialMenu.dragging)
        {
            activeItem = playerGUI.radialMenu.SelectItem();
        }

        // Close and reset the radial menu
        playerGUI.radialMenu.menuOn = false;
        playerGUI.radialMenu.ResetMenu();
        playerGUI.radialMenu.radialMenu.SetActive(false);
        playerGUI.partsContainer.SetActive(false);
        playerGUI.questItemMenu.SetActive(false);
        playerGUI.cursorHoverObject.SetActive(false);
        Cursor.visible = false; // Hide cursor when menu is not active
    }

    private void ThrowAction(bool isItem)
    {
        Vector3 moveDirection;

        if (isItem)
        {
            moveDirection = playerCam.transform.forward; // Forward direction for item throw

            // Calculate throw strength
            Rigidbody itemRb = activeItem.heldItem.GetComponent<Rigidbody>();
            float throwStrength = CalculateThrowStrength(itemRb);

            // Attempt to throw the item
            if (!activeItem.ThrowItem(playerCam, moveDirection, throwStrength))
            {
                activeItem = null; // Reset active item if throwing fails
            }
        }
        else
        {
            // Throwing a grabbable object
            moveDirection = (objectGrabbable.transform.position - playerCam.transform.position).normalized;
            Rigidbody objectRb = objectGrabbable.GetComponent<Rigidbody>();
            float throwStrength = CalculateGrabbableThrowStrength(objectRb);
            objectRb.AddForce(throwStrength * moveDirection, ForceMode.Impulse);
            objectGrabbable.Drop(); // Perform drop action
        }
    }

    private float CalculateThrowStrength(Rigidbody itemRb)
    {
        // Calculate throw strength based on item mass
        return _physics.strength * playerGUI.throwPowerBar.throwCharge / itemRb.mass;
    }

    private float CalculateGrabbableThrowStrength(Rigidbody objectRb)
    {
        // Calculate throw strength for grabbable object
        return _physics.strength * playerGUI.throwPowerBar.throwCharge / objectRb.mass;
    }

    private void UseAction()
    {
        Vector3 moveDirection = playerCam.transform.forward; // Direction player is facing
        // Attempt to use the active item and clear it if the action fails
        if (!activeItem.UseItem(playerCam, moveDirection))
        {
            activeItem = null; // Reset active item on failure
        }
    }

    public void GrabAndUseAction()
    {
        // Check if the player is trying to grab an object
        if (grab)
        {
            // Only attempt to grab if not currently holding an object
            if (objectGrabbable == null && touchRoutine == null)
            {
                RaycastHit hit;
                // Perform raycast to find a grabbable object
                if (Physics.Raycast(playerCam.position, playerCam.forward, out hit, pickupRange, pickupLayerMask))
                {
                    touchRoutine = StartCoroutine(TouchRoutine(hit)); // Start coroutine only if an object is hit
                }
                else
                {
                    grab = false; // Reset grab state if nothing was hit
                }
            }
        }
        else if (objectGrabbable != null)
        {
            // If not grabbing and holding an object, drop it
            objectGrabbable.Drop(playerGUI.throwPowerBar.powerBar);
            objectGrabbable = null; // Clear the reference after dropping
        }

        // Drop the object if it is too far from the hold area
        if (objectGrabbable != null && Vector3.Distance(holdArea.position, objectGrabbable.transform.position) > 1.5f)
        {
            objectGrabbable.Drop(playerGUI.throwPowerBar.powerBar);
            objectGrabbable = null; // Clear the reference after dropping
        }
    }

    private IEnumerator TouchRoutine(RaycastHit hit)
    {
        yield return null; // Wait for the next frame
        // Check if the hit object is grabbable
        if (hit.transform.TryGetComponent<ObjectGrabbable>(out objectGrabbable))
        {
            InitializeGrabbableObject(hit);
        }
        else if (hit.transform.TryGetComponent<IItemInteractible>(out IItemInteractible interactible))
        {
            HandleInteraction(interactible);
        }
        else if (hit.transform.TryGetComponent<ItemPickupable>(out objectPickupable))
        {
            HandlePickupableObject(objectPickupable);
        }

        // Reset touchRoutine after completing the coroutine
        touchRoutine = null;
    }

    private void InitializeGrabbableObject(RaycastHit hit)
    {
        objectGrabbable.holdPoint = holdArea; // Assign hold point
        objectGrabbable.playerStrength = _physics.strength; // Assign player strength

        // Adjust player's movement speed based on the mass of the grabbable object
        float adjustmentFactor = 1 - (objectGrabbable.mass / _physics.Mass);
        playerMovement.moveSpeed *= adjustmentFactor;
        playerMovement.sprintSpeed *= adjustmentFactor;

        // Position the hold area based on distance
        if (hit.distance <= pickupRange)
        {
            holdArea.transform.localPosition = new Vector3(0, 0, hit.distance);
        }

        objectGrabbable.Grab(holdArea, this.gameObject); // Call grab method on the object
    }

    private void HandleInteraction(IItemInteractible interactible)
    {
        interactible.Interact(this.gameObject);
        grab = false;
    }

    private void HandlePickupableObject(ItemPickupable objectPickupable)
    {
        if (objectPickupable._amount > 0)
        {
            playerInventory.GetItem(objectPickupable.gameObject, objectPickupable._amount); // Add item to inventory
            grab = false; // Reset grab state after picking up
        }
    }

    public string ShootRay()
    {
        Ray ray = new(playerCam.position, playerCam.forward);
        string outText = "";

        // Perform the raycast
        if (Physics.Raycast(ray, out RaycastHit hit, pickupRange, ~ignoreRayCastMask))
        {
            outText = HandleRaycastHit(hit);
        }

        return outText;
    }

    private string HandleRaycastHit(RaycastHit hit)
    {
        string itemText = "";

        // Check if the hit object implements IGUIData
        if (hit.transform.TryGetComponent<IGUIData>(out IGUIData guiData))
        {
            itemText = guiData.itemText;
        }

        // Check if the hit object has OutlineControl and activate it
        if (hit.transform.TryGetComponent<ItemPickupable>(out ItemPickupable itemPickupable))
        {
            itemPickupable.outlineIsOn = true;
        }

        return itemText;
    }

    private void Awake()
    {
        anim = gameObject.GetComponentInChildren<Animator>();
        _physics = gameObject.GetComponent<EntityPhysics>();
        playerInventory = gameObject.GetComponent<PlayerInventory>();
        playerInventory.characterHead = anim.GetBoneTransform(HumanBodyBones.Head);
    }
}