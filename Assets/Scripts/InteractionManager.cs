using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class InteractionManager : MonoBehaviour
{
    private bool isDragging = false;
    private Vector3 offset;

    private bool isRotating = false;
    private Vector3 rotationOrigin;
    private GameObject selectedObject;
    public float dragSpeed = 10f;
    public float rotationSpeed = 10f;
    public float minYValue = 0.5f;
    // private float bottomToCenterDistance;
    private RaycastHit hit;
    public LayerMask layerMask;
    private Vector3 pos;
    public float buryDepth = 1f;
    private float bottomToCenterDistance;
    public GameObject sandbox;
    public float destroyYValue = -5f;
    private Rigidbody selectedRb;

    private WaitForFixedUpdate waitForFixedUpdate = new WaitForFixedUpdate();

    private static InteractionManager instance;
    public static InteractionManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<InteractionManager>();
            }
            return instance;
        }
    }
    private void Start()
    {
    }

    void Update()
    {
        HandleSelectionInput();
        if (selectedObject != null)
        {
            // HandleDragInput();
            HandleRotationInput();
            HandleScaleInput();
        }
    }
    void FixedUpdate()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //BUG: Currently the rays interact with *ALL* objects, also the one that is held. Make sure to ignore held objects!
        //BUG: Ray origin changes to center of object when hovering over an object???
        if (Physics.Raycast(ray, out hit, 1000)) //Removed the layer mask so it reacts to other objects
        {
            /*if(selectedObject != null && hit.collider.gameObject == selectedObject)
                return;*/
            
            pos = hit.point;
            Debug.DrawLine(ray.origin, hit.transform.position, Color.red, 1.0f); //Debug ray pos
            Debug.Log("Distance: " + hit.distance);
        }
    }

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

            if (Physics.Raycast(ray, out raycastHit))
            {
                GameObject hitObject = raycastHit.collider.gameObject;

                if (hitObject.CompareTag("Interactable"))
                {
                    if (selectedObject != null)
                    {
                        Outline priorOutline = selectedObject.GetComponent<Outline>();
                        if (priorOutline != null)
                        {
                            priorOutline.enabled = false;
                            selectedObject.layer = LayerMask.NameToLayer("Objects");
                            selectedObject = null;
                        }
                    }
                    selectedObject = hitObject;

                    Outline outline = selectedObject.GetComponent<Outline>();
                    if (outline != null)
                    {
                        outline.enabled = true;
                    }
                    selectedObject.TryGetComponent(out selectedRb);

                    StartCoroutine(DragObject(selectedObject));
                    selectedObject.layer = LayerMask.NameToLayer("Ignore Raycast");
                }
                else
                {
                    if (selectedObject != null)
                    {
                        Outline outline = selectedObject.GetComponent<Outline>();
                        if (outline != null)
                        {
                            outline.enabled = false;
                            selectedObject.layer = LayerMask.NameToLayer("Objects");
                            selectedObject = null;
                        }
                    }
                }
            }
        }
        else if (Input.GetMouseButtonUp(0) && selectedObject) //If we release mouse button and there is a selected object
        {
            selectedObject.layer = LayerMask.NameToLayer("Objects"); //Reenable raycasts on the object
        }
    }
    private IEnumerator DragObject(GameObject selectedObject)
    {
        // float distance = Vector3.Distance(selectedObject.transform.position, Camera.main.transform.position);
        // selectedObject.TryGetComponent(out Rigidbody rb);

        if (selectedRb != null)
        {
            selectedRb.freezeRotation = true;
            selectedRb.useGravity = true;
            Physics.IgnoreCollision(selectedObject.GetComponent<Collider>(), sandbox.GetComponent<Collider>(), false);
        }

        while (Input.GetMouseButton(0))
        {
            //Move object as long as mouse button is held and the rigid body is valid
            if (selectedRb)
            {
                bottomToCenterDistance = selectedObject.GetComponent<Collider>().bounds.extents.y + pos.y;
                //TODO: Increase height to also be on top of the object.
                selectedObject.transform.position = new Vector3(pos.x, Mathf.Max(pos.y + bottomToCenterDistance, minYValue), pos.z);
            }
            yield return waitForFixedUpdate;
        }

        if (selectedRb != null)
        {
            selectedRb.freezeRotation = false;
        }
        selectedObject.transform.position = new Vector3(selectedObject.transform.position.x, pos.y + bottomToCenterDistance, selectedObject.transform.position.z);
    }

    void HandleRotationInput()
    {
        if (Input.GetMouseButton(1)) // 使用右键进行旋转
        {
            selectedRb.constraints = RigidbodyConstraints.FreezePosition;

            float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
            float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;

            selectedObject.transform.Rotate(Vector3.up, -mouseX, Space.World);
            selectedObject.transform.Rotate(Vector3.right, mouseY, Space.World);
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
        if (selectedObject != null)
        {
            selectedRb.useGravity = true;
            selectedRb.velocity = Vector3.zero;
            Physics.IgnoreCollision(selectedObject.GetComponent<Collider>(), sandbox.GetComponent<Collider>(), false);
            selectedObject.transform.position = new Vector3(0f, selectedObject.GetComponent<Renderer>().bounds.extents.y, 0f);
            selectedObject.transform.rotation = Quaternion.identity;
            selectedObject.transform.localScale = Vector3.one;
        }
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
        if (selectedObject != null)
        {
            selectedRb.useGravity = false;
            Physics.IgnoreCollision(selectedObject.GetComponent<Collider>(), sandbox.GetComponent<Collider>());
            selectedObject.transform.position = new Vector3(selectedObject.transform.position.x, selectedObject.transform.position.y - buryDepth, selectedObject.transform.position.z);
        }
    }
}
