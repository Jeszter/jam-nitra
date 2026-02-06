#if UNITY_EDITOR
using JamNitra.Cutscenes;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace JamNitra.EditorTools
{
    public static class SetupDogFallCutsceneInGameScene1
    {
        public static void Execute()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid())
            {
                Debug.LogError("No valid active scene.");
                return;
            }

            // Ensure we are in GameScene 1 (as requested).
            if (scene.name != "GameScene 1")
                Debug.LogWarning($"Active scene is '{scene.name}', expected 'GameScene 1'. Will still proceed.");

            // Find required scene objects.
            GameObject canvasGO = GameObject.Find("Canvas");
            if (canvasGO == null)
            {
                Debug.LogError("Canvas not found in scene.");
                return;
            }

            var mainCamera = GameObject.Find("Player/Main Camera")?.GetComponent<Camera>();
            if (mainCamera == null)
                mainCamera = Camera.main;

            Transform window = GameObject.Find("FOB_LOD/Windows/Window_1")?.transform;
            if (window == null)
            {
                Debug.LogError("Window transform not found at 'FOB_LOD/Windows/Window_1'.");
                return;
            }

            // Root.
            GameObject root = GameObject.Find("Cutscenes");
            if (root == null)
                root = new GameObject("Cutscenes");

            // Cutscene container.
            GameObject cutsceneGO = GameObject.Find("Cutscenes/DogFallCutscene");
            if (cutsceneGO == null)
            {
                cutsceneGO = new GameObject("DogFallCutscene");
                cutsceneGO.transform.SetParent(root.transform, false);
            }

            // Camera.
            GameObject cutCamGO = GameObject.Find("Cutscenes/DogFallCutscene/CutsceneCamera");
            if (cutCamGO == null)
            {
                cutCamGO = new GameObject("CutsceneCamera");
                cutCamGO.transform.SetParent(cutsceneGO.transform, false);
            }

            var cutCam = cutCamGO.GetComponent<Camera>();
            if (cutCam == null)
                cutCam = cutCamGO.AddComponent<Camera>();

            cutCam.enabled = false;
            cutCam.clearFlags = CameraClearFlags.Skybox;
            cutCam.fieldOfView = 50f;
            cutCam.nearClipPlane = 0.05f;
            cutCam.farClipPlane = 1000f;

            // Dog visual (simple 3D "model" made from primitives).
            GameObject dogGO = GameObject.Find("Cutscenes/DogFallCutscene/DogVisual");
            if (dogGO == null)
            {
                dogGO = new GameObject("DogVisual");
                dogGO.transform.SetParent(cutsceneGO.transform, false);
            }
            else
            {
                // If an old quad exists from previous setup, wipe it.
                var quadMr = dogGO.GetComponent<MeshRenderer>();
                var quadMf = dogGO.GetComponent<MeshFilter>();
                var quadCol = dogGO.GetComponent<Collider>();
                if (quadMr != null) Object.DestroyImmediate(quadMr);
                if (quadMf != null) Object.DestroyImmediate(quadMf);
                if (quadCol != null) Object.DestroyImmediate(quadCol);
            }

            // Replace contents.
            foreach (Transform child in dogGO.transform)
                Object.DestroyImmediate(child.gameObject);

            // Prefer a real imported model if present.
            if (!TryAttachGeneratedDogModel(dogGO.transform))
                CreateDogPrimitiveModel(dogGO.transform);

            dogGO.transform.localScale = Vector3.one;

            // Fade image.
            GameObject fadeGO = GameObject.Find("Canvas/CutsceneFade");
            Image fadeImage;
            if (fadeGO == null)
            {
                fadeGO = new GameObject("CutsceneFade", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                fadeGO.transform.SetParent(canvasGO.transform, false);
                fadeImage = fadeGO.GetComponent<Image>();

                var rt = (RectTransform)fadeGO.transform;
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                rt.anchoredPosition = Vector2.zero;

                fadeImage.raycastTarget = false;
            }
            else
            {
                fadeImage = fadeGO.GetComponent<Image>();
            }

            if (fadeImage != null)
                fadeImage.color = new Color(0f, 0f, 0f, 0f);

            // Ensure fade renders on top.
            fadeGO.transform.SetAsLastSibling();

            // Cutscene component.
            var cutscene = cutsceneGO.GetComponent<DogFallCutscene>();
            if (cutscene == null)
                cutscene = cutsceneGO.AddComponent<DogFallCutscene>();

            // Assign serialized fields.
            AssignSerialized(cutscene, mainCamera, cutCam, window, dogGO.transform, fadeImage);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log("Dog fall cutscene set up in scene.");
        }

        private static void AssignSerialized(
            DogFallCutscene cutscene,
            Camera mainCamera,
            Camera cutsceneCamera,
            Transform windowTransform,
            Transform dogVisual,
            Image fadeImage)
        {
            SerializedObject so = new SerializedObject(cutscene);
            so.FindProperty("mainCamera").objectReferenceValue = mainCamera;
            so.FindProperty("cutsceneCamera").objectReferenceValue = cutsceneCamera;
            so.FindProperty("windowTransform").objectReferenceValue = windowTransform;
            so.FindProperty("dogVisual").objectReferenceValue = dogVisual;
            so.FindProperty("fadeImage").objectReferenceValue = fadeImage;
            so.FindProperty("preDelay").floatValue = 0.1f;
            so.FindProperty("runDuration").floatValue = 0.8f;
            so.FindProperty("jumpDuration").floatValue = 0.35f;
            so.FindProperty("fadeOutDuration").floatValue = 0.4f;
            so.FindProperty("blackHoldDuration").floatValue = 0.1f;
            so.FindProperty("fadeInDuration").floatValue = 0.2f;
            so.FindProperty("dogOffsetFromWindow").vector3Value = new Vector3(0f, 0.1f, -0.6f);
            so.FindProperty("runStartBackDistance").floatValue = 4.5f;
            so.FindProperty("runStartSideOffset").floatValue = 0.35f;
            so.FindProperty("dogHeightOffset").floatValue = 0.05f;
            so.FindProperty("jumpForwardDistance").floatValue = 1.4f;
            so.FindProperty("jumpArcHeight").floatValue = 0.9f;
            // Camera inside apartment, offset mostly backwards from the window.
            so.FindProperty("cameraOffsetFromWindow").vector3Value = new Vector3(0.0f, 1.5f, -2.6f);
            so.FindProperty("cameraLookOffset").vector3Value = new Vector3(0.0f, 0.4f, 0.0f);
            so.FindProperty("loadSceneOnFinish").boolValue = true;
            so.FindProperty("sceneToLoad").stringValue = "MainMenu";
            so.FindProperty("playOnStart").boolValue = true;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static bool TryAttachGeneratedDogModel(Transform root)
        {
            const string modelPath = "Assets/Models/Generated/DogCutscene.glb";

            // GLB importers sometimes don't return a GameObject via LoadAssetAtPath, so search all assets at path.
            Object[] all = AssetDatabase.LoadAllAssetsAtPath(modelPath);
            GameObject model = null;

            if (all != null)
            {
                foreach (Object o in all)
                {
                    if (o is GameObject go)
                    {
                        model = go;
                        break;
                    }
                }
            }

            if (model == null)
                model = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);

            if (model == null)
                return false;

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(model);
            if (instance == null)
                return false;

            instance.name = "DogModel";
            instance.transform.SetParent(root, false);

            // Normalize transform (many GLB imports come in with rotations/scales).
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;

            // Strip colliders (if any).
            foreach (var col in instance.GetComponentsInChildren<Collider>(true))
                Object.DestroyImmediate(col);

            // Auto-scale to a reasonable size (~0.9m long).
            Bounds b = GetHierarchyBounds(instance.transform);
            float maxDim = Mathf.Max(0.0001f, Mathf.Max(b.size.x, Mathf.Max(b.size.y, b.size.z)));
            float target = 0.9f;
            float s = target / maxDim;
            instance.transform.localScale = Vector3.one * s;

            // Put feet on ground at root origin (root is the parent transform).
            Bounds b2 = GetHierarchyBounds(instance.transform);
            float deltaY = root.position.y - b2.min.y;
            instance.transform.position += Vector3.up * deltaY;

            return true;
        }

        private static void CreateDogPrimitiveModel(Transform root)
        {
            // Materials
            Material fur = EnsureUnlitColorMaterial("Assets/Materials/DogCutscene_Fur.mat", new Color(0.72f, 0.46f, 0.2f, 1f));
            Material furDark = EnsureUnlitColorMaterial("Assets/Materials/DogCutscene_FurDark.mat", new Color(0.35f, 0.2f, 0.12f, 1f));
            Material furLight = EnsureUnlitColorMaterial("Assets/Materials/DogCutscene_FurLight.mat", new Color(0.92f, 0.86f, 0.78f, 1f));

            // Body
            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(root, false);
            body.transform.localPosition = new Vector3(0f, 0.45f, 0f);
            body.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            body.transform.localScale = new Vector3(0.55f, 0.9f, 0.55f);
            Object.DestroyImmediate(body.GetComponent<Collider>());
            body.GetComponent<Renderer>().sharedMaterial = fur;

            // Chest (light)
            var chest = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            chest.name = "Chest";
            chest.transform.SetParent(root, false);
            chest.transform.localPosition = new Vector3(0f, 0.5f, 0.18f);
            chest.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            chest.transform.localScale = new Vector3(0.42f, 0.65f, 0.42f);
            Object.DestroyImmediate(chest.GetComponent<Collider>());
            chest.GetComponent<Renderer>().sharedMaterial = furLight;

            // Head
            var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(root, false);
            head.transform.localPosition = new Vector3(0f, 0.78f, 0.62f);
            head.transform.localScale = new Vector3(0.52f, 0.48f, 0.52f);
            Object.DestroyImmediate(head.GetComponent<Collider>());
            head.GetComponent<Renderer>().sharedMaterial = fur;

            // Snout
            var snout = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            snout.name = "Snout";
            snout.transform.SetParent(root, false);
            snout.transform.localPosition = new Vector3(0f, 0.70f, 0.85f);
            snout.transform.localScale = new Vector3(0.28f, 0.22f, 0.34f);
            Object.DestroyImmediate(snout.GetComponent<Collider>());
            snout.GetComponent<Renderer>().sharedMaterial = furLight;

            // Ears
            var earL = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            earL.name = "Ear_L";
            earL.transform.SetParent(root, false);
            earL.transform.localPosition = new Vector3(-0.22f, 0.82f, 0.62f);
            earL.transform.localRotation = Quaternion.Euler(0f, 0f, 25f);
            earL.transform.localScale = new Vector3(0.18f, 0.3f, 0.18f);
            Object.DestroyImmediate(earL.GetComponent<Collider>());
            earL.GetComponent<Renderer>().sharedMaterial = furDark;

            var earR = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            earR.name = "Ear_R";
            earR.transform.SetParent(root, false);
            earR.transform.localPosition = new Vector3(0.22f, 0.82f, 0.62f);
            earR.transform.localRotation = Quaternion.Euler(0f, 0f, -25f);
            earR.transform.localScale = new Vector3(0.18f, 0.3f, 0.18f);
            Object.DestroyImmediate(earR.GetComponent<Collider>());
            earR.GetComponent<Renderer>().sharedMaterial = furDark;

            // Legs
            CreateLeg(root, "Leg_FL", new Vector3(-0.18f, 0.18f, 0.35f), furDark);
            CreateLeg(root, "Leg_FR", new Vector3(0.18f, 0.18f, 0.35f), furDark);
            CreateLeg(root, "Leg_BL", new Vector3(-0.18f, 0.18f, -0.25f), furDark);
            CreateLeg(root, "Leg_BR", new Vector3(0.18f, 0.18f, -0.25f), furDark);

            // Tail
            var tail = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            tail.name = "Tail";
            tail.transform.SetParent(root, false);
            tail.transform.localPosition = new Vector3(0f, 0.62f, -0.55f);
            tail.transform.localRotation = Quaternion.Euler(35f, 0f, 0f);
            tail.transform.localScale = new Vector3(0.14f, 0.35f, 0.14f);
            Object.DestroyImmediate(tail.GetComponent<Collider>());
            tail.GetComponent<Renderer>().sharedMaterial = furDark;

            // Overall scale tweak (looks more like a small dog)
            root.localScale = new Vector3(1.1f, 1.1f, 1.1f);
        }

        private static void CreateLeg(Transform root, string name, Vector3 localPos, Material mat)
        {
            var leg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            leg.name = name;
            leg.transform.SetParent(root, false);
            leg.transform.localPosition = localPos;
            leg.transform.localRotation = Quaternion.identity;
            leg.transform.localScale = new Vector3(0.14f, 0.18f, 0.14f);
            Object.DestroyImmediate(leg.GetComponent<Collider>());
            leg.GetComponent<Renderer>().sharedMaterial = mat;
        }

        private static Bounds GetHierarchyBounds(Transform root)
        {
            var renderers = root.GetComponentsInChildren<Renderer>(true);
            if (renderers == null || renderers.Length == 0)
                return new Bounds(root.position, Vector3.zero);

            Bounds b = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
                b.Encapsulate(renderers[i].bounds);

            // World-space bounds.
            return b;
        }

        private static Material EnsureUnlitColorMaterial(string path, Color color)
        {
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null)
                return existing;

            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null)
                shader = Shader.Find("Unlit/Color");

            var mat = new Material(shader != null ? shader : Shader.Find("Standard"));

            // Try common color property names.
            if (mat.HasProperty("_BaseColor"))
                mat.SetColor("_BaseColor", color);
            else if (mat.HasProperty("_Color"))
                mat.SetColor("_Color", color);

            AssetDatabase.CreateAsset(mat, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return mat;
        }
    }
}
#endif
