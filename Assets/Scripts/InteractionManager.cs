using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class InteractionManager : MonoBehaviour
{
    #region Fields
    
    private Vector3 offset;
    private Vector3 rotationOrigin;
    private GameObject selectedObject;
    
    private RaycastHit hit;

    /// <summary>
    /// 3D coordinate of the cursors position in the world
    /// </summary>
    private Vector3 pos;

    private float bottomToCenterDistance;
    
    /// <summary>
    /// Rigidbody of the selected object
    /// </summary>
    private Rigidbody selectedRb;

    #endregion

    #region Properties

    public float dragSpeed = 10f;
    public float rotationSpeed = 10f;
    public float minYValue = 0.5f;
    public float buryDepth = 1f;
    public GameObject sandbox;
    public float destroyYValue = -5f;

    #endregion

    private WaitForFixedUpdate waitForFixedUpdate = new WaitForFixedUpdate();

    private static InteractionManager instance;
    public static InteractionManager Instance
    {
        get
        {
            if (!instance)
            {
                instance = FindObjectOfType<InteractionManager>();
            }
            return instance;
        }
    }

    void Update()
    {
        HandleSelectionInput();
        if (selectedObject)
        {
            // HandleDragInput();
            HandleRotationInput();
            HandleScaleInput();
        }
    }
    void FixedUpdate()
    {
        //Set the world position based on the mouse position in screen space
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
   
        //BUG: Ray origin changes to center of object when hovering over an object???
        //The above mentioned may be a non issue
        
        if (Physics.Raycast(ray, out hit, 1000)) //Removed the layer mask so it reacts to other objects
        {
            pos = hit.point;
            
            // Debug.DrawLine(ray.origin, hit.transform.position, Color.red, 1.0f); //Debug ray pos
            // Debug.Log("Distance: " + hit.distance);
        }
    }

    /// <summary>
    /// Handles selection of objects, and starts dragging the object if held
    /// </summary>
    void HandleSelectionInput()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit raycastHit;

            if (!Physics.Raycast(ray, out raycastHit)) return;
            
            GameObject hitObject = raycastHit.collider.gameObject;
            
            if (hitObject.CompareTag("Interactable"))
            {
                if (selectedObject)
                {
                    Outline priorOutline = selectedObject.GetComponent<Outline>();
                    if (priorOutline)
                    {
                        priorOutline.enabled = false;
                        selectedObject.layer = LayerMask.NameToLayer("Objects");
                        selectedObject = null;
                    }
                }
                selectedObject = hitObject;

                Outline outline = selectedObject.GetComponent<Outline>();
                if (outline)
                {
                    outline.enabled = true;
                }
                selectedObject.TryGetComponent(out selectedRb);

                StartCoroutine(DragObject(selectedObject));
                selectedObject.layer = LayerMask.NameToLayer("Ignore Raycast");
            }
            else
            {
                if (!selectedObject) return;
                
                Outline outline = selectedObject.GetComponent<Outline>();
                    
                if (!outline) return;
                    
                outline.enabled = false;
                selectedObject.layer = LayerMask.NameToLayer("Objects");
                selectedObject = null;
            }
        }
        else if (Input.GetMouseButtonUp(0) && selectedObject) //If we release mouse button and there is a selected object
        {
            selectedObject.layer = LayerMask.NameToLayer("Objects"); //Reenable raycasts on the object
        }
    }
    private IEnumerator DragObject(GameObject selectedObject)
    {
        //Freeze the object and change its settings, as to allow for smoother dragging
        if (selectedRb)
        {
            selectedRb.freezeRotation = true;
            selectedRb.useGravity = true;
            Physics.IgnoreCollision(selectedObject.GetComponent<Collider>(), sandbox.GetComponent<Collider>(), false);
        }

        //Move object as long as mouse button is held and the rigid body is valid
        while (Input.GetMouseButton(0))
        {
            if (selectedRb)
            {
                selectedRb.constraints = RigidbodyConstraints.FreezeRotation;
                
                bottomToCenterDistance = selectedObject.GetComponent<Collider>().bounds.extents.y + pos.y;
                selectedObject.transform.position = new Vector3(pos.x, Mathf.Max(pos.y + bottomToCenterDistance, minYValue), pos.z);
            }
            yield return waitForFixedUpdate;
        }

        if (!selectedRb) yield break;
        
        //Reset velocity to prevent it from smashing down too hard
        selectedRb.velocity = Vector3.zero;
        selectedRb.freezeRotation = false;
    }

    void HandleRotationInput()
    {
        //Only rotate object if the right mouse button is held
        if (Input.GetMouseButton(1))
        {
            selectedRb.constraints = RigidbodyConstraints.FreezePosition;

            float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
            float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;

            selectedObject.transform.Rotate(Vector3.up, -mouseX, Space.World);
            selectedObject.transform.Rotate(Vector3.right, mouseY, Space.World);
        }
        else if(!Input.GetMouseButtonUp(1))
        {
            selectedRb.constraints = RigidbodyConstraints.None;
        }
    }

    void HandleScaleInput()
    {
        float scaleSpeed = 0.1f;
        float scrollWheel = Input.GetAxis("Mouse ScrollWheel");

        selectedObject.transform.localScale += new Vector3(scrollWheel, scrollWheel, scrollWheel) * scaleSpeed;
    }
    public void SpawnObject(GameObject associatedObject)
    {
        Instantiate(associatedObject, new Vector3(0f, associatedObject.GetComponent<Renderer>().bounds.extents.y, 0f), transform.rotation);
    }
    public void Reset()
    {
        if (!selectedObject) return;
        
        selectedRb.useGravity = true;
        selectedRb.velocity = Vector3.zero;
        Physics.IgnoreCollision(selectedObject.GetComponent<Collider>(), sandbox.GetComponent<Collider>(), false);
        selectedObject.transform.position = new Vector3(0f, selectedObject.GetComponent<Renderer>().bounds.extents.y, 0f);
        selectedObject.transform.rotation = Quaternion.identity;
        selectedObject.transform.localScale = Vector3.one;
    }
    public void Delete()
    {
        if (selectedObject != null)
        {
            Destroy(selectedObject);
        }
    }
    public void Bury()
    {
        //BUG: If user attempts to bury while on top of another object, it jumps into the air
        //Proposed solution - Only allow burring if on the sandtray and not on other objects
        if (selectedObject == null) return;

        selectedRb.velocity = Vector3.zero;
        selectedRb.freezeRotation = true;
        selectedRb.useGravity = false;
        
        Physics.IgnoreCollision(selectedObject.GetComponent<Collider>(), sandbox.GetComponent<Collider>());
        selectedObject.transform.position = new Vector3(selectedObject.transform.position.x, selectedObject.transform.position.y - buryDepth, selectedObject.transform.position.z);
    }
}
