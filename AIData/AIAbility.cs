using UnityEngine;
public class AIAbility : MonoBehaviour
{
    internal float abiltyDelay;
    internal AIState[] excludeBehaviours;
    internal Coroutine AbilityCoroutine;
    internal AIBehaviour aiBehav;
    private bool canUse = true;
    internal virtual bool CallAbility() 
    {
        canUse = true;
        for (int i = 0; i < excludeBehaviours.Length; i++)
        {
            if (excludeBehaviours[i] == aiBehav.aiState)
            {
                canUse = false;
                break;
            }
        }
        if (canUse)
        {
            if (AbilityCoroutine == null)
            {
                return true;
            }
        }
        return false;
    }
}
