using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Editor-only helper for CS464 Assignment 01.
// Menu: CS464 > 1 - Create Materials and Prefabs
//       CS464 > 2 - Build Level01
public static class CS464KitBuilder
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

    static Material MakeMat(string name, Color c)
    {
        string path = MatDir + "/" + name + ".mat";
        Material m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (m == null)
        {
            Shader s = Shader.Find("Universal Render Pipeline/Lit");
            if (s == null) s = Shader.Find("Standard");
            m = new Material(s);
            AssetDatabase.CreateAsset(m, path);
        }
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        else if (m.HasProperty("_Color")) m.SetColor("_Color", c);
        if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", 0.1f);
        EditorUtility.SetDirty(m);
        return m;
    }

    static void MakePrefab(string name, PrimitiveType type, Vector3 scale, Material mat)
    {
        GameObject go = GameObject.CreatePrimitive(type);
        go.name = name;
        go.transform.localScale = scale;
        go.GetComponent<Renderer>().sharedMaterial = mat;
        PrefabUtility.SaveAsPrefabAsset(go, PrefabDir + "/" + name + ".prefab");
        Object.DestroyImmediate(go);
    }

    [MenuItem("CS464/1 - Create Materials and Prefabs")]
    public static void CreateKit()
    {
        EnsureFolder(MatDir);
        EnsureFolder(PrefabDir);

        Material floor = MakeMat("Floor", new Color(0.5f, 0.5f, 0.5f));
        Material wall = MakeMat("Wall", new Color(0.8f, 0.8f, 0.8f));
        Material cover = MakeMat("Cover", new Color(1f, 0.5f, 0.1f));
        Material goal = MakeMat("Goal", new Color(0.2f, 0.8f, 0.3f));
        Material landmark = MakeMat("Landmark", new Color(1f, 0.85f, 0.1f));
        Material player = MakeMat("Player", new Color(0.2f, 0.4f, 0.9f));

        MakePrefab("Floor", PrimitiveType.Cube, new Vector3(1, 0.2f, 1), floor);
        MakePrefab("Wall", PrimitiveType.Cube, new Vector3(1, 3, 0.3f), wall);
        MakePrefab("LowCover", PrimitiveType.Cube, new Vector3(1, 1.1f, 0.5f), cover);
        MakePrefab("Goal", PrimitiveType.Cube, new Vector3(2, 0.2f, 2), goal);
        MakePrefab("Spawn", PrimitiveType.Cube, new Vector3(1.5f, 0.1f, 1.5f), player);
        MakePrefab("Landmark", PrimitiveType.Cube, new Vector3(3, 15, 3), landmark);
        // Extra pieces for Levels 02-05
        MakePrefab("Step", PrimitiveType.Cube, new Vector3(3, 0.3f, 1), floor);
        MakePrefab("Ramp", PrimitiveType.Cube, new Vector3(3, 0.2f, 6), floor);
        MakePrefab("Platform", PrimitiveType.Cube, new Vector3(4, 0.5f, 4), floor);
        MakePrefab("PlayerCapsule", PrimitiveType.Capsule, Vector3.one, player);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("CS464: materials and prefabs created in Assets/Materials and Assets/Prefabs.");
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

    [MenuItem("CS464/2 - Build Level01")]
    public static void BuildLevel01()
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabDir + "/Floor.prefab") == null)
            CreateKit();

        EnsureFolder(SceneDir);
        string scenePath = SceneDir + "/Level01.unity";
        UnityEngine.SceneManagement.Scene scene;
        if (File.Exists(scenePath))
            scene = EditorSceneManager.OpenScene(scenePath);
        else
            scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        GameObject old = GameObject.Find("Level01");
        if (old != null) Object.DestroyImmediate(old);
        GameObject root = new GameObject("Level01");

        // Floors (top surface at y = 0). Gap between z = 26 and z = 29.
        Place(root, "Floor", "SpawnRoomFloor", new Vector3(0, -0.1f, 5), 0, new Vector3(10, 0.2f, 10));
        Place(root, "Floor", "CorridorFloor", new Vector3(0, -0.1f, 15), 0, new Vector3(3, 0.2f, 10));
        Place(root, "Floor", "YardPlatformA", new Vector3(0, -0.1f, 23), 0, new Vector3(14, 0.2f, 6));
        Place(root, "Floor", "YardPlatformB", new Vector3(0, -0.1f, 33), 0, new Vector3(14, 0.2f, 8));

        // Walls (3 m tall)
        Place(root, "Wall", "RoomBackWall", new Vector3(0, 1.5f, 0), 0, new Vector3(10, 3, 0.3f));
        Place(root, "Wall", "RoomSideWallL", new Vector3(-5, 1.5f, 5), 90, new Vector3(10, 3, 0.3f));
        Place(root, "Wall", "RoomSideWallR", new Vector3(5, 1.5f, 5), 90, new Vector3(10, 3, 0.3f));
        Place(root, "Wall", "RoomFrontWallL", new Vector3(-3.25f, 1.5f, 10), 0, new Vector3(3.5f, 3, 0.3f));
        Place(root, "Wall", "RoomFrontWallR", new Vector3(3.25f, 1.5f, 10), 0, new Vector3(3.5f, 3, 0.3f));
        Place(root, "Wall", "CorridorWallL", new Vector3(-1.65f, 1.5f, 15), 90, new Vector3(10, 3, 0.3f));
        Place(root, "Wall", "CorridorWallR", new Vector3(1.65f, 1.5f, 15), 90, new Vector3(10, 3, 0.3f));

        // Low cover, spawn, goal, landmark
        Place(root, "LowCover", "LowCover1", new Vector3(-3, 0.55f, 23), 0, new Vector3(3, 1.1f, 0.5f));
        Place(root, "LowCover", "LowCover2", new Vector3(3, 0.55f, 23), 0, new Vector3(3, 1.1f, 0.5f));
        Place(root, "Spawn", "Spawn", new Vector3(0, 0.05f, 2), 0, new Vector3(1.5f, 0.1f, 1.5f));
        Place(root, "Goal", "Goal", new Vector3(0, 0.1f, 35), 0, new Vector3(2, 0.2f, 2));
        Place(root, "Landmark", "Landmark", new Vector3(0, 7.5f, 44), 0, new Vector3(3, 15, 3));

        // Player capsule 2 m x 1 m at the spawn
        Place(root, "PlayerCapsule", "PlayerCapsule", new Vector3(0, 1, 2), 0, Vector3.one);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, scenePath);
        Debug.Log("CS464: Level01 built and saved at " + scenePath);
    }
}
