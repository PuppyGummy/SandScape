using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class HistoryManager : MonoBehaviour
{
    public static HistoryManager Instance;
    private Stack<List<TransformHistory>> undoStack = new Stack<List<TransformHistory>>();
    private Stack<List<TransformHistory>> redoStack = new Stack<List<TransformHistory>>();
    void Awake()
    {
        if (Instance != null)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        GameObject.DontDestroyOnLoad(this.gameObject);
    }

    public void SaveState(List<GameObject> targets)
    {
        List<TransformHistory> edits = new List<TransformHistory>();
        foreach (GameObject target in targets)
        {
            edits.Add(new TransformHistory(target));
        }
        undoStack.Push(edits);
        PrintStack(undoStack);
        redoStack.Clear(); // Clear redo stack on new operation
    }

    public bool Undo()
    {
        if (undoStack.Count <= 1) return false; // Keep the original state

        redoStack.Push(undoStack.Pop());

        List<TransformHistory> prevState = undoStack.Peek();
        foreach (TransformHistory edit in prevState)
        {
            edit.Apply();
        }
        return true;
    }

    public bool Redo()
    {
        if (redoStack.Count == 0) return false;

        List<TransformHistory> nextState = redoStack.Pop();
        undoStack.Push(nextState);

        foreach (TransformHistory edit in nextState)
        {
            edit.Apply();
        }
        return true;
    }
    public void PrintStack(Stack<List<TransformHistory>> stack)
    {
        foreach (List<TransformHistory> edits in stack)
        {
            for (int i = 0; i < edits.Count; i++)
            {
                Debug.Log("edit" + i + " position: " + edits[i].position + " rotation: " + edits[i].rotation + " scale: " + edits[i].scale);
            }
        }
    }
}

[System.Serializable]
public class TransformHistory
{
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;
    public GameObject target; // Reference to the target Transform

    public TransformHistory(GameObject targetObj)
    {
        position = targetObj.transform.position;
        rotation = targetObj.transform.rotation;
        scale = targetObj.transform.localScale;
        target = targetObj;
    }

    public void Apply()
    {
        target.transform.position = position;
        target.transform.rotation = rotation;
        target.transform.localScale = scale;
    }
}