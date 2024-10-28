using System.Collections.Generic;
using UnityEngine;
public class AIAutoCrouch : MonoBehaviour
{
    internal CharacterController aiCharController;
    internal AIMove aiMove;
    internal float height;
    internal LayerMask environmentLayerMask;
    private bool firstEntry = true;
    private List<Collider> obstacles = new();
    private Collider collider;
    internal void Initalize()
    {
        collider = GetComponent<Collider>();
        collider.excludeLayers = ~environmentLayerMask;
    }
    private void OnTriggerEnter(Collider other)
    {
        obstacles.Add(other);
        if (firstEntry)
        {
            Debug.Log("Entering auto-crawl at " + other.transform.name);
            firstEntry = false;
            aiMove.Crawling(true);
            aiCharController.height /= 2;
            aiCharController.center = new(0f, height / 4, 0f);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        obstacles.Remove(other);
        if (obstacles.Count == 0)
        {
            Debug.Log("Exitting auto-crawl at " + other.transform.name);
            firstEntry = true;
            aiMove.Crawling(false);
            aiCharController.height = height;
            aiCharController.center = new(0f, height / 2, 0f);
        }
    }
}