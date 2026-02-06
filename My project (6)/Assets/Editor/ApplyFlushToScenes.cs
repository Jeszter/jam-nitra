#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Game.Interactions;

public static class ApplyFlushToScenes
{
    private const string ClipPath = "Assets/Music/flush.mp3";

    [MenuItem("Tools/Setup/Apply Flush Sound To MainMenu + GameScene (Save In Place)")]
    public static void Execute()
    {
        ApplyToScene("Assets/Scenes/MainMenu.unity", onlyFirst: true);
        ApplyToScene("Assets/Scenes/GameScene.unity", onlyFirst: false);
        Debug.Log("Flush setup applied + scenes saved in place.");
    }

    private static void ApplyToScene(string scenePath, bool onlyFirst)
    {
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(ClipPath);
        if (clip == null)
        {
            Debug.LogError($"AudioClip not found at '{ClipPath}'.");
            return;
        }

        var toilets = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None)
            .Where(go => go != null && go.name == "Toilet_1")
            .ToArray();

        if (toilets.Length == 0)
        {
            Debug.LogWarning($"No Toilet_1 found in scene '{scene.name}'.");
        }

        if (onlyFirst && toilets.Length > 1)
            toilets = new[] { toilets[0] };

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

            var t = typeof(ToiletFlushInteractable);
            t.GetField("audioSource", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                ?.SetValue(interactable, audioSource);
            t.GetField("flushClip", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                ?.SetValue(interactable, clip);

            EditorUtility.SetDirty(toilet);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, scenePath);
    }
}
#endif
