using UnityEngine;

public class PlayerConfiguration : MonoBehaviour
{
    //
    internal float moveSpeed = 4.0f;
    internal float sprintSpeed = 8.0f;
    internal float jumpHeight = 0.7f;
    internal float acceleration = 10.0f;
    internal float height;
    internal float crouchHeight;
    internal float mass = 65f;
    internal float clampX = 75f;
    internal float gravity = -9.81f;
    internal float crouchSpeed = 0.2f;

    private void Start()
    {
        // Optionally you can remove PopulatePlayerConfiguration method
        // because we have assigned default values during declaration.
    }

    public bool SetMovementData(GameObject entity)
    {
        if (!entity.TryGetComponent(out PlayerMovement movement))
        {
            return false; // Return false if the component is not found
        }

        if (entity.CompareTag("Player"))
        {
            // Set specific settings for player
            movement.gravity = gravity;
            // Any player-specific logic can go here
        }
        else if (entity.CompareTag("MOB"))
        {
            // Any mob-specific logic can go here (currently not used)
        }

        // Set common movement properties
        movement.height = height;
        movement.crouchHeight = crouchHeight;
        movement.acceleration = acceleration;
        movement.moveSpeed = moveSpeed;
        movement.sprintSpeed = sprintSpeed;
        movement.jumpHeight = jumpHeight;
        movement.clampX = clampX;
        movement.crouchSpeed = crouchSpeed;

        return true; // Return true if successful
    }

    public bool SetPhysicsData(GameObject entity)
    {
        if (!entity.TryGetComponent(out EntityPhysics physics))
        {
            return false; // Return false if the component is not found
        }

        // Setting physics mass
        physics.Mass = mass;
        return true; // Return true if successful
    }

    public bool SetControllerData(GameObject entity)
    {
        if (entity.CompareTag("Player") && entity.TryGetComponent(out CharacterController control))
        {
            height = control.height;
            crouchHeight = height / 2;
            return true; // Return true if successful
        }

        return false; // Return false if not a player or component not found
    }
}