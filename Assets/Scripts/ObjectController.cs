using UnityEngine;

public class ObjectController : MonoBehaviour
{
    public bool isOnGround;

    void Update()
    {
        if (transform.position.y < InteractionManager.Instance.destroyYValue)
        {
            Destroy(gameObject);
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
    // private void OnCollisionExit(Collision other)
    // {
    //     if (other.gameObject.tag == "Sandbox")
    //     {
    //         isOnGround = false;
    //     }
    // }
    public bool IsOnGround()
    {
        return isOnGround;
    }
}
