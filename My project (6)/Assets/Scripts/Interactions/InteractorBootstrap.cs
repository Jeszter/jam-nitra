using UnityEngine;
using UnityEngine.SceneManagement;
using Game.Doors;

namespace Game.Interactions
{
    /// <summary>
    /// Ensures there is always an interactor in the scene so pressing E works
    /// even when starting from scenes that don't have a Player object.
    /// </summary>
    public sealed class InteractorBootstrap : MonoBehaviour
    {
        private static bool _hooked;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            if (_hooked)
                return;

            _hooked = true;
            SceneManager.sceneLoaded += (_, __) => EnsureInteractorExists();
            EnsureInteractorExists();
        }

        private static void EnsureInteractorExists()
        {
            // If the scene already provides an interactor (e.g., on Player), do nothing.
            if (Object.FindAnyObjectByType<PlayerDoorInteractor>() != null)
                return;

            var cam = Camera.main;
            if (cam == null)
                return;

            // Add interactor to the main camera for menu/intro scenes.
            cam.gameObject.AddComponent<PlayerDoorInteractor>();
        }
    }
}
