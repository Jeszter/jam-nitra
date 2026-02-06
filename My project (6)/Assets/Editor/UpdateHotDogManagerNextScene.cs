#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Game.Gameplay;

namespace Game.EditorTools
{
    public static class UpdateHotDogManagerNextScene
    {
        public static void Execute()
        {
            var sampleScenePath = "Assets/Scenes/SampleScene.unity";
            var scene = EditorSceneManager.OpenScene(sampleScenePath, OpenSceneMode.Single);

            var manager = Object.FindAnyObjectByType<HotDogEatManager>();
            if (manager == null)
            {
                Debug.LogWarning($"No {nameof(HotDogEatManager)} found in {sampleScenePath}.");
                return;
            }

            var so = new SerializedObject(manager);
            var prop = so.FindProperty("nextSceneName");
            if (prop != null)
            {
                prop.stringValue = "FlappyBird";
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log($"Updated {nameof(HotDogEatManager)}.nextSceneName to FlappyBird in SampleScene.");
        }

        [MenuItem("Tools/Setup/Update HotDog next scene to FlappyBird (SampleScene)")]
        private static void MenuExecute() => Execute();
    }
}
#endif
