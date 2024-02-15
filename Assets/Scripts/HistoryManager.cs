using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HistoryManager : MonoBehaviour
{
    public static HistoryManager Instance;
    public Stack<Action> undoStack = new Stack<Action>();
    public Stack<Action> redoStack = new Stack<Action>();
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

    public void RecordAction(Action action)
    {
        // action.PrintAction();
        undoStack.Push(action);
        redoStack.Clear();
    }

    public void Undo()
    {
        if (undoStack.Count > 0)
        {
            Action actionToUndo = undoStack.Pop();
            // actionToUndo.PrintAction();
            actionToUndo.Undo();
            redoStack.Push(actionToUndo);
        }
    }

    public void Redo()
    {
        if (redoStack.Count > 0)
        {
            Action actionToRedo = redoStack.Pop();
            // actionToRedo.PrintAction();
            actionToRedo.Redo();
            undoStack.Push(actionToRedo);
        }
    }
}

public abstract class Action
{
    public abstract void Undo();
    public abstract void Redo();
}


public class MoveAction : Action
{
    private List<GameObject> targetObjects;
    private Vector3 originalPosition;
    private Vector3 newPosition;

    public MoveAction(List<GameObject> targets, Vector3 originalPos, Vector3 newPos)
    {
        targetObjects = targets;
        originalPosition = originalPos;
        newPosition = newPos;
    }

    public override void Undo()
    {
        Vector3 offset = originalPosition - newPosition;
        // targetTransform.position = originalPosition;
        foreach (GameObject target in targetObjects)
        {
            target.transform.position += offset;
        }
    }

    public override void Redo()
    {
        Vector3 offset = newPosition - originalPosition;
        // targetTransform.position = newPosition;
        foreach (GameObject target in targetObjects)
        {
            target.transform.position += offset;
        }
    }
}

public class RotateAction : Action
{
    private List<GameObject> targetObjects;
    private Vector3 originalRotaition;
    private Vector3 newRotation;

    public RotateAction(List<GameObject> target, Vector3 originalRot, Vector3 newRot)
    {
        targetObjects = target;
        originalRotaition = originalRot;
        newRotation = newRot;
    }
    public override void Undo()
    {
        Vector3 offset = originalRotaition - newRotation;
        Debug.Log("Offset: " + offset);
        // targetTransform.rotation = originalRotaition;
        foreach (GameObject target in targetObjects)
        {
            target.transform.Rotate(offset);
        }
    }
    public override void Redo()
    {
        Vector3 offset = newRotation - originalRotaition;
        // targetTransform.rotation = newRotation;
        foreach (GameObject target in targetObjects)
        {
            target.transform.Rotate(offset);
        }
    }
}
public class ScaleAction : Action
{
    private List<GameObject> targetObjects;
    private Vector3 originalScale;
    private Vector3 newScale;

    public ScaleAction(List<GameObject> target, Vector3 originalScl, Vector3 newScl)
    {
        targetObjects = target;
        originalScale = originalScl;
        newScale = newScl;
    }
    public override void Undo()
    {
        Vector3 offset = originalScale - newScale;
        // targetTransform.localScale = originalScale;
        foreach (GameObject target in targetObjects)
        {
            target.transform.localScale += offset;
        }
    }
    public override void Redo()
    {
        Vector3 offset = newScale - originalScale;
        // targetTransform.localScale = newScale;
        foreach (GameObject target in targetObjects)
        {
            target.transform.localScale += offset;
        }
    }
}

// public class Action
// {
//     private Transform targetTransform;
//     private Vector3 originalPosition;
//     private Vector3 newPosition;
//     private Quaternion originalRotation;
//     private Quaternion newRotation;
//     private Vector3 originalScale;
//     private Vector3 newScale;

//     public Action(Transform target, Vector3 originalPos, Vector3 newPos, Quaternion originalRot, Quaternion newRot, Vector3 originalScl, Vector3 newScl)
//     {
//         targetTransform = target;
//         originalPosition = originalPos;
//         newPosition = newPos;
//         originalRotation = originalRot;
//         newRotation = newRot;
//         originalScale = originalScl;
//         newScale = newScl;
//     }
//     public Action(Transform target, Vector3 originalPos, Vector3 newPos)
//     {
//         targetTransform = target;
//         originalPosition = originalPos;
//         newPosition = newPos;
//     }
//     public Action(Transform target, Quaternion originalRot, Quaternion newRot)
//     {
//         targetTransform = target;
//         originalRotation = originalRot;
//         newRotation = newRot;
//     }
//     public Action(Transform target, Vector3 originalPos, Vector3 newPos, Vector3 originalScl, Vector3 newScl)
//     {
//         targetTransform = target;
//         originalScale = originalScl;
//         newScale = newScl;
//     }

//     public void Undo()
//     {
//         targetTransform.position = originalPosition;
//         targetTransform.rotation = originalRotation;
//         targetTransform.localScale = originalScale;
//     }

//     public void Redo()
//     {
//         targetTransform.position = newPosition;
//         targetTransform.rotation = newRotation;
//         targetTransform.localScale = newScale;
//     }
//     public void PrintAction()
//     {
//         Debug.Log(targetTransform.ToString() + originalPosition.ToString() + newPosition.ToString() + originalRotation.ToString() + newRotation.ToString() + originalScale.ToString() + newScale.ToString());
//     }
// }
