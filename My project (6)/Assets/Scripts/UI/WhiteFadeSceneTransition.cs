using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// Simple fade-to-white scene transition.
    /// Creates an overlay canvas at runtime and persists across scenes.
    /// </summary>
    public sealed class WhiteFadeSceneTransition : MonoBehaviour
    {
        [SerializeField] private float fadeDurationSeconds = 0.75f;

        private static WhiteFadeSceneTransition _instance;

        private Canvas _canvas;
        private Image _image;
        private Coroutine _transitionRoutine;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureExists()
        {
            if (_instance != null)
                return;

            var go = new GameObject("WhiteFadeSceneTransition");
            _instance = go.AddComponent<WhiteFadeSceneTransition>();
            DontDestroyOnLoad(go);
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            BuildUI();
        }

        private void BuildUI()
        {
            _canvas = gameObject.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = short.MaxValue;

            gameObject.AddComponent<CanvasScaler>();
            gameObject.AddComponent<GraphicRaycaster>();

            var imgGo = new GameObject("FadeImage");
            imgGo.transform.SetParent(transform, false);

            _image = imgGo.AddComponent<Image>();
            _image.color = new Color(1f, 1f, 1f, 0f);
            _image.raycastTarget = false;

            var rt = _image.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        public static void LoadSceneWithFade(string sceneName, float? durationSeconds = null)
        {
            EnsureExists();

            if (_instance == null)
            {
                SceneManager.LoadScene(sceneName);
                return;
            }

            _instance.StartTransition(sceneName, durationSeconds);
        }

        private void StartTransition(string sceneName, float? durationSeconds)
        {
            if (_transitionRoutine != null)
                StopCoroutine(_transitionRoutine);

            _transitionRoutine = StartCoroutine(TransitionRoutine(sceneName, durationSeconds ?? fadeDurationSeconds));
        }

        private IEnumerator TransitionRoutine(string sceneName, float duration)
        {
            duration = Mathf.Max(0.01f, duration);

            // Fade to white
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                SetAlpha(Mathf.Clamp01(t / duration));
                yield return null;
            }
            SetAlpha(1f);

            // Load
            SceneManager.LoadScene(sceneName);

            // Wait one frame for new scene render
            yield return null;

            // Fade out
            t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                SetAlpha(1f - Mathf.Clamp01(t / duration));
                yield return null;
            }
            SetAlpha(0f);

            _transitionRoutine = null;
        }

        private void SetAlpha(float a)
        {
            if (_image == null)
                return;

            var c = _image.color;
            c.a = a;
            _image.color = c;
        }
    }
}
