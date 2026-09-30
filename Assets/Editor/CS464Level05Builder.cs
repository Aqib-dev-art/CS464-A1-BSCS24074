using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Editor-only helper for CS464 Assignment 01.
// Menu: CS464 > 6 - Build Level05
// Level05 idea: a three-lane hall with low cover (weave to the open lane, like Subway Surfers),
// followed by four 3 m platform jumps, each rising 0.3 m. It combines earlier ideas:
// weaving around cover, 3 m gaps and rising heights.
// Wayfinding tool: breadcrumbs (small pink cubes along the open lane and over each gap).
public static class CS464Level05Builder
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

    // Creates the pink "Crumb" material and the Breadcrumb prefab.
    static void EnsureBreadcrumbPrefab()
    {
        EnsureFolder(MatDir);
        EnsureFolder(PrefabDir);

        string matPath = MatDir + "/Crumb.mat";
        Material m = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (m == null)
        {
            Shader s = Shader.Find("Universal Render Pipeline/Lit");
            if (s == null) s = Shader.Find("Standard");
            m = new Material(s);
            Color c = new Color(1f, 0.25f, 0.75f);
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
            else if (m.HasProperty("_Color")) m.SetColor("_Color", c);
            if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", 0.1f);
            AssetDatabase.CreateAsset(m, matPath);
        }

        string prefabPath = PrefabDir + "/Breadcrumb.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) == null)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "Breadcrumb";
            go.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
            go.GetComponent<Renderer>().sharedMaterial = m;
            PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            Object.DestroyImmediate(go);
        }
        AssetDatabase.SaveAssets();
    }

    static GameObject Place(GameObject parent, string prefabName, string label,
                            Vector3 pos, float rotY, Vector3 scale)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabDir + "/" + prefabName + ".prefab");
        GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        go.name = label;
        go.transform.SetParent(parent.transform);
        go.transform.position = pos;
        go.transform.rotation = Quaternion.Euler(0, rotY, 0);
        go.transform.localScale = scale;
        return go;
    }

    [MenuItem("CS464/6 - Build Level05")]
    public static void BuildLevel05()
    {
        string[] required = { "Floor", "Wall", "LowCover", "Platform", "Goal", "Spawn", "PlayerCapsule" };
        foreach (string r in required)
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabDir + "/" + r + ".prefab") == null)
            {
                Debug.LogError("CS464: prefab '" + r + "' is missing. Run CS464 > 1 - Create Materials and Prefabs first.");
                return;
            }
        }

        EnsureBreadcrumbPrefab();
        EnsureFolder(SceneDir);
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        string level01Path = SceneDir + "/Level01.unity";
        string scenePath = SceneDir + "/Level05.unity";

        // Start from a copy of Level01 so the lighting and camera carry over.
        if (!File.Exists(scenePath) && File.Exists(level01Path))
            AssetDatabase.CopyAsset(level01Path, scenePath);

        UnityEngine.SceneManagement.Scene scene;
        if (File.Exists(scenePath))
            scene = EditorSceneManager.OpenScene(scenePath);
        else
            scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        foreach (string n in new[] { "Level01", "Level05" })
        {
            GameObject old = GameObject.Find(n);
            if (old != null) Object.DestroyImmediate(old);
        }
        GameObject root = new GameObject("Level05");

        // ---------- Space 1: spawn room (z 0 to 10) ----------
        Place(root, "Floor", "SpawnRoomFloor", new Vector3(0, -0.1f, 5), 0, new Vector3(10, 0.2f, 10));
        Place(root, "Wall", "RoomBackWall", new Vector3(0, 1.5f, 0), 0, new Vector3(10, 3, 0.3f));
        Place(root, "Wall", "RoomSideWallL", new Vector3(-5, 1.5f, 5), 90, new Vector3(10, 3, 0.3f));
        Place(root, "Wall", "RoomSideWallR", new Vector3(5, 1.5f, 5), 90, new Vector3(10, 3, 0.3f));
        // Doorway: 3 m wide (x -1.5 to 1.5)
        Place(root, "Wall", "RoomFrontWallL", new Vector3(-3.25f, 1.5f, 10), 0, new Vector3(3.5f, 3, 0.3f));
        Place(root, "Wall", "RoomFrontWallR", new Vector3(3.25f, 1.5f, 10), 0, new Vector3(3.5f, 3, 0.3f));
        Place(root, "Spawn", "Spawn", new Vector3(0, 0.05f, 2), 0, new Vector3(1.5f, 0.1f, 1.5f));
        Place(root, "PlayerCapsule", "PlayerCapsule", new Vector3(0, 1, 2), 0, Vector3.one);

        // ---------- Space 2: three-lane hall (z 10 to 34, three lanes of 3 m) ----------
        Place(root, "Floor", "LaneHallFloor", new Vector3(0, -0.1f, 22), 0, new Vector3(9, 0.2f, 24));
        Place(root, "Wall", "LaneHallWallL", new Vector3(-4.65f, 1.5f, 22), 90, new Vector3(24, 3, 0.3f));
        Place(root, "Wall", "LaneHallWallR", new Vector3(4.65f, 1.5f, 22), 90, new Vector3(24, 3, 0.3f));

        // Low cover blocks two of three lanes on each row (lane centres x = -3, 0, 3)
        Vector3 coverScale = new Vector3(3, 1.1f, 0.5f);
        Place(root, "LowCover", "Row1_Left", new Vector3(-3, 0.55f, 16), 0, coverScale);   // row 1: right lane open
        Place(root, "LowCover", "Row1_Centre", new Vector3(0, 0.55f, 16), 0, coverScale);
        Place(root, "LowCover", "Row2_Centre", new Vector3(0, 0.55f, 22), 0, coverScale);  // row 2: left lane open
        Place(root, "LowCover", "Row2_Right", new Vector3(3, 0.55f, 22), 0, coverScale);
        Place(root, "LowCover", "Row3_Left", new Vector3(-3, 0.55f, 28), 0, coverScale);   // row 3: centre lane open
        Place(root, "LowCover", "Row3_Right", new Vector3(3, 0.55f, 28), 0, coverScale);

        // ---------- The platform jumps: four 3 m gaps, each landing 0.3 m higher ----------
        Place(root, "Floor", "Platform1", new Vector3(0, -0.1f, 39), 0, new Vector3(4, 0.2f, 4));         // z 37 to 41, top 0.0
        Place(root, "Platform", "Platform2", new Vector3(-3, 0.15f, 46), 0, new Vector3(4, 0.3f, 4));      // z 44 to 48, top 0.3
        Place(root, "Platform", "Platform3", new Vector3(3, 0.3f, 53), 0, new Vector3(4, 0.6f, 4));        // z 51 to 55, top 0.6
        Place(root, "Platform", "GoalPlatform", new Vector3(0, 0.45f, 62), 0, new Vector3(8, 0.9f, 8));    // z 58 to 66, top 0.9
        Place(root, "Goal", "Goal", new Vector3(0, 1.0f, 62), 0, new Vector3(2, 0.2f, 2));

        // ---------- Wayfinding: breadcrumbs (small pink cubes) ----------
        EnsureBreadcrumbPrefab();
        Vector3 crumbScale = new Vector3(0.4f, 0.4f, 0.4f);
        // Along the open lane of each row in the hall (x, z), sitting on the floor
        float[,] hallCrumbs = {
            {0, 11}, {1, 13}, {2.5f, 14.5f}, {3, 16}, {3, 17.5f}, {2, 19}, {0.5f, 20},
            {-1.5f, 21}, {-3, 22}, {-3, 23.5f}, {-2, 25}, {-1, 26.5f}, {0, 28}, {0, 30}, {0, 32}
        };
        for (int i = 0; i < hallCrumbs.GetLength(0); i++)
        {
            Place(root, "Breadcrumb", "Crumb_Hall" + i.ToString("00"),
                  new Vector3(hallCrumbs[i, 0], 0.2f, hallCrumbs[i, 1]), 0, crumbScale);
        }
        // Floating over each gap to show where to jump, and one on each platform
        Place(root, "Breadcrumb", "Crumb_Gap1", new Vector3(0, 1.4f, 35.5f), 0, crumbScale);
        Place(root, "Breadcrumb", "Crumb_P1", new Vector3(0, 0.2f, 39), 0, crumbScale);
        Place(root, "Breadcrumb", "Crumb_Gap2", new Vector3(-1.5f, 1.4f, 42.5f), 0, crumbScale);
        Place(root, "Breadcrumb", "Crumb_P2", new Vector3(-3, 0.5f, 46), 0, crumbScale);
        Place(root, "Breadcrumb", "Crumb_Gap3", new Vector3(0, 1.6f, 49.5f), 0, crumbScale);
        Place(root, "Breadcrumb", "Crumb_P3", new Vector3(3, 0.8f, 53), 0, crumbScale);
        Place(root, "Breadcrumb", "Crumb_Gap4", new Vector3(1.5f, 1.9f, 56.5f), 0, crumbScale);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, scenePath);
        AssetDatabase.Refresh();
        Debug.Log("CS464: Level05 built and saved at " + scenePath);
    }
}
