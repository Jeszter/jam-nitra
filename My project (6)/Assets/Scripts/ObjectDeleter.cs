using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Allows the player to delete objects by pressing E while looking at them.
/// Attach this script to the player or camera.
/// </summary>
public class ObjectDeleter : MonoBehaviour
{
    [Header("Raycast Settings")]
    [Tooltip("Maximum distance to detect objects for deletion.")]
    [SerializeField] private float interactionRange = 5f;
    
    [Tooltip("Camera used for raycasting. If not set, will use Camera.main.")]
    [SerializeField] private Camera playerCamera;
    
    [Tooltip("Layers that can be deleted. Set to Everything by default.")]
    [SerializeField] private LayerMask deletableLayers = ~0;
    
    [Header("Optional Settings")]
    [Tooltip("If set, only objects with this tag can be deleted. Leave empty to delete any object.")]
    [SerializeField] private string requiredTag = "";
    
    [Tooltip("If true, shows debug ray in Scene view.")]
    [SerializeField] private bool showDebugRay = true;

    private void Start()
    {
        // If no camera assigned, try to find one
        if (playerCamera == null)
        {
            playerCamera = GetComponentInChildren<Camera>();
            if (playerCamera == null)
            {
                playerCamera = Camera.main;
            }
        }
        
        if (playerCamera == null)
        {
            Debug.LogWarning("ObjectDeleter: No camera found. Please assign a camera or ensure Camera.main exists.");
        }
    }

    private void Update()
    {
        if (playerCamera == null) return;
        
        // Check for E key press
        if (ReadInteractPressedThisFrame())
        {
            TryDeleteObject();
        }
        
        // Debug visualization
        if (showDebugRay)
        {
            Debug.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * interactionRange, Color.yellow);
        }
    }

    private void TryDeleteObject()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        
        if (Physics.Raycast(ray, out RaycastHit hit, interactionRange, deletableLayers, QueryTriggerInteraction.Ignore))
        {
            GameObject targetObject = hit.collider.gameObject;
            
            // Check if the object has the Deletable component
            Deletable deletable = targetObject.GetComponent<Deletable>();
            if (deletable == null)
            {
                Debug.Log($"ObjectDeleter: Object '{targetObject.name}' does not have a Deletable component.");
                return;
            }
            
            // Check tag requirement if specified
            if (!string.IsNullOrEmpty(requiredTag))
            {
                if (!targetObject.CompareTag(requiredTag))
                {
                    Debug.Log($"ObjectDeleter: Object '{targetObject.name}' does not have required tag '{requiredTag}'.");
                    return;
                }
            }
            
            // Call the OnBeforeDelete callback
            deletable.OnBeforeDelete();
            
            Debug.Log($"ObjectDeleter: Deleted '{targetObject.name}'");
            Destroy(targetObject);
        }
        else
        {
            Debug.Log("ObjectDeleter: No object in range to delete.");
        }
    }

    /// <summary>
    /// Checks if the interact key (E) was pressed this frame.
    /// </summary>
    private static bool ReadInteractPressedThisFrame()
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard kb = Keyboard.current;
        if (kb == null) return false;
        return kb.eKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.E);
#endif
    }
}
