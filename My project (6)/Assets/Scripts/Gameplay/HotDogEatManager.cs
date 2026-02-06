using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class HotDogEatManager : MonoBehaviour
    {
        [SerializeField] private int targetHotDogCount = 5;
        [SerializeField] private string nextSceneName = "FlappyBird";

        private static HotDogEatManager _instance;
        private int _eaten;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
        }

        private void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }

        public static void NotifyHotDogEaten()
        {
            if (_instance == null)
                _instance = FindAnyObjectByType<HotDogEatManager>();

            if (_instance == null)
            {
                Debug.LogWarning($"[{nameof(HotDogEatManager)}] No manager found in scene. Add one to enable counting and scene switching.");
                return;
            }

            _instance.OnHotDogEaten();
        }

        private void OnHotDogEaten()
        {
            _eaten++;

            if (_eaten < targetHotDogCount)
                return;

            if (string.IsNullOrWhiteSpace(nextSceneName))
                return;

            Game.UI.WhiteFadeSceneTransition.LoadSceneWithFade(nextSceneName);
        }
    }
}
