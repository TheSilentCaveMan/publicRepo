using UnityEngine;
[RequireComponent(typeof(BoxCollider))]
public class AIVisionCube : MonoBehaviour
{
    internal AISense aiSense;
    internal float visionPower;
    internal LayerMask playerLayerMask;
    private BoxCollider boxCollider;
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            aiSense.visionTarget = other.transform;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            aiSense.visionTarget = null;
        }
    }
    internal void Initalize()
    {
        boxCollider = GetComponent<BoxCollider>();
        boxCollider.excludeLayers = ~playerLayerMask;
        boxCollider.size = new (visionPower * MainManager.gameConfig.gameDifficulty, visionPower * MainManager.gameConfig.gameDifficulty, visionPower * MainManager.gameConfig.gameDifficulty * 2f);
        Vector3 boxOffset = new (0, 0, visionPower * MainManager.gameConfig.gameDifficulty);
        boxCollider.transform.position += boxOffset;
        aiSense.visionDistance = visionPower * MainManager.gameConfig.gameDifficulty * 2f;
    }

}
