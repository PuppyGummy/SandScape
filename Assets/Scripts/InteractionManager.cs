using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;


public class InteractionManager : MonoBehaviour
{
    #region Fields

    private GameObject selectedObject;

    private RaycastHit hit;

    /// <summary>
    /// 3D coordinate of the cursors position in the world
    /// </summary>
    private Vector3 pos;

    private float bottomToCenterDistance;
    private bool useGizmo = false;
    // private List<GameObject> objs;
    private Vector3 cameraPos;
    private Vector3 cameraRotation;

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
    public float scaleSpeed = 0.1f;
    public bool selectMode = true;
    public GameObject objectIndicator;
    public GameObject playerObject;

    #endregion

    private WaitForFixedUpdate waitForFixedUpdate = new WaitForFixedUpdate();

    public static InteractionManager Instance;
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
    void Start()
    {
        cameraPos = Camera.main.transform.position;
        cameraRotation = Camera.main.transform.rotation.eulerAngles;
    }

    void Update()
    {
        if (!selectMode)
            return;

        HandleSelectionInput();
        if (selectedObject && !useGizmo)
        {
            HandleRotationInput();
            HandleScaleInput();
        }
        // if (selectedObject && Input.GetMouseButton(0) && !useGizmo)
        // {
        //     StartCoroutine(DragObject(selectedObject));
        // }
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetCamera();
        }
    }
    void FixedUpdate()
    {
        //Set the world position based on the mouse position in screen space
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        //NOTE: Ray origin changes to center of object when hovering over an object???
        //The above mentioned may be a non issue

        if (Physics.Raycast(ray, out hit, Mathf.Infinity)) //Removed the layer mask so it reacts to other objects
        {
            pos = hit.point;

            // Debug.DrawLine(ray.origin, hit.transform.position, Color.red, 1.0f); //Debug ray pos
            // Debug.Log("Distance: " + hit.distance);
        }
    }

    /// <summary>
    /// Handles selection of objects, and starts dragging the object if held
    /// </summary>
    // TODO: Stop lifting the object when selecting it?
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
                        DeselectObject();
                    }
                }
                selectedObject = hitObject;

                Outline outline = selectedObject.GetComponent<Outline>();
                if (outline)
                {
                    outline.enabled = true;
                }
                selectedObject.TryGetComponent(out selectedRb);

                //Set player if selected object is a player
                if (selectedObject.gameObject.GetComponent<PlayerMovementController>())
                {
                    playerObject = selectedObject;
                }

                if (!useGizmo)
                    StartCoroutine(DragObject(selectedObject));
                selectedObject.layer = LayerMask.NameToLayer("Ignore Raycast");
            }
            else
            {
                DeselectObject();
                if (!selectedObject) return;
                if (GizmoController.Instance.IsHoveringGizmo()) return;

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

    public void DeselectObject()
    {
        if (!selectedObject) return;

        Outline outline = selectedObject.GetComponent<Outline>();

        if (!outline) return;

        outline.enabled = false;
        selectedObject.layer = LayerMask.NameToLayer("Objects");
        selectedObject = null;
        playerObject = null;

        objectIndicator.gameObject.SetActive(false);
    }

    private IEnumerator DragObject(GameObject selectedObject)
    {
        objectIndicator.SetActive(true);

        //Freeze the object and change its settings, as to allow for smoother dragging
        if (useGizmo) yield return null;
        if (selectedRb)
        {
            selectedRb.freezeRotation = true;
            selectedRb.useGravity = true;
            selectedRb.isKinematic = false;
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
                objectIndicator.transform.position = pos;
            }
            yield return waitForFixedUpdate;
        }

        if (!selectedRb) yield break;

        //Reset velocity to prevent it from smashing down too hard
        selectedRb.velocity = Vector3.zero;
        objectIndicator.gameObject.SetActive(false);
        UnlockRotation();
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
        else if (!Input.GetMouseButtonUp(1))
        {
            UnlockRotation();
        }
    }

    void HandleScaleInput()
    {
        float scrollWheel = Input.GetAxis("Mouse ScrollWheel");

        selectedObject.transform.localScale += new Vector3(scrollWheel, scrollWheel, scrollWheel) * scaleSpeed;
    }
    public void SpawnObject(GameObject associatedObject)
    {
        GameObject spawnedObject = new GameObject();
        if (Physics.Raycast(Vector3.zero, Vector3.up, out RaycastHit hit, Mathf.Infinity))
        {
            GameObject hitObject = hit.collider.gameObject;

            spawnedObject = Instantiate(associatedObject, new Vector3(0f, associatedObject.GetComponent<Renderer>().bounds.extents.y + hitObject.GetComponent<Renderer>().bounds.size.y, 0f), transform.rotation);
        }
        else
        {
            spawnedObject = Instantiate(associatedObject, new Vector3(0f, associatedObject.GetComponent<Renderer>().bounds.extents.y, 0f), transform.rotation);
        }
        if (selectedObject)
        {
            Outline priorOutline = selectedObject.GetComponent<Outline>();
            if (priorOutline)
            {
                DeselectObject();
            }
        }
        selectedObject = spawnedObject;

        Outline outline = selectedObject.GetComponent<Outline>();
        if (outline)
        {
            outline.enabled = true;
        }
        selectedObject.TryGetComponent(out selectedRb);

        //Set player if selected object is a player
        if (selectedObject.gameObject.GetComponent<PlayerMovementController>())
        {
            playerObject = selectedObject;
        }
        selectedObject.layer = LayerMask.NameToLayer("Objects");
    }
    public void Reset()
    {
        if (!selectedObject) return;

        selectedRb.useGravity = true;
        selectedRb.velocity = Vector3.zero;

        UnlockRotation();

        selectedRb.isKinematic = false;
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
            GizmoController.Instance.EnableWorkGizmo(false);
        }
    }
    public void Bury()
    {
        if (selectedObject == null) return;
        if (selectedObject.GetComponent<ObjectController>().IsOnGround() == false) return;

        selectedRb.velocity = Vector3.zero;
        selectedRb.freezeRotation = true;
        selectedRb.constraints = RigidbodyConstraints.FreezePosition;
        selectedRb.useGravity = false;
        selectedRb.isKinematic = true;

        Physics.IgnoreCollision(selectedObject.GetComponent<Collider>(), sandbox.GetComponent<Collider>());
        selectedObject.transform.position = new Vector3(selectedObject.transform.position.x, selectedObject.transform.position.y - buryDepth, selectedObject.transform.position.z);
    }

    private void UnlockRotation()
    {
        selectedRb.constraints = RigidbodyConstraints.None;

        if (!selectedObject.GetComponent<ObjectController>().lockRotation) return;

        selectedRb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }
    public void SetUseGizmo()
    {
        useGizmo = !useGizmo;
        if (useGizmo && !selectedObject) return;
        GizmoController.Instance.EnableWorkGizmo(useGizmo);
    }
    public bool GetUseGizmo()
    {
        return useGizmo;
    }
    public GameObject GetSelectedObject()
    {
        return selectedObject;
    }
    private void ResetCamera()
    {
        Camera.main.transform.position = cameraPos;
        Camera.main.transform.rotation = Quaternion.Euler(cameraRotation);
        Camera.main.orthographic = false;
    }
}