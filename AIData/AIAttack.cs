using System.Collections;
using UnityEngine;
using UnityEngine.AI;
public class AIAttack : MonoBehaviour
{
    internal AIAttackRadius attackRadius;
    internal NavMeshAgent entityNav;
    internal Animator animator;
    internal AIBehaviour aiBehav;
    internal Vector3 originalDest;
    private AIState aiState;
    private Coroutine lookCoroutine = null;
    internal void OnAttack(IAttackable _target)
    {
        aiState = aiBehav.aiState;
        aiBehav.aiState = AIState.Idle;
        animator.SetTrigger("attack");
        if (lookCoroutine != null)
        {
            aiBehav.aiState = aiState;
            entityNav.SetDestination(originalDest);
            StopCoroutine(lookCoroutine);
        }
        lookCoroutine = StartCoroutine(LookAt(_target.GetTransform()));
    }
    private IEnumerator LookAt(Transform _target)
    {
        entityNav.SetDestination(_target.position);        
        while (_target.gameObject.activeSelf)
        {
            yield return null;
        }
        aiBehav.aiState = aiState;
        entityNav.SetDestination(originalDest);
    }
}
