using UnityEngine;
using System.Collections.Generic;

public class ObjectController : MonoBehaviour
{
    public bool lockRotation;
    public bool isOnGround;
    public Vector3 defaultScale;
    public Quaternion defaultRotation;
    private void Start()
    {
        InteractionManager.Instance.AddObject(gameObject);
        defaultScale = transform.localScale;
        defaultRotation = transform.rotation;
    }

    void Update()
    {
        if (transform.position.y < InteractionManager.Instance.destroyYValue)
        {
            InteractionManager.Instance.RemoveObject(gameObject);
            Destroy(gameObject);
            HistoryManager.Instance.SaveState(new List<GameObject> { gameObject }, Operation.Delete);
            if (InteractionManager.Instance.GetSelectedObjects().Contains(gameObject) && InteractionManager.Instance.GetUseGizmo())
                GizmoController.Instance.EnableGizmo(false);
        }
        // Maybe not put it here inside the Update()
        // if (InteractionManager.Instance.GetEnablePhysics() && !gameObject.GetComponent<Collider>().isTrigger)
        // {
        //     if (gameObject.CompareTag("Interactable"))
        //         InteractionManager.Instance.EnablePhysics(gameObject);
        // }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Sandbox")
        {
            isOnGround = true;
        }
        else if (other.gameObject.tag == "Interactable")
        {
            isOnGround = false;
        }
    }
    public bool IsOnGround()
    {
        return isOnGround;
    }
}
