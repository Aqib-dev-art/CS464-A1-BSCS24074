using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Editor-only helper for CS464 Assignment 01.
// Menu: CS464 > 5 - Build Level04
// Level04 idea: pinch and release. The route narrows in stages down to a 1.6 m open bridge
// (the pinch), then opens into a big arena where the goal is revealed (the release).
// Wayfinding tool: pinch and release.
public static class CS464Level04Builder
{
    const string PrefabDir = "Assets/Prefabs";
    const string SceneDir = "Assets/Scenes";

    static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = Path.GetDirectoryName(path).Replace("\\", "/");
        string name = Path.GetFileName(path);
        AssetDatabase.CreateFolder(parent, name);
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

    [MenuItem("CS464/5 - Build Level04")]
    public static void BuildLevel04()
    {
        string[] required = { "Floor", "Wall", "LowCover", "Goal", "Spawn", "PlayerCapsule" };
        foreach (string r in required)
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabDir + "/" + r + ".prefab") == null)
            {
                Debug.LogError("CS464: prefab '" + r + "' is missing. Run CS464 > 1 - Create Materials and Prefabs first.");
                return;
            }
        }

        EnsureFolder(SceneDir);
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        string level01Path = SceneDir + "/Level01.unity";
        string scenePath = SceneDir + "/Level04.unity";

        // Start from a copy of Level01 so the lighting and camera carry over.
        if (!File.Exists(scenePath) && File.Exists(level01Path))
            AssetDatabase.CopyAsset(level01Path, scenePath);

        UnityEngine.SceneManagement.Scene scene;
        if (File.Exists(scenePath))
            scene = EditorSceneManager.OpenScene(scenePath);
        else
            scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        foreach (string n in new[] { "Level01", "Level04" })
        {
            GameObject old = GameObject.Find(n);
            if (old != null) Object.DestroyImmediate(old);
        }
        GameObject root = new GameObject("Level04");

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

        // ---------- The pinch, stage 1: corridor 3 m wide (z 10 to 14) ----------
        Place(root, "Floor", "PinchFloorA", new Vector3(0, -0.1f, 12), 0, new Vector3(3, 0.2f, 4));
        Place(root, "Wall", "PinchWallA_L", new Vector3(-1.65f, 1.5f, 12), 90, new Vector3(4, 3, 0.3f));
        Place(root, "Wall", "PinchWallA_R", new Vector3(1.65f, 1.5f, 12), 90, new Vector3(4, 3, 0.3f));
        Place(root, "Wall", "PinchCapA_L", new Vector3(-1.3f, 1.5f, 14), 0, new Vector3(0.4f, 3, 0.3f));
        Place(root, "Wall", "PinchCapA_R", new Vector3(1.3f, 1.5f, 14), 0, new Vector3(0.4f, 3, 0.3f));

        // ---------- The pinch, stage 2: corridor 2.2 m wide (z 14 to 18) ----------
        Place(root, "Floor", "PinchFloorB", new Vector3(0, -0.1f, 16), 0, new Vector3(2.2f, 0.2f, 4));
        Place(root, "Wall", "PinchWallB_L", new Vector3(-1.25f, 1.5f, 16), 90, new Vector3(4, 3, 0.3f));
        Place(root, "Wall", "PinchWallB_R", new Vector3(1.25f, 1.5f, 16), 90, new Vector3(4, 3, 0.3f));
        Place(root, "Wall", "PinchCapB_L", new Vector3(-0.95f, 1.5f, 18), 0, new Vector3(0.4f, 3, 0.3f));
        Place(root, "Wall", "PinchCapB_R", new Vector3(0.95f, 1.5f, 18), 0, new Vector3(0.4f, 3, 0.3f));

        // ---------- The pinch, stage 3: open bridge 1.6 m wide, 12 m long (z 18 to 30) ----------
        Place(root, "Floor", "NarrowBridge", new Vector3(0, -0.1f, 24), 0, new Vector3(1.6f, 0.2f, 12));

        // ---------- The release: a big arena (z 30 to 54, x -15 to 15) ----------
        Place(root, "Floor", "ArenaFloor", new Vector3(0, -0.1f, 42), 0, new Vector3(30, 0.2f, 24));
        Place(root, "Wall", "ArenaWallL", new Vector3(-15, 1.5f, 42), 90, new Vector3(24, 3, 0.3f));
        Place(root, "Wall", "ArenaWallR", new Vector3(15, 1.5f, 42), 90, new Vector3(24, 3, 0.3f));
        Place(root, "Wall", "ArenaBackWall", new Vector3(0, 1.5f, 54), 0, new Vector3(30, 3, 0.3f));
        // Front wall at z = 30 with a 1.6 m opening where the bridge arrives
        Place(root, "Wall", "ArenaFrontL", new Vector3(-7.9f, 1.5f, 30), 0, new Vector3(14.2f, 3, 0.3f));
        Place(root, "Wall", "ArenaFrontR", new Vector3(7.9f, 1.5f, 30), 0, new Vector3(14.2f, 3, 0.3f));

        // Low cover scattered in the arena
        Place(root, "LowCover", "ArenaCover1", new Vector3(-8, 0.55f, 36), 0, new Vector3(3, 1.1f, 0.5f));
        Place(root, "LowCover", "ArenaCover2", new Vector3(8, 0.55f, 36), 0, new Vector3(3, 1.1f, 0.5f));
        Place(root, "LowCover", "ArenaCover3", new Vector3(-4, 0.55f, 44), 0, new Vector3(3, 1.1f, 0.5f));
        Place(root, "LowCover", "ArenaCover4", new Vector3(4, 0.55f, 44), 0, new Vector3(3, 1.1f, 0.5f));

        // The goal, revealed across the arena
        Place(root, "Goal", "Goal", new Vector3(0, 0.1f, 50), 0, new Vector3(2, 0.2f, 2));

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, scenePath);
        AssetDatabase.Refresh();
        Debug.Log("CS464: Level04 built and saved at " + scenePath);
    }
}
