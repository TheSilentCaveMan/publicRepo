using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Rigidbody))]
public class ItemBase : MonoBehaviour, IGUIData
{
    public Rigidbody rb;
    public string _itemText = "Default";
    public string itemText
    {
        get
        {
            return _itemText;
        }
    }
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
}
