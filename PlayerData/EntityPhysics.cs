using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class EntityPhysics : MonoBehaviour
{
    //
    internal float strength;
    internal float carryStrength;
    private float mass;
    private CharacterController characterController;

    private Vector3 impact = Vector3.zero;

    // Property to handle mass, strength, and carry strength calculations
    public float Mass
    {
        get => mass;
        set
        {
            mass = value;
            strength = value * 10;
            carryStrength = value * 2;
        }
    }

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody hitRb = hit.gameObject.GetComponent<Rigidbody>();

        if (hitRb != null)
        {
            if (hit.gameObject.layer == 6) // Collision Layer
            {
                AddImpact(hitRb.velocity * hitRb.mass);
            }
            else if (hit.gameObject.layer == 9) // Grabbable Layer
            {
                PushRigidBodies(hit, hitRb);
            }
        }
    }

    private void PushRigidBodies(ControllerColliderHit hit, Rigidbody objRb)
    {
        if (hit.moveDirection.y < -0.3f || objRb.isKinematic) return;

        Vector3 pushDir = new(hit.moveDirection.x, 0f, hit.moveDirection.z);
        objRb.AddForce(pushDir * strength / 2, ForceMode.Force);
    }

    private void AddImpact(Vector3 force)
    {
        Vector3 direction = force.normalized;
        direction.y = 0.5f; // Adjust influence direction towards y-axis
        impact += direction * (force.magnitude / mass); // Add magnitude relative to mass
    }

    private void FixedUpdate()
    {
        if (impact.magnitude > 0.2f)
        {
            characterController.Move(impact * Time.deltaTime); // Apply the calculated impact
        }

        // Gradually reduce impact to simulate friction
        impact = Vector3.Lerp(impact, Vector3.zero, 5 * Time.deltaTime);
    }
}