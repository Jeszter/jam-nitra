using UnityEditor;
using UnityEngine;
using Game.Doors;

/// <summary>
/// Creates a hinge pivot for the fridge door so it opens like a real door (around an edge).
/// </summary>
public static class SetupFridgeDoorHinge
{
    public static void Execute()
    {
        const string doorPath = "FOB_LOD/Props/Stage_1_rightRoom/Fridge_1/Fridge_1_Door_1";

        var door = GameObject.Find(doorPath);
        if (door == null)
        {
            Debug.LogError($"Door not found: {doorPath}");
            return;
        }

        // If already set up, do nothing.
        if (door.transform.parent != null && door.transform.parent.name.Contains("Hinge"))
        {
            Debug.Log("Fridge door already has a hinge parent.");
            return;
        }

        var renderer = door.GetComponentInChildren<Renderer>();
        if (renderer == null)
        {
            Debug.LogError("Renderer not found on fridge door.");
            return;
        }

        // Compute hinge position at the left edge of the renderer bounds (world space).
        var b = renderer.bounds;
        var hingeWorld = new Vector3(b.min.x, b.center.y, b.center.z);

        var parent = door.transform.parent;

        var hingeGO = new GameObject(door.name + "_Hinge", typeof(BoxCollider));
        hingeGO.transform.SetParent(parent, true);
        hingeGO.transform.position = hingeWorld;
        hingeGO.transform.rotation = door.transform.rotation;
        hingeGO.transform.localScale = Vector3.one;
        hingeGO.isStatic = false;

        // Reparent door under hinge, keep world transform.
        door.transform.SetParent(hingeGO.transform, true);
        door.isStatic = false;

        // Remove any DoorInteractable from the door itself.
        var existingOnDoor = door.GetComponent<DoorInteractable>();
        if (existingOnDoor != null)
            Object.DestroyImmediate(existingOnDoor);

        // Add DoorInteractable to hinge.
        var di = hingeGO.GetComponent<DoorInteractable>();
        if (di == null) di = hingeGO.AddComponent<DoorInteractable>();
        di.SetRotateTarget(hingeGO.transform);

        // Make it open similar to DoorMech: close -90 -> open -170 => delta -80
        var so = new SerializedObject(di);
        so.FindProperty("openAngle").floatValue = -80f;
        so.FindProperty("openSpeed").floatValue = 6f;
        so.ApplyModifiedPropertiesWithoutUndo();

        // Fit collider on hinge to the door renderer bounds.
        var bc = hingeGO.GetComponent<BoxCollider>();
        FitBoxColliderToBounds(hingeGO.transform, bc, b);

        // Disable built-in DoorMech if present.
        var mech = door.GetComponent<DoorMech>();
        if (mech != null)
            mech.enabled = false;

        EditorUtility.SetDirty(hingeGO);
        EditorUtility.SetDirty(door);
        EditorUtility.SetDirty(di);
        EditorUtility.SetDirty(bc);

        Debug.Log("SetupFridgeDoorHinge: created hinge + added DoorInteractable.");
    }

    private static void FitBoxColliderToBounds(Transform root, BoxCollider bc, Bounds worldBounds)
    {
        var centerLocal = root.InverseTransformPoint(worldBounds.center);
        var sizeLocal = root.InverseTransformVector(worldBounds.size);
        bc.center = centerLocal;
        bc.size = new Vector3(Mathf.Abs(sizeLocal.x), Mathf.Abs(sizeLocal.y), Mathf.Abs(sizeLocal.z));
        bc.isTrigger = false;
    }
}
