#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Game.Gameplay;

namespace Game.EditorTools
{
    public static class SetupHotDogsInSampleScene
    {
        public static void Execute()
        {
            // Ensure we operate on the currently opened scene.
            var scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded)
            {
                Debug.LogError("No active scene loaded.");
                return;
            }

            // Manager
            var manager = Object.FindAnyObjectByType<HotDogEatManager>();
            if (manager == null)
            {
                var go = new GameObject("HotDogEatManager");
                manager = go.AddComponent<HotDogEatManager>();
            }

            // Hot Dogs by name prefix (Hot Dog, Hot Dog (1) ...)
            var hotDogs = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None)
                .Where(g => g.name != null && g.name.StartsWith("Hot Dog"))
                .ToList();

            foreach (var hd in hotDogs)
            {
                if (hd.GetComponent<HotDogInteractable>() == null)
                    hd.AddComponent<HotDogInteractable>();
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log($"HotDog setup complete. Found {hotDogs.Count} Hot Dog objects.");
        }

        [MenuItem("Tools/Setup/Setup Hot Dogs In Current Scene")]
        private static void MenuExecute() => Execute();
    }
}
#endif
