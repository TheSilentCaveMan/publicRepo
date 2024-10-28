using System.Collections;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    private Camera playerCamera;
    public PlayerMovement playerMovement;
    public PlayerBehaviour playerBehav;
    private Vector3 ogPosition;
    private Coroutine cameraShake;
    public float shakeFrequencyChangeRate = 0.5f;
    private void Awake()
    {
        playerCamera = GetComponent<Camera>();
        ogPosition = transform.localPosition;
    }
    private void Start()
    {
        cameraShake = StartCoroutine(CameraShake());
    }
    private IEnumerator CameraShake()
    {
        while (playerBehav.IsAttackable)
        {
            // Determine speed-based shake values only when player is moving horizontally
            if (!playerMovement.jump)  // Assuming a boolean that checks if horizontal motion is happening
            {
                // Lerp shake intensity based on horizontal speed proportion
                // Check if speed values are valid to avoid NaN
                float movementRatio = (playerMovement.sprintSpeed > 0.0f) ? playerMovement.speed / playerMovement.sprintSpeed : 0.0f;
                float intensity = Mathf.Lerp(0, movementRatio / 2, movementRatio);
                float frequency = Mathf.Lerp(1f, Mathf.Round(playerMovement.speed / 2), shakeFrequencyChangeRate);

                Vector3 cameraOffset = new Vector3(
                    OffsetX(frequency, intensity, 2.0f),
                    OffsetY(frequency, intensity)
                );
                transform.localPosition = ogPosition + cameraOffset;
            }
            else
            {
                transform.localPosition = ogPosition; // Reset position when not moving
            }
            yield return null;
        }
    }

    private float OffsetX(float frequency, float intensity)
    {
        float offsetX = (Mathf.PerlinNoise(Time.time * frequency, 0) - 0.5f) * intensity;
        return offsetX;
    }
    private float OffsetX(float frequency, float intensity, float bias)
    {
        float offsetX = (Mathf.PerlinNoise(Time.time * frequency, 0) - 0.5f) * intensity;
        return offsetX / bias;
    }
    private float OffsetY(float frequency, float intensity)
    {
        float offsetY = (Mathf.PerlinNoise(0, Time.time * frequency) - 0.5f) * intensity;
        return offsetY;
    }
    private float OffsetY(float frequency, float intensity, float bias)
    {
        float offsetY = (Mathf.PerlinNoise(0, Time.time * frequency) - 0.5f) * intensity;
        return offsetY / bias;
    }
}