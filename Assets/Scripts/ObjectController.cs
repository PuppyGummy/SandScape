using UnityEngine;

public class ObjectController : MonoBehaviour
{
    public bool lockRotation;
    public bool isOnGround;

    void Update()
    {
        if (transform.position.y < InteractionManager.Instance.destroyYValue)
        {
            Destroy(gameObject);
            if (InteractionManager.Instance.GetUseGizmo())
                GizmoController.Instance.EnableGizmo(false);
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
