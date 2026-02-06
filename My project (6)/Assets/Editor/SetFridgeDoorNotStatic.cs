using UnityEditor;
using UnityEngine;

public static class SetFridgeDoorNotStatic
{
    public static void Execute()
    {
        var go = GameObject.Find("FOB_LOD/Props/Stage_1_rightRoom/Fridge_1/Fridge_1_Door_1");
        if (go == null)
        {
            Debug.LogError("Fridge_1_Door_1 not found");
            return;
        }

        int changed = 0;
        foreach (var t in go.GetComponentsInChildren<Transform>(true))
        {
            if (t.gameObject.isStatic)
            {
                t.gameObject.isStatic = false;
                EditorUtility.SetDirty(t.gameObject);
                changed++;
            }
        }

        Debug.Log($"SetFridgeDoorNotStatic: cleared static on {changed} objects.");
    }
}
