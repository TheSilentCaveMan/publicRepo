using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public enum SlowType { Fixed = 0, Dynamic = 1 };
[RequireComponent(typeof(Collider), typeof(NavMeshModifierVolume))]
public class AINavMeshModifier : MonoBehaviour
{
    public SlowType type;
    public float fixedSpeedModifier = 0;
    private NavMeshModifierVolume volume;
    private float ogSpeed;

    private void Awake()
    {
        volume = GetComponent<NavMeshModifierVolume>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<NavMeshAgent>(out NavMeshAgent agent))
        {
            if (volume.AffectsAgentType(agent.agentTypeID))
            {
                if (type == SlowType.Dynamic)
                {
                    float costModifier = NavMesh.GetAreaCost(volume.area);
                    agent.speed /= costModifier;
                }
                else
                {
                    ogSpeed = agent.speed;
                    agent.speed = fixedSpeedModifier;
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<NavMeshAgent>(out NavMeshAgent agent))
        {
            if (volume.AffectsAgentType(agent.agentTypeID))
            {
                if (type == SlowType.Dynamic)
                {
                    float costModifier = NavMesh.GetAreaCost(volume.area);
                    agent.speed *= costModifier;
                }
                else
                {
                    agent.speed = ogSpeed;
                }
            }
        }
    }
}
