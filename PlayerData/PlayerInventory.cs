using Unity.VisualScripting;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    //
    public PartsContainer[] partsInventory;
    public ItemHolder[] itemInventory = new ItemHolder[4];
    public QuestItemMenu questInventory;
    internal Transform characterHead;
    public Transform inventoryTransform;
    private GameObject flashlight;
    private bool flashlight_enable = false;
    public bool flashlight_on = false;
    public bool AddItem(PartsContainer[] collection, int itemType, int amount = 1)
    {
        try
        {
            collection[itemType].amount += amount;
            collection[itemType].Refresh();
            return true;
        }
        catch { return false; }
    }
    public bool RemoveItem(PartsContainer[] collection, int itemType, int amount = 1) 
    {
        try
        {
            if (collection[itemType].amount - amount < 1)
            {
                return false;
            }
            else
            {
                collection[itemType].amount -= amount;
                return true;
            }
        }
        catch { return false; }
    }
    internal bool GetItem(GameObject _object, int amount = 1)
    {
        _object.transform.SetParent(inventoryTransform);
        _object.transform.localPosition = Vector3.zero;
        if (_object.GetComponent<Collider>() != null)
            _object.GetComponent<Collider>().enabled = false;
        if (_object.GetComponent<Rigidbody>() != null)
            _object.GetComponent<Rigidbody>().isKinematic = true;
        try
        {
            switch (_object.tag)
            {
                case "Component_Scrap":
                    if (AddItem(partsInventory, 3, amount))
                        Debug.Log(partsInventory[3]);
                    else
                        Debug.Log("AddError");
                    break;
                case "Component_ElectricalComponent":
                    if (AddItem(partsInventory, 1, amount))
                        Debug.Log(partsInventory[1]);
                    else
                        Debug.Log("AddError");
                    break;
                case "Component_Chemical":
                    if (AddItem(partsInventory, 2, amount))
                        Debug.Log(partsInventory[2]);
                    else
                        Debug.Log("AddError");
                    break;
                case "Component_Alcohol":
                    if (AddItem(partsInventory, 0, amount))
                        Debug.Log(partsInventory[0]);
                    else
                        Debug.Log("AddError");
                    break;
                case "QuestItem":
                    GetQuestItem(_object);
                    break;
                default:
                    GetCraftedItem(_object, (short)_object.GetComponent<ItemGeneric>()._amount);
                    break;
            }
            return true;
        }
        catch { return false; }
    }
    internal void GetQuestItem(GameObject _object)
    {
       questInventory.GetQuestItem(_object);
    }
    internal void GetCraftedItem(GameObject _object, short amount = 1)
    {
        for (int i = 0; i < itemInventory.Length; i++)
        {
            if (itemInventory[i].heldItem != null)
            {
                if (itemInventory[i].heldItem.CompareTag(_object.tag))
                {
                    AddCraftedItem(i, amount);
                    Destroy(_object);
                    return;
                }
            }
        }
        for (int i = 0; i < itemInventory.Length; i++)
        {
            if (itemInventory[i].heldItem == null)
            {
                Debug.Log("Adding new item");
                itemInventory[i].heldItem = _object;
                AddCraftedItem(i, amount);
                itemInventory[i].PushItem();
                return;
            }
        }
    }
    private void AddCraftedItem(int i, short amount = 1)
    {
        itemInventory[i].amount += amount;
        itemInventory[i].amountText.text = itemInventory[i].amount.ToShortString();
    }
    public void UseFlashlight()
    {
        if (flashlight_enable)
        {
            flashlight.GetComponentInChildren<Light>().enabled = !flashlight.GetComponentInChildren<Light>().enabled;
        }
    }
}
