using System.Collections;
using UnityEngine;
using UnityEngine.AI;
public class AIMove : MonoBehaviour
{
    //GENERIC
    internal float idleTime;
    internal float moveDistance;
    internal Animator animator;
    internal AIBehaviour aiBehav;
    internal Vector3 fedPath;
    internal NavMeshAgent entityNav;
    internal Transform entityTransform;
    //SPEEDCHANGE
    private Coroutine speedChangeCoroutine;
    private float speedMultiplier = 0f;
    //STOP
    private bool isMove = false;
    private GameObject target;
    private bool isJump = false;
    private Coroutine waitCoroutine;
    //ATTRIBUTES
    internal float ogSpeed;
    internal float ogAngularSpeed;
    internal float ogAcceleration;
    private float expectedSpeed;
    private void Update()
    {
        AnimationData();
    }
    internal virtual void CallMove(GameObject _target)
    {
        if (!IsEndOfPath())
        {
            if (aiBehav.isFedPath)
            {
                if (aiBehav.isFedPath)
                {
                    Move(fedPath);
                }
                isMove = true;
            }
            if (!isMove)
            {
                Move(RandomMoveData());
                isMove = true;
            }
        }
    }
    internal virtual void Move(Vector3 movePosition) 
    {
        NavMeshPath path = new();        
        entityNav.CalculatePath(movePosition, path);
        entityNav.SetPath(path);
    }
    private void AnimationData() 
    {
        if (entityNav.isOnOffMeshLink)
        {
            if (!isJump)
            {
                isJump = true;
                animator.SetTrigger("isJump");
                animator.SetBool("isOnGround", false);
                animator.SetBool("isInAir", true);
            }
        }
        else
        {
            isJump = false;
            animator.SetBool("isOnGround", true);
            animator.SetBool("isInAir", false);
        }
        //Set rotation
        Vector3 currentRotation = entityNav.transform.InverseTransformDirection(entityNav.velocity).normalized;
        animator.SetFloat("rotation", currentRotation.x);
        //Set speed
        float speedDifference = 1;
        if (expectedSpeed > 0)
        {
            speedDifference = entityNav.velocity.magnitude / expectedSpeed;
        }
        if (speedDifference > 1)
        {
            speedDifference = 1;
        }
        float topSpeed = ogSpeed * 2;
        float currentSpeed = entityNav.speed;
        animator.SetFloat("speed", currentSpeed / topSpeed);
    }
    internal void Crawling(bool isCrouch)
    {
        if (isCrouch)
        {
            expectedSpeed = ogSpeed * 0.5f;
            entityNav.speed = expectedSpeed;
            entityNav.angularSpeed *= 0.5f;
            entityNav.acceleration *= 0.5f;
            entityNav.stoppingDistance = 0;
            animator.SetTrigger("isCrawl");
            animator.SetBool("isNotCrawl", false);
        }
        else
        {
            animator.SetBool("isNotCrawl", true);
            aiBehav.aiState = AIState.Roam;
        }
    } 
    internal void IdleMode()
    {
        expectedSpeed = 0f;
        entityNav.speed = expectedSpeed;
        entityNav.acceleration = ogAcceleration * 2f;
        entityNav.stoppingDistance = 0;
    }
    internal void RoamMode()
    {
        expectedSpeed = ogSpeed;
        entityNav.speed = expectedSpeed;
        entityNav.angularSpeed = ogAngularSpeed;
        entityNav.acceleration = ogAcceleration;
        entityNav.stoppingDistance = 0;
    }
    internal void ChaseMode()
    {
        expectedSpeed = ogSpeed * 2f;
        entityNav.speed = expectedSpeed;
        entityNav.angularSpeed = ogAngularSpeed * 2f;
        entityNav.acceleration = ogAcceleration * 2f;
        entityNav.stoppingDistance = 0;
    }
    internal void CarefulMode()
    {
        if (speedChangeCoroutine == null)
        {
            speedChangeCoroutine = StartCoroutine(SpeedUpChange(2f));
        }
        entityNav.stoppingDistance = 0;
    }
    internal void StunnedMode()
    {
        animator.SetBool("isStunned", true);
    }
    private Vector3 RandomMoveData()
    {
        Vector3 direction = Random.insideUnitSphere * moveDistance;
        direction += transform.position;
        NavMeshHit hit;
        NavMesh.SamplePosition(direction, out hit, moveDistance, 1);
        return hit.position;
    }
    private IEnumerator SpeedUpChange(float expectedMultiplier)
    {
        entityNav.acceleration = ogAcceleration * 2f;
        entityNav.angularSpeed = ogAngularSpeed * 2f;
        while (speedMultiplier <= expectedMultiplier)
        {
            speedMultiplier += 0.1f;
            expectedSpeed = ogSpeed * speedMultiplier;
            entityNav.speed = expectedSpeed;
            Debug.Log(entityNav.speed);
            WaitForSeconds _waitTime = new WaitForSeconds(1f);
            yield return _waitTime;
            if(aiBehav.aiState != AIState.Careful)
            {
                break;
            }
        }
        if (speedChangeCoroutine != null)
        {
            StopCoroutine(speedChangeCoroutine);
            speedChangeCoroutine = null;
        }
    }
    private IEnumerator Wait(bool endOfPath)
    {
        aiBehav.aiState = AIState.Idle;
        aiBehav.aiStop = true;
        WaitForSeconds _waitTime = new WaitForSeconds(idleTime);
        yield return _waitTime;
        aiBehav.aiStop = false;
        if (endOfPath)
        {
            Move(RandomMoveData());
            isMove = true;
        }
        StopCoroutine(waitCoroutine);
        waitCoroutine = null;
    }
    private IEnumerator Wait(float delay)
    {
        aiBehav.aiState = AIState.Idle;
        aiBehav.aiStop = true;
        WaitForSeconds _waitTime = new WaitForSeconds(delay);
        yield return _waitTime;
        aiBehav.aiStop = false;
        StopCoroutine(waitCoroutine);
        waitCoroutine = null;
    }
    private bool IsEndOfPath()
    {
        if(aiBehav.aiState != AIState.Idle)
        {
            if (!entityNav.pathPending)
            {
                if (entityNav.remainingDistance <= entityNav.stoppingDistance)
                {
                    if(aiBehav.isFedPath)
                    {
                        aiBehav.isFedPath = false;
                    }
                    if (waitCoroutine == null)
                    {
                        waitCoroutine = StartCoroutine(Wait(true));
                    }
                    return true;
                }
            }
        }
        return false;
    }
}
