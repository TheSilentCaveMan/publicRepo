using UnityEngine;

public enum TargetHand
{
    LeftHand = 0,
    RightHand = 1
}

public class PlayerIKControl : MonoBehaviour
{
    [Header("IK Settings")]
    public Transform retargeter;
    public Transform handEffector;
    public TargetHand hand;

    private Animator animator;
    internal float weight;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        weight = 0f;
    }

    private void OnAnimatorIK(int layerIndex)
    {
        SetIKForHand(hand, weight, handEffector.position, handEffector.rotation);
    }

    private void SetIKForHand(TargetHand targetHand, float ikWeight, Vector3 targetPosition, Quaternion targetRotation)
    {
        AvatarIKGoal goal = (targetHand == TargetHand.LeftHand) ? AvatarIKGoal.LeftHand : AvatarIKGoal.RightHand;

        animator.SetIKPositionWeight(goal, ikWeight);
        animator.SetIKPosition(goal, targetPosition);

        animator.SetIKRotationWeight(goal, ikWeight);
        animator.SetIKRotation(goal, targetRotation);
    }
}
