using UnityEngine;

namespace Game.Interactions
{
    [DisallowMultipleComponent]
    public class ToiletFlushInteractable : MonoBehaviour, IInteractable
    {
        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip flushClip;
        [SerializeField] private float cooldownSeconds = 0.25f;

        private float _nextAllowedTime;

        private void Reset()
        {
            audioSource = GetComponent<AudioSource>();
        }

        private void Awake()
        {
            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();
        }

        public void Interact(GameObject interactor)
        {
            if (Time.time < _nextAllowedTime)
                return;

            _nextAllowedTime = Time.time + cooldownSeconds;

            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();

            if (audioSource == null)
            {
                Debug.LogWarning($"[{nameof(ToiletFlushInteractable)}] Missing AudioSource on '{name}'.", this);
                return;
            }

            var clip = flushClip != null ? flushClip : audioSource.clip;
            if (clip == null)
            {
                Debug.LogWarning($"[{nameof(ToiletFlushInteractable)}] Missing flush clip on '{name}'.", this);
                return;
            }

            audioSource.PlayOneShot(clip);
        }
    }
}
