using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;


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

    // ... Additional methods ...
}

public enum Operation
{
    Create,
    Delete,
    Modify
}

[System.Serializable]
public class ObjectHistory
{
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;
    public Operation operation;
    public GameObject target;
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

            prefabName = targetObj.name.Replace("(Clone)", "");
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
        PrefabLoader.LoadPrefabByName(prefabName, OnPrefabLoaded);
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

    // Define what each Redo helper function does
    private void RedoCreate()
    {
        PrefabLoader.LoadPrefabByName(prefabName, OnPrefabLoaded);
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
    private void OnPrefabLoaded(GameObject prefab)
    {
        if (prefab != null)
        {
            target = Object.Instantiate(prefab, position, rotation);
            target.transform.localScale = scale;
        }
        else
        {
            Debug.LogError("Failed to load prefab.");
        }
    }
}

public class PrefabLoader : MonoBehaviour
{
    public static void LoadPrefabByName(string prefabName, System.Action<GameObject> onComplete)
    {
        string[] prefabFolders = { "Animal", "Avatar", "Building", "Furniture", "Monster", "Nature", "Spiritual" };

        foreach (string folderName in prefabFolders)
        {
            LoadPrefabInFolder(prefabName, folderName, onComplete);
        }
    }

    private static void LoadPrefabInFolder(string prefabName, string folderName, System.Action<GameObject> onComplete)
    {
        string prefabPath = "Assets/Prefabs/Miniatures/" + folderName + "/" + prefabName + ".prefab";

        Addressables.LoadResourceLocationsAsync(prefabPath).Completed += locationsHandle =>
        {
            if (locationsHandle.Result.Count > 0)
            {
                Addressables.LoadAssetAsync<GameObject>(prefabPath).Completed += assetHandle =>
                {
                    if (assetHandle.Status == AsyncOperationStatus.Succeeded)
                    {
                        onComplete?.Invoke(assetHandle.Result);
                    }
                };
            }
        };
    }
}