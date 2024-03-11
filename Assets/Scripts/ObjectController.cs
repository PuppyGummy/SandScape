using System;
using UnityEngine;
using UnityEngine.Serialization;

public class ObjectController : MonoBehaviour
{
    public bool locked;
    public bool isOnGround;
    public Vector3 defaultScale;
    public Quaternion defaultRotation;
    private void Start()
    {
        InteractionManager.Instance.AddObject(gameObject);
        defaultScale = transform.localScale;
        defaultRotation = transform.rotation;
        
        if(locked)
            InteractionManager.Instance.LockSingleObject(gameObject);
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
