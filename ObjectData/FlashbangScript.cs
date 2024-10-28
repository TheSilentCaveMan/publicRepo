using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;

public class FlashbangScript : UsableItem
{
    public Camera playerCam;
    public RawImage playerEffect;
    public LayerMask layerMask;
    private GameObject[] players;
    private GameObject[] entities;
    private PlayerGUI playerGUI;
    private Plane[] cameraFrustum;
    private Bounds bounds;
    private RenderTexture screenShot;
    private float flashDistance = 1;
    private const float FLASHDISTANCEMAX = 20f;
    private const float AISTUNDURATION = 5f;
    private const float FLASHEFFICIENCYDECREASE = 0.002f;
    private const float MINFLASHDISTANCE = 3f;
    private void Awake()
    {
        entities = GameObject.FindGameObjectsWithTag("Entity");
        players = GameObject.FindGameObjectsWithTag("Player");
    }
    private void RenderPipeLineManagerEndCameraRendering(ScriptableRenderContext context, Camera cam)
    {
        if (screenShot == null || screenShot.width != Screen.width || screenShot.height != Screen.height)
        {
            if (screenShot != null) screenShot.Release();
            screenShot = new RenderTexture(Screen.width, Screen.height, 32);
        }
        ScreenCapture.CaptureScreenshotIntoRenderTexture(screenShot);
    }

    public override void Use()
    {
        FindActivePlayerCamera();
        FindEntities();
        if (playerCam == null) return;
        cameraFrustum = GeometryUtility.CalculateFrustumPlanes(playerCam);
        StartCoroutine(GetVisibility());
    }
    private void FindActivePlayerCamera()
    {
        foreach (GameObject player in players)
        {
            GameObject playerCamera = player.GetComponent<PlayerBehaviour>().playerCamera.gameObject;
            if (!ShootRay(playerCamera, MINFLASHDISTANCE / 2)) continue;
            Camera camera = player.GetComponentInChildren<Camera>();
            if (camera != null && camera.gameObject.activeSelf)
            {
                playerCam = camera;
                break;
            }
        }
    }
    private void FindEntities()
    {
        foreach (GameObject entity in entities)
        {
            if (!ShootRay(entity, MINFLASHDISTANCE * 2)) continue;
            AIBehaviour aiBehav = entity.GetComponent<AIBehaviour>();
            StartCoroutine(AIEffect(aiBehav));
            aiBehav.aiState = AIState.Stunned;
        }
    }
    private IEnumerator AIEffect(AIBehaviour aiBehav)
    {
        aiBehav.aiState = AIState.Stunned;
        yield return new WaitForSeconds(AISTUNDURATION * (1-flashDistance));
        aiBehav.aiState = AIState.Roam;
    }
    private bool ShootRay(GameObject target, float minDistance)
    {
        Vector3 direction = target.transform.position - gameObject.transform.position;
        Ray ray = new Ray(gameObject.transform.position, direction);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, FLASHDISTANCEMAX, layerMask))
            if (hit.collider.gameObject.layer == 8 || hit.collider.gameObject.layer == 11)
                return false;
        flashDistance = hit.distance / FLASHDISTANCEMAX;
        return true;
    }
    private IEnumerator GetVisibility()
    {
        yield return null;

        Collider collider = GetComponent<Collider>();
        if (collider == null) yield break;

        bounds = collider.bounds;
        if (!GeometryUtility.TestPlanesAABB(cameraFrustum, bounds)) yield break;

        playerGUI = playerCam.gameObject.GetComponentInParent<PlayerActions>().playerGUI;
        if (playerGUI == null) yield break;
        StartCoroutine(FlashEffect(playerGUI.FullScreenEffects));
    }

    private IEnumerator FlashEffect(RawImage image)
    {
        if (playerGUI == null) yield break;

        playerGUI.gameObject.SetActive(false);
        RenderPipelineManager.endCameraRendering += RenderPipeLineManagerEndCameraRendering;

        yield return new WaitForEndOfFrame();

        RenderPipelineManager.endCameraRendering -= RenderPipeLineManagerEndCameraRendering;
        playerGUI.gameObject.SetActive(true);

        image.texture = screenShot;

        // Flip the image vertically by adjusting the UV rect
        image.uvRect = new Rect(0, 1, 1, -1);

        StartCoroutine(ApplyFlashEffect(image));
    }

    private IEnumerator ApplyFlashEffect(RawImage image)
    {
        // Initialize color with the calculated alpha value based on distance
        Color tempColor = image.color;
        tempColor.a = Mathf.Max(0f, 1f - flashDistance);

        Image frontFlash = playerGUI.FullScreenEffects.GetComponentInChildren<Image>();
        Color frontTempColor = Color.white;
        frontTempColor.a = Mathf.Max(0f, 1f - flashDistance);
        while (tempColor.a > 0 || (frontFlash != null && frontTempColor.a > 0))
        {
            // Set colors to the images
            image.color = tempColor;
            if (frontFlash != null)
            {
                frontFlash.color = frontTempColor;
                frontTempColor.a -= FLASHEFFICIENCYDECREASE;
                frontTempColor.a = Mathf.Max(0f, frontTempColor.a); // Ensure alpha doesn't go negative
            }

            // Decrease tempColor's alpha, ensuring it doesn't go negative
            tempColor.a -= FLASHEFFICIENCYDECREASE / 2;
            tempColor.a = Mathf.Max(0f, tempColor.a); // Clamping to zero to prevent negative values

            if (tempColor.a == 0 && (frontFlash == null || frontTempColor.a == 0))
            {
                break; // Exit if both are fully transparent
            }

            yield return new WaitForEndOfFrame();
        }
        // Set color once explicitly at the end to ensure it’s clear
        tempColor.a = 0;
        image.color = tempColor;
        if (frontFlash != null)
        {
            frontTempColor.a = 0;
            frontFlash.color = frontTempColor;
        }

        image.texture = null;  // Clear texture to ensure visual resetting
        inEffect = false;
        canBeDestroyed = true; // Mark object as destroyable
    }
}