using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Editor-only helper for CS464 Assignment 01.
// Menu: CS464 > 3 - Build Level02
// Level02 idea: height. A stair climb, a raised platform, then a ramp to a goal 6 m up.
// Wayfinding tool: leading lines (a green path stripe that runs from the spawn to the goal).
public static class CS464Level02Builder
{
    const string MatDir = "Assets/Materials";
    const string PrefabDir = "Assets/Prefabs";
    const string SceneDir = "Assets/Scenes";

    static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = Path.GetDirectoryName(path).Replace("\\", "/");
        string name = Path.GetFileName(path);
        AssetDatabase.CreateFolder(parent, name);
    }

    // Creates the green "Path" material and the PathStripe prefab (used for the leading lines).
    static void EnsureStripePrefab()
    {
        EnsureFolder(MatDir);
        EnsureFolder(PrefabDir);

        string matPath = MatDir + "/Path.mat";
        Material m = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (m == null)
        {
            Shader s = Shader.Find("Universal Render Pipeline/Lit");
            if (s == null) s = Shader.Find("Standard");
            m = new Material(s);
            Color c = new Color(0.3f, 1f, 0.5f);
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
            else if (m.HasProperty("_Color")) m.SetColor("_Color", c);
            if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", 0.1f);
            AssetDatabase.CreateAsset(m, matPath);
        }

        string prefabPath = PrefabDir + "/PathStripe.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) == null)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "PathStripe";
            go.transform.localScale = new Vector3(0.4f, 0.05f, 1f);
            go.GetComponent<Renderer>().sharedMaterial = m;
            PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            Object.DestroyImmediate(go);
        }
        AssetDatabase.SaveAssets();
    }

    static GameObject Place(GameObject parent, string prefabName, string label,
                            Vector3 pos, Vector3 euler, Vector3 scale)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabDir + "/" + prefabName + ".prefab");
        GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        go.name = label;
        go.transform.SetParent(parent.transform);
        go.transform.position = pos;
        go.transform.rotation = Quaternion.Euler(euler);
        go.transform.localScale = scale;
        return go;
    }

    [MenuItem("CS464/3 - Build Level02")]
    public static void BuildLevel02()
    {
        string[] required = { "Floor", "Wall", "Step", "Ramp", "Spawn", "Goal", "PlayerCapsule" };
        foreach (string r in required)
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabDir + "/" + r + ".prefab") == null)
            {
                Debug.LogError("CS464: prefab '" + r + "' is missing. Run CS464 > 1 - Create Materials and Prefabs first.");
                return;
            }
        }

        EnsureStripePrefab();
        EnsureFolder(SceneDir);

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        string level01Path = SceneDir + "/Level01.unity";
        string scenePath = SceneDir + "/Level02.unity";

        // Start from a copy of Level01 so the lighting and camera carry over.
        if (!File.Exists(scenePath) && File.Exists(level01Path))
            AssetDatabase.CopyAsset(level01Path, scenePath);

        UnityEngine.SceneManagement.Scene scene;
        if (File.Exists(scenePath))
            scene = EditorSceneManager.OpenScene(scenePath);
        else
            scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // Remove the old level geometry (copied Level01 parent or a previous Level02 build).
        foreach (string n in new[] { "Level01", "Level02" })
        {
            GameObject old = GameObject.Find(n);
            if (old != null) Object.DestroyImmediate(old);
        }
        GameObject root = new GameObject("Level02");

        Vector3 noRot = Vector3.zero;

        // ---------- Space 1: spawn room (same footprint as Level01) ----------
        Place(root, "Floor", "SpawnRoomFloor", new Vector3(0, -0.1f, 5), noRot, new Vector3(10, 0.2f, 10));
        Place(root, "Wall", "RoomBackWall", new Vector3(0, 1.5f, 0), noRot, new Vector3(10, 3, 0.3f));
        Place(root, "Wall", "RoomSideWallL", new Vector3(-5, 1.5f, 5), new Vector3(0, 90, 0), new Vector3(10, 3, 0.3f));
        Place(root, "Wall", "RoomSideWallR", new Vector3(5, 1.5f, 5), new Vector3(0, 90, 0), new Vector3(10, 3, 0.3f));
        // Doorway: 3 m wide gap between x = -1.5 and x = 1.5
        Place(root, "Wall", "RoomFrontWallL", new Vector3(-3.25f, 1.5f, 10), noRot, new Vector3(3.5f, 3, 0.3f));
        Place(root, "Wall", "RoomFrontWallR", new Vector3(3.25f, 1.5f, 10), noRot, new Vector3(3.5f, 3, 0.3f));
        Place(root, "Spawn", "Spawn", new Vector3(0, 0.05f, 2), noRot, new Vector3(1.5f, 0.1f, 1.5f));
        Place(root, "PlayerCapsule", "PlayerCapsule", new Vector3(0, 1, 2), noRot, Vector3.one);

        // ---------- The climb: 10 steps, 0.3 m rise each, 3 m wide, 1 m deep ----------
        for (int i = 1; i <= 10; i++)
        {
            float height = 0.3f * i;
            Place(root, "Step", "Step" + i.ToString("00"),
                  new Vector3(0, height / 2f, 9.5f + i), noRot, new Vector3(3, height, 1));
        }
        // Framing walls along the stairs
        Place(root, "Wall", "StairWallL", new Vector3(-1.65f, 1.5f, 15), new Vector3(0, 90, 0), new Vector3(10, 3, 0.3f));
        Place(root, "Wall", "StairWallR", new Vector3(1.65f, 1.5f, 15), new Vector3(0, 90, 0), new Vector3(10, 3, 0.3f));

        // ---------- Space 2: upper platform, top at y = 3 ----------
        Place(root, "Floor", "UpperPlatform", new Vector3(0, 1.5f, 25), noRot, new Vector3(12, 3, 10));

        // ---------- The ramp: rises 3 m over 6 m (about 26.6 degrees, under the 45 limit) ----------
        Place(root, "Ramp", "Ramp", new Vector3(0, 4.41f, 33.045f), new Vector3(-26.57f, 0, 0), new Vector3(3, 0.2f, 6.708f));

        // ---------- Space 3: goal platform, top at y = 6 ----------
        Place(root, "Floor", "GoalPlatform", new Vector3(0, 3, 39), noRot, new Vector3(8, 6, 6));
        Place(root, "Goal", "Goal", new Vector3(0, 6.1f, 39), noRot, new Vector3(2, 0.2f, 2));

        // ---------- Wayfinding: leading lines (green path stripe from spawn to goal) ----------
        Place(root, "PathStripe", "Stripe_Room", new Vector3(0, 0.03f, 6.5f), noRot, new Vector3(0.4f, 0.05f, 7));
        for (int i = 1; i <= 10; i++)
        {
            Place(root, "PathStripe", "Stripe_Step" + i.ToString("00"),
                  new Vector3(0, 0.3f * i + 0.03f, 9.5f + i), noRot, new Vector3(0.4f, 0.05f, 0.9f));
        }
        Place(root, "PathStripe", "Stripe_Upper", new Vector3(0, 3.03f, 25), noRot, new Vector3(0.4f, 0.05f, 10));
        Place(root, "PathStripe", "Stripe_Ramp", new Vector3(0, 4.526f, 32.987f), new Vector3(-26.57f, 0, 0), new Vector3(0.4f, 0.05f, 6.708f));
        Place(root, "PathStripe", "Stripe_Goal", new Vector3(0, 6.03f, 37), noRot, new Vector3(0.4f, 0.05f, 2));

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, scenePath);
        AssetDatabase.Refresh();
        Debug.Log("CS464: Level02 built and saved at " + scenePath);
    }
}
