using UnityEngine;
public class LightControl : ItemInteractible, IItemInteractible
{
    public LampData[] lampData;

    private bool isOn = false;
    public void Interact(GameObject _player)
    {
        Debug.Log("Interacting");
        ToggleLights();
    }

    private void ToggleLights()
    {
        isOn = !isOn;

        // Toggle all lamps in the lampData array
        foreach (LampData lamp in lampData)
        {
            lamp.isOn = isOn;
            lamp.Switch();
        }

        // Play the audio if available
        UpdateAudio();
        // Execute animation logic
        UpdateAnimation();
    }
    private void UpdateAudio()
    {
        if (audioPackage.Length > 0 && isOn)
        {
            PlayAudio(audioPackage[0]);
        }
        if (audioPackage.Length > 1 && !isOn)
        {
            PlayAudio(audioPackage[1]);
        }
    }
    private void UpdateAnimation()
    {
        if (animationPackage.Length > 0)
        {
            if (isOn)
            {
                PlayAnimation(animationPackage[0]);
            }
            else
            {
                PlayAnimation(animationPackage[1]);
            }
        }
    }
}