using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestEntitySense : AISense
{
    internal override void CallSense(GameObject[] players)
    {
        base.CallSense(players);
    }
    internal override bool Smell(PlayerBehaviour playerData, float distance)
    {
        return base.Smell(playerData, distance);
    }
    internal override bool Hear()
    {
        return base.Hear();
    }
    internal override bool Look(GameObject player)
    {
        return base.Look(player);
    }
    internal override bool CanChase(GameObject _target)
    {
        return base.CanChase(_target);
    }
}
