using UnityEngine;
using UnityEngine.AI;
public class AISense : MonoBehaviour
{
    internal float aiSmell;
    internal float aiHearing;
    internal LayerMask senseLayerMask;
    internal AIBehaviour aiBehav;
    internal Transform visionTarget;
    internal NavMeshAgent entityNav;
    internal AISixthSense defaultAISixthSense;
    internal float visionDistance;
    internal Transform aiHead;
    private float targetDistance = 0;
    private GameObject target;
    private PlayerBehaviour playerBehaviour;
    private AISixthSense _aiSixthSense;
    internal AISixthSense aiSixthSense
    {
        get
        {
            return _aiSixthSense;
        }
        set
        {
            _aiSixthSense = value;
        }
    }
    internal virtual void CallSense(GameObject[] players)
    {
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] != null)
            {
                float distance = Vector3.Distance(transform.position, players[i].transform.position);
                playerBehaviour = players[i].GetComponent<PlayerBehaviour>();
                if ((Smell(playerBehaviour, distance) || Hear() || Look(players[i])) && CanChase(players[i]))
                {
                    if (targetDistance < distance)
                    {
                        target = players[i];
                        targetDistance = distance;
                    }
                }
                else
                {
                    targetDistance = 0;
                    target = null;
                }
            }
        }
        if (aiBehav.isFedPath)
        {
            if (!aiBehav.aiStop)
            {
                if (CheckPlayerItem())
                {
                    Debug.Log("Is that item");
                    aiBehav.aiState = AIState.Careful;
                }
                else
                {
                    Debug.Log("Is not that item");
                    aiBehav.aiState = AIState.Chase;
                }
            }
            aiBehav.target = target;
        }
        else
        {
            if (!aiBehav.aiStop)
            {
                aiBehav.aiState = AIState.Roam;
            }
            aiBehav.target = null;
        }
    }
    internal virtual bool Smell(PlayerBehaviour playerData, float distance)
    {
        if (playerData.playerSmell * distance < aiSmell * MainManager.gameConfig.gameDifficulty)
        {
            Debug.Log("Smells Player");
            return true;
        }
        else
        {
            return false;
        }
    }
    internal virtual bool Hear()
    {
        return false;
    }
    internal virtual bool Look(GameObject player)
    {
        if (visionTarget != null)
        {
            Vector3 lookDirection = (player.transform.position - aiHead.transform.position).normalized;
            RaycastHit hit;
            if (Physics.Raycast(aiHead.transform.position, lookDirection, out hit, visionDistance, senseLayerMask))
            {
                if (hit.transform.CompareTag("Player"))
                {
                    Debug.Log("Sees Player");
                    return true;
                }            
                else { return false; }
            }
        }
        return false;
    }
    private NavMeshHit SamplePath(GameObject _target, Vector3 pos)
    {
        NavMeshHit hit = new();
        NavMesh.SamplePosition(pos, out hit, aiBehav.aiAttack.attackRadius.rangeCollider.radius, NavMesh.AllAreas);
        return hit;
    }
    private Vector3 OffsetPosition(GameObject _target)
    {
        float offsetX = Random.Range(0, 1);
        float offsetY = Random.Range(0, 1);
        Vector3 pos = _target.transform.position;
        pos.x += offsetX;
        pos.y += offsetY;
        return pos;
    }
    private bool CheckPlayerItem()
    {
        if (playerBehaviour.playerActions.activeItem != null)
        {
            if (playerBehaviour.playerActions.activeItem.heldItem.CompareTag(aiBehav.entityWeakness))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        return false;
    }
    private void FeedPath(NavMeshHit hit, Vector3 pos)
    {
        aiBehav.isFedPath = true;
        Vector3 destination = hit.position;
        Vector3 edgeCorrection = pos - destination;
        destination += edgeCorrection.normalized * (entityNav.radius / 2);
        aiBehav.aiMove.fedPath = destination;
    }
    internal virtual bool CanChase(GameObject _target)
    {
        Vector3 orgPos = OffsetPosition(_target);
        NavMeshHit hit = SamplePath(_target, orgPos);
        if (hit.hit)
        {
            Debug.Log("CanChase");
            FeedPath(hit, orgPos);
            return true;
        }
        else 
        {
            aiBehav.isFedPath = false;
            return false; 
        }
    }
    internal virtual void CallSixthSense(AISixthSense type, GameObject _target)
    {
        if(_aiSixthSense == type)
        {
            Vector3 orgPos = OffsetPosition(_target);
            NavMeshHit hit = SamplePath(_target, orgPos);
            if (hit.hit)
            {
                Debug.Log("SixthSenseInvestigation");
                FeedPath(hit, orgPos);
            }
        }
    }
}
