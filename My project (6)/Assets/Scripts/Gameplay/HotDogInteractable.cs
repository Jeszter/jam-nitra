using UnityEngine;
using Game.Interactions;

namespace Game.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class HotDogInteractable : MonoBehaviour, IInteractable
    {
        private const string DefaultEatClipResourcePath = "Audio/sound_of_eating";

        [Header("Audio")]
        [SerializeField] private AudioClip eatClip;
        [SerializeField, Range(0f, 1f)] private float volume = 1f;

        [Header("Behavior")]
        [SerializeField] private bool deactivateOnEat = true;

        private bool _eaten;

        private void Awake()
        {
            if (eatClip == null)
                eatClip = Resources.Load<AudioClip>(DefaultEatClipResourcePath);
        }

        public void Interact(GameObject interactor)
        {
            if (_eaten)
                return;

            _eaten = true;

            if (eatClip != null)
                AudioSource.PlayClipAtPoint(eatClip, transform.position, volume);

            HotDogEatManager.NotifyHotDogEaten();

            if (deactivateOnEat)
                gameObject.SetActive(false);
        }
    }
}
