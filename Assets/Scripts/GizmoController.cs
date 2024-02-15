using UnityEngine;
using System.Collections.Generic;
using RTG;

public class GizmoController : MonoBehaviour
{
    public static GizmoController Instance;
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

    private enum GizmoId
    {
        Move = 1,
        Rotate,
        Scale,
        Universal
    }

    private ObjectTransformGizmo objectMoveGizmo;
    private ObjectTransformGizmo objectRotationGizmo;
    private ObjectTransformGizmo objectScaleGizmo;
    private ObjectTransformGizmo objectUniversalGizmo;

    private GizmoId workGizmoId;
    private ObjectTransformGizmo workGizmo;
    // private List<GameObject> targetObjects;

    private void Start()
    {
        // targetObjects = new List<GameObject>();
        objectMoveGizmo = RTGizmosEngine.Get.CreateObjectMoveGizmo();
        objectRotationGizmo = RTGizmosEngine.Get.CreateObjectRotationGizmo();
        objectScaleGizmo = RTGizmosEngine.Get.CreateObjectScaleGizmo();
        objectUniversalGizmo = RTGizmosEngine.Get.CreateObjectUniversalGizmo();

        EnableGizmo(false);
        objectMoveGizmo.SetTargetObjects(GetTartgetObjects());
        objectRotationGizmo.SetTargetObjects(GetTartgetObjects());
        objectScaleGizmo.SetTargetObjects(GetTartgetObjects());
        objectUniversalGizmo.SetTargetObjects(GetTartgetObjects());
        workGizmo = objectMoveGizmo;
        workGizmoId = GizmoId.Move;
    }

    private void Update()
    {
        if (InteractionManager.Instance.GetUseGizmo())
        {
            if (RTInput.WasLeftMouseButtonPressedThisFrame() &&
                RTGizmosEngine.Get.HoveredGizmo == null)
            {
                workGizmo.SetTargetObjects(GetTartgetObjects());
                OnSelectionChanged();
            }
            if (RTInput.WasKeyPressedThisFrame(KeyCode.W)) SetWorkGizmoId(GizmoId.Move);
            else if (RTInput.WasKeyPressedThisFrame(KeyCode.E)) SetWorkGizmoId(GizmoId.Rotate);
            else if (RTInput.WasKeyPressedThisFrame(KeyCode.R)) SetWorkGizmoId(GizmoId.Scale);
            else if (RTInput.WasKeyPressedThisFrame(KeyCode.T)) SetWorkGizmoId(GizmoId.Universal);
        }
    }

    private void SetWorkGizmoId(GizmoId gizmoId)
    {
        if (gizmoId == workGizmoId) return;

        EnableGizmo(false);

        workGizmoId = gizmoId;
        if (gizmoId == GizmoId.Move) workGizmo = objectMoveGizmo;
        else if (gizmoId == GizmoId.Rotate) workGizmo = objectRotationGizmo;
        else if (gizmoId == GizmoId.Scale) workGizmo = objectScaleGizmo;
        else if (gizmoId == GizmoId.Universal) workGizmo = objectUniversalGizmo;

        if (InteractionManager.Instance.GetSelectedObjects().Count != 0)
        {
            workGizmo.Gizmo.SetEnabled(true);
            workGizmo.SetTargetObjects(GetTartgetObjects());
            workGizmo.RefreshPositionAndRotation();
        }
    }

    public void OnSelectionChanged()
    {
        if (InteractionManager.Instance.GetUseGizmo())
            if (InteractionManager.Instance.GetSelectedObjects().Count != 0)
            {
                workGizmo.Gizmo.SetEnabled(true);
                // workGizmo.SetTargetObjects(InteractionManager.Instance.GetSelectedObjects());
                workGizmo.RefreshPositionAndRotation();
            }
            else
            {
                EnableGizmo(false);
            }
    }
    public void EnableGizmo(bool enable)
    {
        objectMoveGizmo.Gizmo.SetEnabled(enable);
        objectRotationGizmo.Gizmo.SetEnabled(enable);
        objectScaleGizmo.Gizmo.SetEnabled(enable);
        objectUniversalGizmo.Gizmo.SetEnabled(enable);
    }
    public void EnableWorkGizmo(bool enable)
    {
        workGizmo.SetTargetObjects(GetTartgetObjects());
        workGizmo.Gizmo.SetEnabled(enable);
    }
    public List<GameObject> GetTartgetObjects()
    {
        List<GameObject> selectedObjects = InteractionManager.Instance.GetSelectedObjects();
        List<GameObject> targetObjects = new List<GameObject>();
        foreach (GameObject obj in selectedObjects)
        {
            if (obj.CompareTag("Interactable"))
            {
                targetObjects.Add(obj);
            }
        }
        return targetObjects;
    }
    public void RefreshGizmo()
    {
        workGizmo.RefreshPositionAndRotation();
    }
    public bool IsHoveringGizmo()
    {
        return workGizmo.Gizmo.IsHovered;
    }
}
