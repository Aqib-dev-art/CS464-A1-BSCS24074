using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Editor-only helper for CS464 Assignment 01.
// Menu: CS464 > 4 - Build Level03
// Level03 idea: a split route. A short risky path (open strip with a 3 m gap)
// versus a long safe path (enclosed corridor with low cover). Both reach the same goal.
// Wayfinding tools: framing (two yellow pillars around the safe doorway)
// and a brighter colour (green stripe) on the intended safe path.
public static class CS464Level03Builder
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

    [MenuItem("CS464/4 - Build Level03")]
    public static void BuildLevel03()
    {
        string[] required = { "Floor", "Wall", "LowCover", "Goal", "Spawn", "Landmark", "PlayerCapsule", "PathStripe" };
        foreach (string r in required)
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabDir + "/" + r + ".prefab") == null)
            {
                Debug.LogError("CS464: prefab '" + r + "' is missing. Run CS464 > 1 (and CS464 > 3 for PathStripe) first.");
                return;
            }
        }

        EnsureFolder(SceneDir);
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        string level01Path = SceneDir + "/Level01.unity";
        string scenePath = SceneDir + "/Level03.unity";

        // Start from a copy of Level01 so the lighting and camera carry over.
        if (!File.Exists(scenePath) && File.Exists(level01Path))
            AssetDatabase.CopyAsset(level01Path, scenePath);

        UnityEngine.SceneManagement.Scene scene;
        if (File.Exists(scenePath))
            scene = EditorSceneManager.OpenScene(scenePath);
        else
            scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        foreach (string n in new[] { "Level01", "Level03" })
        {
            GameObject old = GameObject.Find(n);
            if (old != null) Object.DestroyImmediate(old);
        }
        GameObject root = new GameObject("Level03");

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

        // ---------- Space 2: the fork yard (z 10 to 20, x -12 to 12) ----------
        Place(root, "Floor", "ForkYardFloor", new Vector3(0, -0.1f, 15), 0, new Vector3(24, 0.2f, 10));
        Place(root, "Wall", "ForkWallSideL", new Vector3(-12, 1.5f, 15), 90, new Vector3(10, 3, 0.3f));
        Place(root, "Wall", "ForkWallSideR", new Vector3(12, 1.5f, 15), 90, new Vector3(10, 3, 0.3f));
        // Far wall of the yard at z = 20, with two 3 m openings:
        //   safe opening  x -11 to -8   |   risky opening  x -1.5 to 1.5
        Place(root, "Wall", "ForkFrontFarLeft", new Vector3(-11.5f, 1.5f, 20), 0, new Vector3(1, 3, 0.3f));
        Place(root, "Wall", "ForkFrontMid", new Vector3(-4.75f, 1.5f, 20), 0, new Vector3(6.5f, 3, 0.3f));
        Place(root, "Wall", "ForkFrontRight", new Vector3(6.75f, 1.5f, 20), 0, new Vector3(10.5f, 3, 0.3f));

        // ---------- Risky route: open 3 m strip with a 3 m gap (z 26 to 29) ----------
        Place(root, "Floor", "RiskyFloorA", new Vector3(0, -0.1f, 23), 0, new Vector3(3, 0.2f, 6));
        Place(root, "Floor", "RiskyFloorB", new Vector3(0, -0.1f, 32), 0, new Vector3(3, 0.2f, 6));

        // ---------- Safe route: enclosed 3 m corridor with low cover to weave around ----------
        Place(root, "Floor", "SafeFloor", new Vector3(-9.5f, -0.1f, 27.5f), 0, new Vector3(3, 0.2f, 15));
        Place(root, "Wall", "SafeWallOuter", new Vector3(-11.15f, 1.5f, 27.5f), 90, new Vector3(15, 3, 0.3f));
        Place(root, "Wall", "SafeWallInner", new Vector3(-7.85f, 1.5f, 27.5f), 90, new Vector3(15, 3, 0.3f));
        Place(root, "LowCover", "LowCover1", new Vector3(-10.25f, 0.55f, 24), 0, new Vector3(1.5f, 1.1f, 0.5f));
        Place(root, "LowCover", "LowCover2", new Vector3(-8.75f, 0.55f, 30), 0, new Vector3(1.5f, 1.1f, 0.5f));

        // ---------- Space 3: merge room with the goal (z 35 to 50) ----------
        Place(root, "Floor", "MergeFloor", new Vector3(0, -0.1f, 42.5f), 0, new Vector3(24, 0.2f, 15));
        Place(root, "Wall", "MergeWallL", new Vector3(-12, 1.5f, 42.5f), 90, new Vector3(15, 3, 0.3f));
        Place(root, "Wall", "MergeWallR", new Vector3(12, 1.5f, 42.5f), 90, new Vector3(15, 3, 0.3f));
        Place(root, "Wall", "MergeBackWall", new Vector3(0, 1.5f, 50), 0, new Vector3(24, 3, 0.3f));
        Place(root, "Wall", "MergeFrontFarLeft", new Vector3(-11.5f, 1.5f, 35), 0, new Vector3(1, 3, 0.3f));
        Place(root, "Wall", "MergeFrontMid", new Vector3(-4.75f, 1.5f, 35), 0, new Vector3(6.5f, 3, 0.3f));
        Place(root, "Wall", "MergeFrontRight", new Vector3(6.75f, 1.5f, 35), 0, new Vector3(10.5f, 3, 0.3f));
        Place(root, "Goal", "Goal", new Vector3(0, 0.1f, 47), 0, new Vector3(2, 0.2f, 2));

        // ---------- Wayfinding 1: framing. Two tall yellow pillars flank the safe doorway ----------
        Place(root, "Landmark", "FramePillarL", new Vector3(-11.25f, 2, 19.6f), 0, new Vector3(0.5f, 4, 0.5f));
        Place(root, "Landmark", "FramePillarR", new Vector3(-7.75f, 2, 19.6f), 0, new Vector3(0.5f, 4, 0.5f));

        // ---------- Wayfinding 2: a brighter colour on the intended (safe) path ----------
        Place(root, "PathStripe", "Stripe_Room", new Vector3(0, 0.03f, 6.5f), 0, new Vector3(0.4f, 0.05f, 7));
        Place(root, "PathStripe", "Stripe_Door", new Vector3(0, 0.03f, 11.5f), 0, new Vector3(0.4f, 0.05f, 3));
        Place(root, "PathStripe", "Stripe_HubAcross", new Vector3(-4.75f, 0.03f, 13), 0, new Vector3(9.9f, 0.05f, 0.4f));
        Place(root, "PathStripe", "Stripe_HubToDoor", new Vector3(-9.5f, 0.03f, 16.5f), 0, new Vector3(0.4f, 0.05f, 7));
        Place(root, "PathStripe", "Stripe_Corridor", new Vector3(-9.5f, 0.03f, 27.5f), 0, new Vector3(0.4f, 0.05f, 15));
        Place(root, "PathStripe", "Stripe_MergeDiagonal", new Vector3(-4.75f, 0.03f, 41), 38.4f, new Vector3(0.4f, 0.05f, 15.31f));

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, scenePath);
        AssetDatabase.Refresh();
        Debug.Log("CS464: Level03 built and saved at " + scenePath);
    }
}
