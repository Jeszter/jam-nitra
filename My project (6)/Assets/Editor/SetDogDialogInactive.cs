using UnityEngine;
using UnityEditor;

public class SetDogDialogInactive
{
    [MenuItem("Tools/Set DogDialogUI Inactive")]
    public static void Execute()
    {
        GameObject dogDialog = GameObject.Find("Canvas/DogDialogUI");
        if (dogDialog == null)
        {
            // Try finding it differently
            var canvas = GameObject.Find("Canvas");
            if (canvas != null)
            {
                var dogDialogTransform = canvas.transform.Find("DogDialogUI");
                if (dogDialogTransform != null)
                    dogDialog = dogDialogTransform.gameObject;
            }
        }
        
        if (dogDialog != null)
        {
            dogDialog.SetActive(false);
            EditorUtility.SetDirty(dogDialog);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            Debug.Log("DogDialogUI set to inactive!");
        }
        else
        {
            Debug.LogError("DogDialogUI not found!");
        }
    }
}
