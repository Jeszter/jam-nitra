using UnityEngine;

public class ActivateObjectTrigger : MonoBehaviour
{
    public GameObject targetFolder;

    private bool used = false;

    private void OnTriggerEnter(Collider other)
    {
        if (used) return;
        if (!other.CompareTag("Player")) return;

        if (targetFolder != null)
            targetFolder.SetActive(true);

        used = true;
        gameObject.SetActive(false);
    }
}
