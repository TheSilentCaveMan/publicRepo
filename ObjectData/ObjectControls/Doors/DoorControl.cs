using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public enum DoorType { Manual = 0, Automatic = 1 };
[RequireComponent(typeof(NavMeshObstacle))]
public class DoorControl : ItemInteractible, IAttackable, IItemInteractible
{
    public DoorType doorType;
    public string key = "";
    internal bool locked = false;
    public bool aiOpen = false;
    private NavMeshObstacle obstacle;
    internal bool isOpening = false;
    public bool isOpen = false;
    private float speed = 1f;
    [SerializeField] private float rotationAmount = -90f;
    [SerializeField] private Vector3 startRotation;
    private Coroutine animationCoroutine;
    public GameObject destroyedObject { get; set; }
    public GameObject _destroyedObject;
    public short life { get; set; }
    public short _life;
    public Transform GetTransform()
    {
        return transform;
    }
    public bool IsAttackable { get; set; }

    private void Awake()
    {
        obstacle = GetComponent<NavMeshObstacle>();
        obstacle.carveOnlyStationary = false;
        obstacle.carving = false;
        obstacle.enabled = false;
        startRotation = transform.rotation.eulerAngles;
        life = _life;
        destroyedObject = _destroyedObject;
    }

    private void Start()
    {
        if (!string.IsNullOrEmpty(key))
        {
            locked = true;
        }
    }

    public void Interact(GameObject _player)
    {
        if (aiOpen || _player == null) return;

        if (locked)
        {
            PlayerInventory playerInventory = _player.GetComponent<PlayerInventory>();
            if (playerInventory == null || playerInventory.questInventory == null) return;

            if (CheckAccessRights(playerInventory.questInventory.questItemContainers))
            {
                locked = false;
                DoorStateChange();
            }
        }
        else
        {
            DoorStateChange();
        }
    }

    public void TakeHit()
    {
        Debug.Log("Door is taking a hit");
        EndOfLife();
    }

    public void EndOfLife()
    {
        gameObject.SetActive(false);
        if (destroyedObject != null)
        {
            Instantiate(destroyedObject, transform.position, transform.rotation);
        }
    }

    internal void DoorStateChange()
    {
        if (isOpening)
        {
            Close();
        }
        else
        {
            Open();
        }
    }

    public void Open()
    {
        if (!isOpening)
        {
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
            }
            animationCoroutine = StartCoroutine(RotateDoor(-rotationAmount, true));
        }
    }

    public void Close()
    {
        if (isOpening)
        {
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
            }
            animationCoroutine = StartCoroutine(RotateDoor(rotationAmount, false));
        }
    }

    private IEnumerator RotateDoor(float angle, bool opening)
    {
        Quaternion startQuaternion = Quaternion.Euler(startRotation);
        Quaternion targetQuaternion;

        if (opening)
        {
            targetQuaternion = Quaternion.Euler(new Vector3(startRotation.x, startRotation.y + angle, startRotation.z));
        }
        else
        {
            targetQuaternion = startQuaternion;
        }

        isOpening = opening;
        float time = 0;

        while (time < 1)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetQuaternion, time);
            time += Time.deltaTime * speed;
            yield return null;
        }
        isOpen = opening;
        obstacle.enabled = opening;
        obstacle.carving = opening;
    }

    private bool CheckAccessRights(QuestItemContainer[] input)
    {
        if (input == null) return false;

        foreach (var container in input)
        {
            if (container.item == null) continue;

            if (container.item.TryGetComponent<ItemPickupable>(out var oP) && oP.itemType == key)
            {
                return true;
            }
        }
        return false;
    }
}