using UnityEngine;
public class ItemInteractible : ItemBase
{
    public AudioSource[] audioPackage;
    public Animation[] animationPackage;
    internal void PlayAudio(AudioSource source) 
    {
        source.Play();
    }
    internal void StopAudio(AudioSource source)
    {
        source.Stop();
    }
    internal void PlayAnimation(Animation source) 
    {
        source.Play();
    }
    internal void StopAnimation(Animation source)
    {
        source.Stop();
    }
}
