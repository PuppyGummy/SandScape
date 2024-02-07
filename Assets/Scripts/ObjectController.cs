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
            if (InteractionManager.Instance.GetSelectedObject() == gameObject && InteractionManager.Instance.GetUseGizmo())
                GizmoController.Instance.EnableGizmo(false);
        }
    }

    private void Start()
    {
        InteractionManager.Instance.AddObject(gameObject);
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
