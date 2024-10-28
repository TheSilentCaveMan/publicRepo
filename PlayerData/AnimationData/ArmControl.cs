using UnityEngine;
using UnityEngine.Animations.Rigging;

public class ArmControl : MonoBehaviour
{
    [Header("Transforms")]
    public Transform inventoryTransform;

    [Header("Hand Constraints")]
    public ChainIKConstraint[] handConstraints;

    private Animator animator;
    private PlayerIKControl ikControl;
    private RigBuilder rigBuilder;
    private Transform rightHand;
    private GameObject currentItem;

    private void Awake()
    {
        InitializeComponents();
        rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
        animator.GetBoneTransform(HumanBodyBones.RightShoulder); // shoulder not used so remove variable
    }

    private void Start()
    {
        InitHandConstraints();
        SetWeight(false);
    }

    private void InitializeComponents()
    {
        animator = transform.parent.GetComponent<Animator>();
        ikControl = transform.parent.GetComponent<PlayerIKControl>();
        rigBuilder = transform.parent.GetComponent<RigBuilder>();
    }

    private void Update()
    {
        // Potentially redundant as it sets the position to its current value. Remove if not needed.
    }

    private void InitHandConstraints()
    {
        Transform rightHandTransform = animator.GetBoneTransform(HumanBodyBones.RightHand);

        HumanBodyBones[] fingerBones = {
            HumanBodyBones.RightThumbDistal,
            HumanBodyBones.RightIndexDistal,
            HumanBodyBones.RightMiddleDistal,
            HumanBodyBones.RightRingDistal,
            HumanBodyBones.RightLittleDistal
        };

        for (int i = 0; i < handConstraints.Length; i++)
        {
            handConstraints[i].data.root = rightHandTransform;
            handConstraints[i].data.tip = animator.GetBoneTransform(fingerBones[i]);
        }
    }

    private void SetWeight(bool isHolding)
    {
        ikControl.weight = isHolding ? 1f : 0f;

        for (int i = 0; i < handConstraints.Length; i++)
        {
            handConstraints[i].weight = isHolding ? 1f : 0f;
        }
    }

    internal void HandControl(GameObject inItem)
    {
        if (inItem != null)
        {
            SetWeight(true);
            animator.SetBool("holdingItem", true);
            UpdateCurrentItem(inItem);
        }
        else
        {
            SetWeight(false);
            animator.SetBool("holdingItem", false);
            UpdateCurrentItem(null);
        }

        if (inItem != null)
        {
            SetFingerTargets(inItem.GetComponent<UsableItem>());
            rigBuilder.Build();
        }
    }

    private void SetFingerTargets(UsableItem itemData)
    {
        for (int i = 0; i < handConstraints.Length; i++)
        {
            handConstraints[i].data.target = itemData.fingerPositions[i].transform;
        }
    }

    private void UpdateCurrentItem(GameObject inItem)
    {
        if (currentItem != null)
        {
            if (inItem == null || !currentItem.CompareTag(inItem.tag))
            {
                currentItem.transform.SetParent(inventoryTransform);
                currentItem.SetActive(false);
                currentItem = null;
            }
        }

        if (inItem != null)
        {
            currentItem = inItem;
            currentItem.transform.SetParent(rightHand);
            currentItem.SetActive(true);
            currentItem.transform.localPosition = Vector3.zero;
            currentItem.transform.rotation = Quaternion.Euler(transform.rotation.x, transform.rotation.y + 90f, transform.rotation.z);
        }
    }
}