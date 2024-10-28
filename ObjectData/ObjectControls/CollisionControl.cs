using System.Collections;
using UnityEngine;

public abstract class CollisionControl : MonoBehaviour
{
    /// <summary>
    /// Sets the collision detection mode on the Rigidbody and reverts it after a delay.
    /// </summary>
    /// <param name="objectRigidbody">The Rigidbody to modify.</param>
    internal IEnumerator SetCollisionMode(Rigidbody objectRigidbody)
    {
        if (objectRigidbody == null)
        {
            Debug.LogWarning("Rigidbody is null. Cannot set collision detection mode.");
            yield break;
        }

        objectRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        yield return new WaitForSeconds(10);
        objectRigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }
}