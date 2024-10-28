using UnityEngine;

public class CWallCollision : MonoBehaviour
{   
    public void WallCheck()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, 0.01f);
        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("CWall") && gameObject.CompareTag("NDCWall"))
            {
                collider.gameObject.SetActive(false);
                return;
            }
        }
        colliders = Physics.OverlapSphere(transform.position, 0.01f);
        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("RFloor"))
            {
                gameObject.SetActive(false);
                return;
            }
        }
    }
}
