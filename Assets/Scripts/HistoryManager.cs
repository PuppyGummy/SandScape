using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Text.RegularExpressions;

// using UnityEngine.AddressableAssets;
// using UnityEngine.ResourceManagement.AsyncOperations;


public class HistoryManager : MonoBehaviour
{
    public static HistoryManager Instance;

    private Stack<List<ObjectHistory>> undoStack = new Stack<List<ObjectHistory>>();
    private Stack<List<ObjectHistory>> redoStack = new Stack<List<ObjectHistory>>();

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SaveState(List<GameObject> targets, Operation operation)
    {
        List<ObjectHistory> edits = new List<ObjectHistory>();
        foreach (GameObject target in targets)
        {
            ObjectHistory edit = new ObjectHistory(target, operation);
            edits.Add(edit);
        }
        undoStack.Push(edits);
        redoStack.Clear();
    }

    // Call this function to undo the last operation
    public bool Undo()
    {
        if (undoStack.Count <= 0) return false;

        List<ObjectHistory> lastEdits = undoStack.Pop();
        redoStack.Push(new List<ObjectHistory>(lastEdits));

        foreach (ObjectHistory edit in lastEdits)
        {
            edit.Undo();
        }

        return true;
    }

    // Call this function to redo the last undone operation
    public bool Redo()
    {
        if (redoStack.Count <= 0) return false;

        List<ObjectHistory> nextEdits = redoStack.Pop();
        undoStack.Push(new List<ObjectHistory>(nextEdits));

        foreach (ObjectHistory edit in nextEdits)
        {
            edit.Redo();
        }

        return true;
    }

    public void SaveCurrentState()
    {
        GameData data = new GameData();
        data.sceneID = SceneLoader.Instance.GetLoadedScenario();

        foreach (GameObject obj in InteractionManager.Instance.GetObjects())
        {
            ObjectData objectData = new ObjectData
            {
                objectName = PrefabLoader.GetPrefabName(obj.name),
                position = obj.transform.position,
                rotation = obj.transform.rotation,
                scale = obj.transform.localScale,
                tag = obj.tag,
                customizationData = obj.GetComponent<Customization>().SaveCustomization()
            };

            data.objectsData.Add(objectData);
        }

        SaveLoadManager.SaveGame(data);
    }

    public void LoadState()
    {
        Debug.Log("Loading state");
        GameData data = SaveLoadManager.LoadGame();
        if (data != null)
        {
            SceneLoader.Instance.LoadScene(data.sceneID);
            StartCoroutine(WaitForSceneLoad(data));
        }
    }

    private IEnumerator WaitForSceneLoad(GameData data)
    {
        while (!SceneManager.GetSceneByBuildIndex(data.sceneID).isLoaded)
        {
            yield return null;
        }

        InteractionManager.Instance.ClearAll();
        foreach (ObjectData objectData in data.objectsData)
        {
            GameObject prefab = PrefabLoader.LoadPrefabByName(objectData.objectName);
            if (prefab != null)
            {
                GameObject obj = Instantiate(prefab);
                if (objectData.tag == "Locked")
                {
                    InteractionManager.Instance.LockSingleObject(obj);
                }
                obj.transform.position = objectData.position;
                obj.transform.rotation = objectData.rotation;
                obj.transform.localScale = objectData.scale;
                obj.tag = objectData.tag;
                obj.GetComponent<Customization>().LoadCustomization(objectData.customizationData);
            }
        }
    }
}

public enum Operation
{
    Create,
    Delete,
    Modify,
    Customize
}

[System.Serializable]
public class ObjectHistory
{
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;
    public Operation operation;
    public GameObject target;
    public CustomizationData customizationData;
    public string prefabName;

    public ObjectHistory(GameObject targetObj, Operation op)
    {
        operation = op;
        target = targetObj;
        if (targetObj != null)
        {
            position = targetObj.transform.position;
            rotation = targetObj.transform.rotation;
            scale = targetObj.transform.localScale;
            customizationData = targetObj.GetComponent<Customization>().SaveCustomization();

            prefabName = PrefabLoader.GetPrefabName(targetObj.name);
        }
    }


    public void Undo()
    {
        switch (operation)
        {
            case Operation.Create:
                // To undo a creation, delete the object
                UndoCreate();
                break;
            case Operation.Delete:
                // To undo a deletion, reinstantiate the object
                UndoDelete();
                break;
            case Operation.Modify:
                // To undo a modification, revert to the previous state
                UndoModify();
                break;
            case Operation.Customize:
                // To undo a customization, revert to the previous state
                UndoCustomize();
                break;
        }
    }

    // Call this function to redo an operation
    public void Redo()
    {
        switch (operation)
        {
            case Operation.Create:
                // To redo a creation, reinstantiate the object
                RedoCreate();
                break;
            case Operation.Delete:
                // To redo a deletion, delete the object
                RedoDelete();
                break;
            case Operation.Modify:
                // To redo a modification, reapply the new state
                RedoModify();
                break;
            case Operation.Customize:
                // To redo a customization, reapply the new state
                RedoCustomize();
                break;
        }
    }

    // Define what each Undo helper function does
    private void UndoCreate()
    {
        if (target != null)
        {
            InteractionManager.Instance.RemoveObject(target);
            Object.Destroy(target);
            target = null;
        }
    }

    private void UndoDelete()
    {
        // PrefabLoader.LoadPrefabByName(prefabName, OnPrefabLoaded);
        GameObject prefab = PrefabLoader.LoadPrefabByName(prefabName);
        OnPrefabLoaded(prefab);
    }

    private void UndoModify()
    {
        if (target != null)
        {
            target.transform.position = position;
            target.transform.rotation = rotation;
            target.transform.localScale = scale;
        }
    }
    private void UndoCustomize()
    {
        if (target != null)
        {
            target.GetComponent<Customization>().LoadCustomization(customizationData);
        }
    }

    // Define what each Redo helper function does
    private void RedoCreate()
    {
        // PrefabLoader.LoadPrefabByName(prefabName, OnPrefabLoaded);
        GameObject prefab = PrefabLoader.LoadPrefabByName(prefabName);
        OnPrefabLoaded(prefab);
    }

    private void RedoDelete()
    {
        if (target != null)
        {
            InteractionManager.Instance.RemoveObject(target);
            Object.Destroy(target);
            target = null;
        }
    }

    private void RedoModify()
    {
        if (target != null)
        {
            target.transform.position = position;
            target.transform.rotation = rotation;
            target.transform.localScale = scale;
        }
    }
    private void RedoCustomize()
    {
        if (target != null)
        {
            target.GetComponent<Customization>().LoadCustomization(customizationData);
        }
    }
    private void OnPrefabLoaded(GameObject prefab)
    {
        if (prefab != null)
        {
            target = Object.Instantiate(prefab, position, rotation);
            target.transform.localScale = scale;
            target.GetComponent<Customization>().LoadCustomization(customizationData);
        }
        else
        {
            Debug.LogError("Failed to load prefab.");
        }
    }
}

// public class PrefabLoader : MonoBehaviour
// {
//     public static void LoadPrefabByName(string prefabName, System.Action<GameObject> onComplete)
//     {
//         string[] prefabFolders = { "Animal", "Avatar", "Building", "Furniture", "Monster", "Nature", "Spiritual" };

//         foreach (string folderName in prefabFolders)
//         {
//             LoadPrefabInFolder(prefabName, folderName, onComplete);
//         }
//     }

//     private static void LoadPrefabInFolder(string prefabName, string folderName, System.Action<GameObject> onComplete)
//     {
//         string prefabPath = "Assets/Prefabs/Miniatures/" + folderName + "/" + prefabName + ".prefab";

//         Addressables.LoadResourceLocationsAsync(prefabPath).Completed += locationsHandle =>
//         {
//             if (locationsHandle.Result.Count > 0)
//             {
//                 Addressables.LoadAssetAsync<GameObject>(prefabPath).Completed += assetHandle =>
//                 {
//                     if (assetHandle.Status == AsyncOperationStatus.Succeeded)
//                     {
//                         onComplete?.Invoke(assetHandle.Result);
//                     }
//                 };
//             }
//         };
//     }
// }

public class PrefabLoader : MonoBehaviour
{
    public static GameObject LoadPrefabByName(string prefabName)
    {
        string[] prefabFolders = { "Animal", "Avatar", "Building", "Furniture", "Monster", "Nature", "Spiritual" };

        foreach (string folderName in prefabFolders)
        {
            GameObject prefab = LoadPrefabInFolder(prefabName, folderName);
            if (prefab != null)
            {
                return prefab;
            }
        }

        Debug.LogError("Prefab not found: " + prefabName);
        return null;
    }

    private static GameObject LoadPrefabInFolder(string prefabName, string folderName)
    {
        string prefabPath = "Prefabs/Miniatures/" + folderName + "/" + prefabName;
        GameObject prefab = Resources.Load<GameObject>(prefabPath);
        if (prefab != null)
        {
            return prefab;
        }
        return null;
    }

    public static string GetPrefabName(string objectName)
    {
        string cleanName = Regex.Replace(objectName, @"(\s?\(Clone\))+|\s?\(\d+\)", "");

        return cleanName;
    }

}
