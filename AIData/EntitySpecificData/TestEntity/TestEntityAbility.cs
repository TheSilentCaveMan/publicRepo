using System.Collections;
using UnityEngine;

public class TestEntityAbility : AIAbility
{
    internal override bool CallAbility()
    {
        if (base.CallAbility())
        {
            AbilityCoroutine = StartCoroutine(Ability());
            return true;
        }
        return false;
    }
    private IEnumerator Ability() 
    {
        WaitForSeconds delay = new WaitForSeconds(abiltyDelay);
        yield return delay;
        Debug.Log("Using Abilty");
        StopCoroutine(AbilityCoroutine);
        AbilityCoroutine = null;
    }
}
