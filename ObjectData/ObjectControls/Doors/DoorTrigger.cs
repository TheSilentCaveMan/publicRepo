using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    public DoorControl door;
    private AIBehaviour aiBehav;
    private bool isEntity = false;

    private void OnTriggerStay(Collider other)
    {
        if (!isEntity && other.gameObject.transform.parent.TryGetComponent(out AIBehaviour detectedAI) && other.transform.parent.CompareTag("Entity"))
        {
            aiBehav = detectedAI;
            isEntity = true;
        }

        if (isEntity)
        {
            HandleDoorInteraction();
        }
    }

    private void HandleDoorInteraction()
    {
        if (door.isOpening || isEntity == false)
        {
            return;
        }

        aiBehav.aiStop = true;
        aiBehav.aiState = AIState.Idle;

        if (!door.locked)
        {
            if (door.doorType == DoorType.Manual)
            {
                door.aiOpen = true;
            }
            door.DoorStateChange();
        }
        else
        {
            aiBehav.aiAttack.attackRadius.AddAttackable(door);
            Debug.Log("Adding to be destroyed");
        }

        if (door.isOpen || !door.gameObject.activeSelf)
        {
            ResetEntityState();
        }
    }

    private void ResetEntityState()
    {
        aiBehav.aiState = AIState.Roam;
        aiBehav.aiStop = false;
        aiBehav = null;
        door.aiOpen = false;
        isEntity = false;
    }
}