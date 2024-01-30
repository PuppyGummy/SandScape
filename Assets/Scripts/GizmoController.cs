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

        _objectMoveGizmo.Gizmo.SetEnabled(false);
        _objectRotationGizmo.Gizmo.SetEnabled(false);
        _objectScaleGizmo.Gizmo.SetEnabled(false);
        _objectUniversalGizmo.Gizmo.SetEnabled(false);

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
            if (RTInput.WasKeyPressedThisFrame(KeyCode.W)) SetWorkGizmoId(GizmoId.Move);
            else if (RTInput.WasKeyPressedThisFrame(KeyCode.E)) SetWorkGizmoId(GizmoId.Rotate);
            else if (RTInput.WasKeyPressedThisFrame(KeyCode.R)) SetWorkGizmoId(GizmoId.Scale);
            else if (RTInput.WasKeyPressedThisFrame(KeyCode.T)) SetWorkGizmoId(GizmoId.Universal);
        }
    }

    private void SetWorkGizmoId(GizmoId gizmoId)
    {
        if (gizmoId == _workGizmoId) return;

        _objectMoveGizmo.Gizmo.SetEnabled(false);
        _objectRotationGizmo.Gizmo.SetEnabled(false);
        _objectScaleGizmo.Gizmo.SetEnabled(false);
        _objectUniversalGizmo.Gizmo.SetEnabled(false);

        _workGizmoId = gizmoId;
        if (gizmoId == GizmoId.Move) _workGizmo = _objectMoveGizmo;
        else if (gizmoId == GizmoId.Rotate) _workGizmo = _objectRotationGizmo;
        else if (gizmoId == GizmoId.Scale) _workGizmo = _objectScaleGizmo;
        else if (gizmoId == GizmoId.Universal) _workGizmo = _objectUniversalGizmo;

        if (_targetObject != null) _workGizmo.Gizmo.SetEnabled(true);
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
            _objectMoveGizmo.Gizmo.SetEnabled(false);
            _objectRotationGizmo.Gizmo.SetEnabled(false);
            _objectScaleGizmo.Gizmo.SetEnabled(false);
            _objectUniversalGizmo.Gizmo.SetEnabled(false);
        }
    }
    // public void GetMoveGizmo(GameObject obj)
    // {
    //     objectTransformGizmo = RTGizmosEngine.Get.CreateObjectMoveGizmo();
    //     objectTransformGizmo.SetTargetObject(obj);
    //     MoveGizmo moveGizmo = objectTransformGizmo.Gizmo.MoveGizmo;
    //     moveGizmo.SetVertexSnapTargetObjects(new List<GameObject> { obj });
    // }
    // public void GetRotationGizmo(GameObject obj)
    // {
    //     objectTransformGizmo = RTGizmosEngine.Get.CreateObjectRotationGizmo();
    //     objectTransformGizmo.SetTargetObject(obj);
    // }
    // public void GetScaleGizmo(GameObject obj)
    // {
    //     objectTransformGizmo = RTGizmosEngine.Get.CreateObjectScaleGizmo();
    //     objectTransformGizmo.SetTargetObject(obj);
    // }
    // public void GetUniversalGizmo(List<GameObject> objs)
    // {

    //     objectTransformGizmo = RTGizmosEngine.Get.CreateObjectUniversalGizmo();

    //     objectTransformGizmo.SetTargetObjects(objs);
    //     objectTransformGizmo.Gizmo.UniversalGizmo.SetMvVertexSnapTargetObjects(objs);
    // }
    // public void GetUniversalGizmo(GameObject obj)
    // {
    //     objectTransformGizmo = RTGizmosEngine.Get.CreateObjectUniversalGizmo();
    //     objectTransformGizmo.SetTargetObject(obj);
    //     objectTransformGizmo.GetTargetObjectGroupWorldAABB();
    //     objectTransformGizmo.Gizmo.UniversalGizmo.SetMvVertexSnapTargetObjects(new List<GameObject> { obj });
    // }
    // public IEnumerable<GameObject> GetTargetObjects()
    // {
    //     if (objectTransformGizmo != null)
    //         return objectTransformGizmo.GetTargetObjects();
    //     return null;
    // }
    // public bool ContainsTargetObject(GameObject obj)
    // {
    //     if (objectTransformGizmo != null)
    //     {
    //         IEnumerable<GameObject> targetObjects = objectTransformGizmo.GetTargetObjects();
    //         if (targetObjects != null)
    //         {
    //             return targetObjects.Contains<GameObject>(obj);
    //         }
    //     }
    //     return false;
    // }
    // public void SetGizmo(bool useGizmo)
    // {
    //     if (objectTransformGizmo != null)
    //         objectTransformGizmo.Gizmo.SetEnabled(useGizmo);
    // }
}
