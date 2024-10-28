using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Destroyable : MonoBehaviour
{
    [SerializeField] private float mass = 20f;
    [SerializeField] private float fadeSpeed = 0.1f;
    public GameObject preDefinedObject;

    private Coroutine destroyRoutine;

    private void Start()
    {
        if (preDefinedObject == null)
        {
            StartDestruction();
        }
        else
        {
            StartDestruction(preDefinedObject);
        }
    }

    private void AddRigidBody(GameObject piece)
    {
        if (piece.GetComponent<Rigidbody>() == null)
        {
            Rigidbody body = piece.AddComponent<Rigidbody>();
            body.useGravity = true;
            body.mass = mass / transform.childCount;
        }
    }

    private void AddCollider(GameObject piece)
    {
        if (piece.GetComponent<MeshCollider>() == null)
        {
            MeshCollider collider = piece.AddComponent<MeshCollider>();
            collider.convex = true;
        }
    }

    private void StartDestruction()
    {
        foreach (Transform piece in transform)
        {
            AddRigidBody(piece.gameObject);
            AddCollider(piece.gameObject);
            StartCoroutine(FadeAndDisable(piece));
        }
        if (destroyRoutine == null)
        {
            destroyRoutine = StartCoroutine(CheckIfAllPiecesDestroyed());
        }
    }

    private void StartDestruction(GameObject item)
    {
        StartCoroutine(FadeAndDisable(item.transform));
    }

    private IEnumerator CheckIfAllPiecesDestroyed()
    {
        bool allDestroyed;
        do
        {
            allDestroyed = true;
            foreach (Transform piece in transform)
            {
                if (piece.gameObject.activeSelf)
                {
                    allDestroyed = false;
                    break;
                }
            }
            yield return new WaitForSeconds(1f);
        } while (!allDestroyed);

        gameObject.SetActive(false);
    }

    private IEnumerator FadeAndDisable(Transform piece)
    {
        Renderer renderer = piece.GetComponent<Renderer>();
        if (renderer == null)
        {
            yield break;
        }

        Material material = renderer.material;
        material.SetColor("_EmissiveColor", material.color * 0);
        Color color = material.color;

        while (color.a > 0.1f)
        {
            color.a -= fadeSpeed * Time.deltaTime;
            material.color = color;
            material.SetColor("_Color", new Color(material.color.r, material.color.g, material.color.b, color.a));
            yield return null;
        }

        piece.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        if (gameObject != null)
        {
            Destroy(gameObject);
        }
    }
}