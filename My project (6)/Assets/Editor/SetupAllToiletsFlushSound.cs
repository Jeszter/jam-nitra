#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Game.Interactions;

public static class SetupAllToiletsFlushSound
{
    [MenuItem("Tools/Setup/Assign Flush Sound To ALL Toilet_1 In Scene")]
    public static void Execute()
    {
        const string clipPath = "Assets/Music/flush.mp3";

        var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(clipPath);
        if (clip == null)
        {
            Debug.LogError($"AudioClip not found at '{clipPath}'.");
            return;
        }

        var toilets = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None)
            .Where(go => go != null && go.name == "Toilet_1")
            .ToArray();

        if (toilets.Length == 0)
        {
            Debug.LogWarning("No GameObjects named 'Toilet_1' found in the opened scene.");
            return;
        }

        foreach (var toilet in toilets)
        {
            var collider = toilet.GetComponent<Collider>();
            if (collider == null)
            {
                var box = toilet.AddComponent<BoxCollider>();
                box.isTrigger = false;
            }

            var audioSource = toilet.GetComponent<AudioSource>();
            if (audioSource == null) audioSource = toilet.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f;
            audioSource.clip = clip;

            var interactable = toilet.GetComponent<ToiletFlushInteractable>();
            if (interactable == null) interactable = toilet.AddComponent<ToiletFlushInteractable>();

            // Assign serialized fields (private) via reflection.
            var t = typeof(ToiletFlushInteractable);
            t.GetField("audioSource", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                ?.SetValue(interactable, audioSource);
            t.GetField("flushClip", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                ?.SetValue(interactable, clip);

            EditorUtility.SetDirty(toilet);
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log($"Assigned flush sound to {toilets.Length} toilets in scene '{EditorSceneManager.GetActiveScene().name}'.");
    }
}
#endif
