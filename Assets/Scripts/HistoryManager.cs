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
        undoStack.Push(action);
        redoStack.Clear();
    }

    public void Undo()
    {
        if (undoStack.Count > 0)
        {
            Action actionToUndo = undoStack.Pop();
            actionToUndo.Undo();
            redoStack.Push(actionToUndo);
        }
    }

    public void Redo()
    {
        if (redoStack.Count > 0)
        {
            Action actionToRedo = redoStack.Pop();
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
        Debug.Log("Offset: " + offset);
        InteractionManager.Instance.DisableAllPhysics();
        foreach (GameObject target in targetObjects)
        {
            target.transform.position += offset;
        }
        InteractionManager.Instance.EnableAllPhysics();
    }

    public override void Redo()
    {
        InteractionManager.Instance.DisableAllPhysics();
        Vector3 offset = newPosition - originalPosition;
        foreach (GameObject target in targetObjects)
        {
            target.transform.position += offset;
        }
        InteractionManager.Instance.EnableAllPhysics();
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
        foreach (GameObject target in targetObjects)
        {
            target.transform.Rotate(offset);
        }
    }
    public override void Redo()
    {
        Vector3 offset = newRotation - originalRotaition;
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
        foreach (GameObject target in targetObjects)
        {
            target.transform.localScale += offset;
        }
    }
    public override void Redo()
    {
        Vector3 offset = newScale - originalScale;
        foreach (GameObject target in targetObjects)
        {
            target.transform.localScale += offset;
        }
    }
}
public class TransformAction : Action
{
    private List<GameObject> targetObjects;
    private Vector3 originalPosition;
    private Vector3 newPosition;
    private Vector3 originalRotaition;
    private Vector3 newRotation;
    private Vector3 originalScale;
    private Vector3 newScale;

    public TransformAction(List<GameObject> target, Vector3 originalPos, Vector3 newPos, Vector3 originalRot, Vector3 newRot, Vector3 originalScl, Vector3 newScl)
    {
        targetObjects = target;
        originalPosition = originalPos;
        newPosition = newPos;
        originalRotaition = originalRot;
        newRotation = newRot;
        originalScale = originalScl;
        newScale = newScl;
    }
    public override void Undo()
    {
        InteractionManager.Instance.DisableAllPhysics();
        Vector3 offset = originalPosition - newPosition;
        foreach (GameObject target in targetObjects)
        {
            target.transform.position += offset;
        }
        offset = originalRotaition - newRotation;
        foreach (GameObject target in targetObjects)
        {
            target.transform.Rotate(offset);
        }
        offset = originalScale - newScale;
        foreach (GameObject target in targetObjects)
        {
            target.transform.localScale += offset;
        }
        InteractionManager.Instance.EnableAllPhysics();
    }
    public override void Redo()
    {
        InteractionManager.Instance.DisableAllPhysics();
        Vector3 offset = newPosition - originalPosition;
        foreach (GameObject target in targetObjects)
        {
            target.transform.position += offset;
        }
        offset = newRotation - originalRotaition;
        foreach (GameObject target in targetObjects)
        {
            target.transform.Rotate(offset);
        }
        offset = newScale - originalScale;
        foreach (GameObject target in targetObjects)
        {
            target.transform.localScale += offset;
        }
        InteractionManager.Instance.EnableAllPhysics();
    }
}