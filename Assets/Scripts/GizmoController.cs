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

    private ObjectTransformGizmo _objectMoveGizmo;
    private ObjectTransformGizmo _objectRotationGizmo;
    private ObjectTransformGizmo _objectScaleGizmo;
    private ObjectTransformGizmo _objectUniversalGizmo;

    private GizmoId _workGizmoId;
    private ObjectTransformGizmo _workGizmo;
    private List<GameObject> _targetObjects;

    private void Start()
    {
        _targetObjects = new List<GameObject>();
        _objectMoveGizmo = RTGizmosEngine.Get.CreateObjectMoveGizmo();
        _objectRotationGizmo = RTGizmosEngine.Get.CreateObjectRotationGizmo();
        _objectScaleGizmo = RTGizmosEngine.Get.CreateObjectScaleGizmo();
        _objectUniversalGizmo = RTGizmosEngine.Get.CreateObjectUniversalGizmo();

        EnableGizmo(false);
        _objectMoveGizmo.SetTargetObjects(_targetObjects);
        _objectRotationGizmo.SetTargetObjects(_targetObjects);
        _objectScaleGizmo.SetTargetObjects(_targetObjects);
        _objectUniversalGizmo.SetTargetObjects(_targetObjects);
        _workGizmo = _objectMoveGizmo;
        _workGizmoId = GizmoId.Move;
    }

    private void Update()
    {
        if (InteractionManager.Instance.GetUseGizmo())
        {
            if (RTInput.WasLeftMouseButtonPressedThisFrame() &&
                RTGizmosEngine.Get.HoveredGizmo == null)
            {
                _targetObjects = InteractionManager.Instance.GetSelectedObjects();
                // if (pickedObject != _targetObject) OnTargetObjectChanged(pickedObject);
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
        if (gizmoId == _workGizmoId) return;

        EnableGizmo(false);

        _workGizmoId = gizmoId;
        if (gizmoId == GizmoId.Move) _workGizmo = _objectMoveGizmo;
        else if (gizmoId == GizmoId.Rotate) _workGizmo = _objectRotationGizmo;
        else if (gizmoId == GizmoId.Scale) _workGizmo = _objectScaleGizmo;
        else if (gizmoId == GizmoId.Universal) _workGizmo = _objectUniversalGizmo;

        if (_targetObjects.Count != 0)
        {
            _workGizmo.Gizmo.SetEnabled(true);
            _workGizmo.RefreshPositionAndRotation();
        }
    }

    public void OnSelectionChanged()
    {
        if (_targetObjects.Count != 0)
        {

            _workGizmo.Gizmo.SetEnabled(true);
            _workGizmo.RefreshPositionAndRotation();
        }
        else
        {
            EnableGizmo(false);
        }
    }
    public void EnableGizmo(bool enable)
    {
        _objectMoveGizmo.Gizmo.SetEnabled(enable);
        _objectRotationGizmo.Gizmo.SetEnabled(enable);
        _objectScaleGizmo.Gizmo.SetEnabled(enable);
        _objectUniversalGizmo.Gizmo.SetEnabled(enable);
    }
    public void EnableWorkGizmo(bool enable)
    {
        // _targetObject = InteractionManager.Instance.GetSelectedObject();
        // _workGizmo.SetTargetObject(InteractionManager.Instance.GetSelectedObject());
        _workGizmo.Gizmo.SetEnabled(enable);
    }
    public void RefreshGizmo()
    {
        _workGizmo.RefreshPositionAndRotation();
    }
    public bool IsHoveringGizmo()
    {
        return _workGizmo.Gizmo.IsHovered;
    }
}
