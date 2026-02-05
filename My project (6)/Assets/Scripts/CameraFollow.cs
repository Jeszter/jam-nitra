using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(-5f, 2f, 5f);
    [SerializeField] private float smoothSpeed = 10f;
    
    void LateUpdate()
    {
        if (target == null) return;
        
        // Follow target position with offset, but don't inherit rotation
        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;
    }
}
