#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Game.Interactions;

public static class SetupToiletFlushSound
{
    [MenuItem("Tools/Setup/Assign Flush Sound To Toilet_1")]
    public static void Execute()
    {
        const string toiletPath = "FOB_LOD/Props/Stage_0_rightRoom/Toilet_1";
        const string clipPath = "Assets/Music/flush.mp3";

        var toilet = GameObject.Find(toiletPath);
        if (toilet == null)
        {
            Debug.LogError($"Toilet not found at '{toiletPath}'.");
            return;
        }

        var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(clipPath);
        if (clip == null)
        {
            Debug.LogError($"AudioClip not found at '{clipPath}'.");
            return;
        }

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
        EditorSceneManager.MarkSceneDirty(toilet.scene);

        Debug.Log("Assigned flush sound + ensured collider/audio/interactable on Toilet_1.");
    }
}
#endif
