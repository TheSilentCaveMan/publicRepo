using UnityEngine;

public class MolotovCocktailScript : UsableItem
{
    public override void Use()
    {
        // Implement specific Molotov behavior, e.g., ignite an area or spawn fire particles
        Debug.Log("Molotov has been used: triggering effects.");
    }

    internal override void Thrown()
    {
        _amount = 0;
        StartCoroutine(CollisionSet());
        inEffect = false;
        Debug.Log("Molotov has been thrown.");
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Check if the Molotov should "pop" (trigger effects) immediately upon collision
        if (popDelay == 0f && throwable && !inEffect)
        {
            inEffect = true;
            Initialize();
            Debug.Log("Molotov collided and effects initialized.");
        }
    }
}