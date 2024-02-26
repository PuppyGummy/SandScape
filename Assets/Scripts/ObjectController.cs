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
            if (InteractionManager.Instance.GetSelectedObjects().Contains(gameObject) && InteractionManager.Instance.GetUseGizmo())
                GizmoController.Instance.EnableGizmo(false);
        }

        if (InteractionManager.Instance.GetEnablePhysics() && !gameObject.GetComponent<Collider>().isTrigger)
        {
            if (gameObject.CompareTag("Interactable"))
                InteractionManager.Instance.EnablePhysics(gameObject);
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
