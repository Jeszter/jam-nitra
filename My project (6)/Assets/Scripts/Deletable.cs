using UnityEngine;

/// <summary>
/// Attach this component to any object that should be deletable by the player.
/// Objects without this component cannot be deleted by the ObjectDeleter.
/// </summary>
public class Deletable : MonoBehaviour
{
    [Tooltip("Optional message to display when this object is deleted.")]
    [SerializeField] private string deleteMessage = "";
    
    [Tooltip("If true, plays a sound or effect when deleted (extend as needed).")]
    [SerializeField] private bool showDeleteFeedback = true;

    /// <summary>
    /// Called just before the object is deleted. Override or extend for custom behavior.
    /// </summary>
    public virtual void OnBeforeDelete()
    {
        if (showDeleteFeedback && !string.IsNullOrEmpty(deleteMessage))
        {
            Debug.Log(deleteMessage);
        }
    }
}
