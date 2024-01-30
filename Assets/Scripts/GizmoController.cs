using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    private GameObject _targetObject;

    private void Start()
    {
        _objectMoveGizmo = RTGizmosEngine.Get.CreateObjectMoveGizmo();
        _objectRotationGizmo = RTGizmosEngine.Get.CreateObjectRotationGizmo();
        _objectScaleGizmo = RTGizmosEngine.Get.CreateObjectScaleGizmo();
        _objectUniversalGizmo = RTGizmosEngine.Get.CreateObjectUniversalGizmo();

        EnableGizmo(false);

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
                GameObject pickedObject = InteractionManager.Instance.GetSelectedObject();
                if (pickedObject != _targetObject) OnTargetObjectChanged(pickedObject);
            }
            if (RTInput.WasKeyPressedThisFrame(KeyCode.W))
            {
                SetWorkGizmoId(GizmoId.Move);
            }
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

        if (_targetObject != null)
        {
            _workGizmo.Gizmo.SetEnabled(true);
            _workGizmo.RefreshPositionAndRotation();
        }

    }

    private void OnTargetObjectChanged(GameObject newTargetObject)
    {
        _targetObject = newTargetObject;

        if (_targetObject != null)
        {
            _objectMoveGizmo.SetTargetObject(_targetObject);
            _objectRotationGizmo.SetTargetObject(_targetObject);
            _objectScaleGizmo.SetTargetObject(_targetObject);
            _objectUniversalGizmo.SetTargetObject(_targetObject);

            _workGizmo.Gizmo.SetEnabled(true);
        }
        else
        {
            EnableGizmo(false);
        }
    }
    public void EnableGizmo(bool enable)
    {
        // OnTargetObjectChanged(InteractionManager.Instance.GetSelectedObject());
        _objectMoveGizmo.Gizmo.SetEnabled(enable);
        _objectRotationGizmo.Gizmo.SetEnabled(enable);
        _objectScaleGizmo.Gizmo.SetEnabled(enable);
        _objectUniversalGizmo.Gizmo.SetEnabled(enable);
    }
    public void EnableWorkGizmo(bool enable)
    {
        _workGizmo.SetTargetObject(InteractionManager.Instance.GetSelectedObject());
        _workGizmo.Gizmo.SetEnabled(enable);
    }
    public void RefreshGizmo()
    {
        // _objectMoveGizmo.RefreshPositionAndRotation();
        // _objectRotationGizmo.RefreshPositionAndRotation();
        // _objectScaleGizmo.RefreshPositionAndRotation();
        // _objectUniversalGizmo.RefreshPositionAndRotation();
        _workGizmo.RefreshPositionAndRotation();
    }
}
