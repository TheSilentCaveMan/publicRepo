using System.Collections;
using UnityEngine;
public class GenericEventControl : ItemInteractible, IItemInteractible
{
    private bool canHappen = true;
    private Coroutine eventRoutine;
    public bool repeatable;
    public GameEvent[] gameEvents;

    public void Interact(GameObject _player)
    {
        if (eventRoutine == null && canHappen)
        {
            eventRoutine = StartCoroutine(GameEventRoutine());
        }
        if (!repeatable)
        {
            canHappen = false;
        }
    }

    private IEnumerator GameEventRoutine()
    {
        // Iterate through each game event
        for (int i = 0; i < gameEvents.Length; i++)
        {
            GameEvent currentEvent = gameEvents[i];
            if (currentEvent == null)
            {
                Debug.LogWarning($"GameEvent at index {i} is not assigned.");
                continue; // Skip to the next event if current is not defined
            }

            // Check if the event is ready to play
            if (currentEvent.isReady())
            {
                currentEvent.PlayGameEvent();
                yield return new WaitForSeconds(currentEvent.delay);
            }
            else
            {
                i--; // Retry this event if it is not ready
            }
        }

        eventRoutine = null; // Reset the coroutine reference
    }
}