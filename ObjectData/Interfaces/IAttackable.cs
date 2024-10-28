using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAttackable
{
    public short life { get; set; }
    public bool IsAttackable { get; set; }
    public GameObject destroyedObject { get; set; }
    public void TakeHit();
    public Transform GetTransform();
    public void EndOfLife();
}
