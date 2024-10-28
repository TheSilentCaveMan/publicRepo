using UnityEngine;

public class GameEvent
{
    public Animation animation;
    public AudioSource audioSource;
    public float delay;

    /// <summary>
    /// Plays the event, triggering the animation and audio if they are available.
    /// </summary>
    internal void PlayGameEvent()
    {
        if (animation != null)
        {
            animation.Play();
        }

        if (audioSource != null)
        {
            audioSource.Play();
        }
    }

    /// <summary>
    /// Checks whether the game event is ready to be played.
    /// An event is ready if the animation is not currently playing
    /// and the audio source is also not playing.
    /// </summary>
    internal bool isReady()
    {
        bool animationReady = animation == null || !animation.isPlaying;
        bool audioReady = audioSource == null || !audioSource.isPlaying;

        return animationReady && audioReady;
    }
}