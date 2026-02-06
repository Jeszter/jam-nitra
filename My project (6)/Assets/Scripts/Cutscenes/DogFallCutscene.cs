using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace JamNitra.Cutscenes
{
    public sealed class DogFallCutscene : MonoBehaviour
    {
        [Header("Scene refs")]
        [SerializeField] private Camera mainCamera;
        [SerializeField] private Camera cutsceneCamera;
        [SerializeField] private Transform windowTransform;
        [SerializeField] private Transform dogVisual;
        [SerializeField] private Image fadeImage;

        [Header("Timing")]
        [SerializeField, Min(0.0f)] private float preDelay = 0.1f;
        [SerializeField, Min(0.1f)] private float runDuration = 0.8f;
        [SerializeField, Min(0.1f)] private float jumpDuration = 0.35f;
        [SerializeField, Min(0.0f)] private float fadeOutDuration = 0.4f;
        [SerializeField, Min(0.0f)] private float blackHoldDuration = 0.1f;
        [SerializeField, Min(0.0f)] private float fadeInDuration = 0.2f;

        [Header("Staging")]
        [Tooltip("Offset from the window (in window local space) for the moment before jumping.")]
        [SerializeField] private Vector3 dogOffsetFromWindow = new(0.0f, 0.1f, -0.6f);

        [Tooltip("How far inside the apartment the dog starts (meters, along -window.forward).")]
        [SerializeField, Min(0.5f)] private float runStartBackDistance = 3.5f;

        [Tooltip("Side offset inside room (meters, along window.right).")]
        [SerializeField] private float runStartSideOffset = 0.6f;

        [Tooltip("Height offset for dog (meters, world up).")]
        [SerializeField] private float dogHeightOffset = 0.05f;

        [Tooltip("How far outside the window the dog gets by end of jump (meters, along window.forward).")]
        [SerializeField, Min(0.1f)] private float jumpForwardDistance = 1.2f;

        [Tooltip("Jump arc height (meters, world up).")]
        [SerializeField, Min(0.0f)] private float jumpArcHeight = 0.8f;

        [Tooltip("Camera position relative to window (in window local space). This should be inside the apartment.")]
        [SerializeField] private Vector3 cameraOffsetFromWindow = new(-2.4f, 1.4f, -2.2f);

        [SerializeField] private Vector3 cameraLookOffset = new(0.0f, 0.4f, 0.0f);

        [Header("Auto play")]
        [SerializeField] private bool playOnStart = true;

        [Header("After cutscene")]
        [SerializeField] private bool loadSceneOnFinish;
        [SerializeField] private string sceneToLoad = "MainMenu";

        private bool _played;
        private bool _wasMainCameraEnabled;

        private void Reset()
        {
            TryAutoAssignReferences();
        }

        private void Awake()
        {
            TryAutoAssignReferences();
            EnsureFadeInitialized();
        }

        private void Start()
        {
            if (playOnStart)
                PlayOnce();
        }

        public void PlayOnce()
        {
            if (_played)
                return;

            _played = true;
            StartCoroutine(PlayRoutine());
        }

        private IEnumerator PlayRoutine()
        {
            if (mainCamera == null)
                mainCamera = Camera.main;

            if (cutsceneCamera == null || windowTransform == null || dogVisual == null || fadeImage == null)
            {
                Debug.LogWarning("DogFallCutscene: missing references, cutscene will not play.");
                yield break;
            }

            _wasMainCameraEnabled = mainCamera != null && mainCamera.enabled;

            if (mainCamera != null)
                mainCamera.enabled = false;

            cutsceneCamera.enabled = true;

            // Determine which direction is outside so we can guarantee an interior view.
            Vector3 windowPos = windowTransform.position;
            Vector3 right = windowTransform.right;
            Vector3 up = Vector3.up;

            Vector3 forward = windowTransform.forward;
            Vector3 outward = ResolveOutwardDirection(windowPos, forward, up);
            Vector3 inward = -outward;

            // Camera: inside apartment, looking at the window area.
            Vector3 cameraPos = windowPos + inward * Mathf.Abs(cameraOffsetFromWindow.z)
                              + right * cameraOffsetFromWindow.x
                              + up * cameraOffsetFromWindow.y;
            cutsceneCamera.transform.position = cameraPos;

            // Dog path (start deeper inside, run to window, jump out).
            Vector3 runStart = windowPos + inward * runStartBackDistance + right * runStartSideOffset + up * dogHeightOffset;
            Vector3 preJump = windowPos + inward * 0.6f + up * dogHeightOffset;
            Vector3 jumpEnd = windowPos + outward * jumpForwardDistance + up * dogHeightOffset;

            dogVisual.position = runStart;
            dogVisual.rotation = Quaternion.LookRotation((preJump - runStart).normalized, Vector3.up);

            cutsceneCamera.transform.LookAt(preJump + cameraLookOffset);

            yield return new WaitForSeconds(preDelay);

            // Run to the window.
            for (float t = 0f; t < runDuration; t += Time.deltaTime)
            {
                float u = Mathf.Clamp01(t / runDuration);
                dogVisual.position = Vector3.Lerp(runStart, preJump, SmoothStep(u));

                Vector3 dir = preJump - dogVisual.position;
                if (dir.sqrMagnitude > 0.0001f)
                    dogVisual.rotation = Quaternion.Slerp(dogVisual.rotation, Quaternion.LookRotation(dir.normalized, Vector3.up), 12f * Time.deltaTime);

                cutsceneCamera.transform.LookAt(preJump + cameraLookOffset);
                yield return null;
            }

            dogVisual.position = preJump;
            dogVisual.rotation = Quaternion.LookRotation(outward, Vector3.up);

            // Jump through the window (parabolic arc).
            for (float t = 0f; t < jumpDuration; t += Time.deltaTime)
            {
                float u = Mathf.Clamp01(t / jumpDuration);
                Vector3 p = Vector3.Lerp(preJump, jumpEnd, u);
                p += up * (Mathf.Sin(u * Mathf.PI) * jumpArcHeight);
                dogVisual.position = p;
                dogVisual.rotation = Quaternion.LookRotation(outward, Vector3.up);

                // Keep camera on the dog as it exits.
                cutsceneCamera.transform.LookAt(dogVisual.position + cameraLookOffset);

                // Start fading near the end of the jump (so we don't show impact).
                float fadeU = Mathf.InverseLerp(0.55f, 1.0f, u);
                if (fadeOutDuration > 0f)
                    SetFadeAlpha(Mathf.Clamp01(fadeU));

                yield return null;
            }

            SetFadeAlpha(1f);
            yield return new WaitForSeconds(blackHoldDuration);

            // End cutscene.
            cutsceneCamera.enabled = false;

            if (mainCamera != null)
                mainCamera.enabled = _wasMainCameraEnabled;

            // Fade back in on gameplay camera.
            if (fadeInDuration > 0f)
            {
                float ft = 0f;
                while (ft < fadeInDuration)
                {
                    ft += Time.deltaTime;
                    SetFadeAlpha(1f - Mathf.Clamp01(ft / fadeInDuration));
                    yield return null;
                }
            }

            SetFadeAlpha(0f);

            if (loadSceneOnFinish && !string.IsNullOrWhiteSpace(sceneToLoad))
                SceneManager.LoadScene(sceneToLoad);
        }

        private static Vector3 ResolveOutwardDirection(Vector3 windowPos, Vector3 forward, Vector3 up)
        {
            // If casting slightly away from the window immediately hits geometry, we're probably casting into the interior.
            // In that case outward is the opposite direction.
            Vector3 origin = windowPos + up * 1.0f;

            if (Physics.Raycast(origin, forward, out _, 0.75f, ~0, QueryTriggerInteraction.Ignore))
                return -forward;

            return forward;
        }

        private void TryAutoAssignReferences()
        {
            if (mainCamera == null)
                mainCamera = Camera.main;

            if (cutsceneCamera == null)
            {
                Transform t = transform.Find("CutsceneCamera");
                if (t != null)
                    cutsceneCamera = t.GetComponent<Camera>();
            }

            if (dogVisual == null)
            {
                Transform t = transform.Find("DogVisual");
                if (t != null)
                    dogVisual = t;
            }

            if (windowTransform == null)
            {
                GameObject go = GameObject.Find("FOB_LOD/Windows/Window_1");
                if (go != null)
                    windowTransform = go.transform;
            }

            if (fadeImage == null)
            {
                GameObject go = GameObject.Find("Canvas/CutsceneFade");
                if (go != null)
                    fadeImage = go.GetComponent<Image>();
            }
        }

        private void EnsureFadeInitialized()
        {
            if (fadeImage == null)
                return;

            fadeImage.color = new Color(0f, 0f, 0f, 0f);
        }

        private void SetFadeAlpha(float a)
        {
            if (fadeImage == null)
                return;

            Color c = fadeImage.color;
            c.a = a;
            fadeImage.color = c;
        }

        private static float SmoothStep(float t)
        {
            // cubic smoothstep
            return t * t * (3f - 2f * t);
        }
    }
}
