using UnityEngine;

namespace Game.Interactions
{
    /// <summary>
    /// Minimal interaction contract used by Player interactor components.
    /// </summary>
    public interface IInteractable
    {
        void Interact(GameObject interactor);
    }
}
