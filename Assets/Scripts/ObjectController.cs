using System;
using UnityEngine;

public class ObjectController : MonoBehaviour
{
    public bool lockRotation;
    public bool isOnGround;
    private void Start()
    {
        InteractionManager.Instance.AddObject(gameObject);
    }

    void Update()
    {
        if (transform.position.y < InteractionManager.Instance.destroyYValue)
        {
            InteractionManager.Instance.RemoveObject(gameObject);
            Destroy(gameObject);
            //Fixed an issue here; Have to check that the object being destroyed is also the one selected
            //NOTE: do wee need the former check?
            if (InteractionManager.Instance.GetSelectedObjects().Contains(gameObject) && InteractionManager.Instance.GetUseGizmo())
                GizmoController.Instance.EnableGizmo(false);
        }
        if (InteractionManager.Instance.GetEnablePhysics())
        {
            if (!InteractionManager.Instance.IsIntersecting(gameObject, InteractionManager.Instance.GetSandbox()))
            {
                gameObject.GetComponent<Rigidbody>().isKinematic = false;
                gameObject.GetComponent<Rigidbody>().useGravity = true;
                Physics.IgnoreCollision(gameObject.GetComponent<Collider>(), InteractionManager.Instance.GetSandbox().GetComponent<Collider>(), false);
            }
        }
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
