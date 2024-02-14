using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class InteractionManager : MonoBehaviour
{
    #region Fields

    private RaycastHit hit;

    /// <summary>
    /// 3D coordinate of the cursors position in the world
    /// </summary>
    private Vector3 pos;

    private bool useGizmo = false;
    public List<GameObject> objs;
    public List<GameObject> selectedObjects;
    /// <summary>
    /// Rigidbody of the selected object
    /// </summary>
    private List<Rigidbody> selectedRbs;
    private List<GameObject> indicators;
    private Vector3 cameraPos;
    private Vector3 cameraRotation;

    private bool isDragging = false;
    private bool isHoveringObject = false;
    private bool enablePhysics = true;

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
    public float maxScaleSize = 5.0f;
    public float minScaleSize = 0.1f;

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
        objs = new List<GameObject>();
        selectedObjects = new List<GameObject>();
        selectedRbs = new List<Rigidbody>();
        indicators = new List<GameObject>();
        sandbox = GameObject.Find("Sandbox");
    }

    void Update()
    {
        if (!selectMode)
            return;

        HandleSelectionInput();
        if (selectedObjects.Count != 0 && !useGizmo)
        {
            HandleRotationInput();
            HandleScaleInput();
        }
        if (Input.GetMouseButton(0) && isHoveringObject && selectedObjects.Count != 0 && !EventSystem.current.IsPointerOverGameObject())
        {
            isDragging = true;
        }
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }
        if (isDragging && !Input.GetKey(KeyCode.LeftShift) && !useGizmo)
        {
            StartCoroutine(DragMultiObjects(selectedObjects));
            foreach (GameObject obj in selectedObjects)
            {
                obj.layer = LayerMask.NameToLayer("Ignore Raycast");
            }
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
            if (hit.collider.CompareTag("Interactable"))
            {
                isHoveringObject = true;
            }
            else
            {
                isHoveringObject = false;
            }
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
        //If the mouse is over a UI element, return
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
        if (Input.GetMouseButtonDown(0))
        {
            GameObject hitObject = GetHitObject();

            if (hitObject != null && hitObject.CompareTag("Interactable"))
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    //multi select
                    if (selectedObjects.Contains(hitObject))
                    {
                        DeselectObject(hitObject);
                        selectedObjects.Remove(hitObject);
                        selectedRbs.Remove(hitObject.GetComponent<Rigidbody>());
                    }
                    else
                    {
                        SelectObject(hitObject);
                    }
                    GizmoController.Instance.OnSelectionChanged();
                }
                else
                {
                    //single select
                    if (!selectedObjects.Contains(hitObject))
                    {
                        DeselectAllObjects();
                        SelectObject(hitObject);
                        GizmoController.Instance.OnSelectionChanged();
                    }
                }
            }
            else
            {
                DeselectAllObjects();
                GizmoController.Instance.OnSelectionChanged();
            }
        }
        //If we release mouse button and there is a selected object
        else if (Input.GetMouseButtonUp(0) && selectedObjects.Count != 0)
        {
            foreach (GameObject obj in selectedObjects)
            {
                if (obj)
                    obj.layer = LayerMask.NameToLayer("Objects");
            }
        }
    }
    public GameObject GetHitObject()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit raycastHit;

        if (!Physics.Raycast(ray, out raycastHit)) return null;
        return raycastHit.collider.gameObject;
    }
    public void SelectObject(GameObject objectToSelect)
    {
        if (selectedObjects.Contains(objectToSelect)) return;
        selectedObjects.Add(objectToSelect);

        Outline outline = objectToSelect.GetComponent<Outline>();
        if (outline)
        {
            outline.enabled = true;
        }
        else
        {
            objectToSelect.AddComponent<Outline>();
        }

        //Set player if selected object is a player
        if (objectToSelect.gameObject.GetComponent<PlayerMovementController>())
        {
            playerObject = objectToSelect;
        }
        if (useGizmo)
        {
            GizmoController.Instance.EnableWorkGizmo(true);
        }
    }

    public void DeselectObject(GameObject obj)
    {
        //if the object is not selected, return
        if (!selectedObjects.Contains(obj)) return;
        //if is dragging gizmo, return
        if (GizmoController.Instance.IsHoveringGizmo()) return;

        Outline outline = obj.GetComponent<Outline>();

        if (!outline) return;

        outline.enabled = false;
        obj.layer = LayerMask.NameToLayer("Objects");
        playerObject = null;
    }
    public void DeselectAllObjects()
    {
        if (selectedObjects.Count == 0) return;
        if (GizmoController.Instance.IsHoveringGizmo()) return;

        foreach (GameObject obj in selectedObjects)
        {
            DeselectObject(obj);
        }
        selectedObjects.Clear();
        selectedRbs.Clear();
    }
    IEnumerator DragMultiObjects(List<GameObject> objectsToDrag)
    {
        List<Vector3> relativeOffsets = new List<Vector3>();

        foreach (GameObject objectToDrag in objectsToDrag)
        {
            Rigidbody selectedRb;
            objectToDrag.TryGetComponent(out selectedRb);

            if (selectedRb)
            {
                if (enablePhysics)
                {
                    selectedRb.freezeRotation = true;
                    selectedRb.useGravity = true;
                    selectedRb.isKinematic = false;
                    Physics.IgnoreCollision(objectToDrag.GetComponent<Collider>(), sandbox.GetComponent<Collider>(), false);
                }

                selectedRbs.Add(selectedRb);
                Vector3 relativeOffset = selectedRb.transform.position - pos;
                relativeOffsets.Add(relativeOffset);
            }
        }

        while (Input.GetMouseButton(0))
        {
            for (int i = 0; i < objectsToDrag.Count; i++)
            {
                Rigidbody rb = objectsToDrag[i].GetComponent<Rigidbody>();
                if (rb)
                {
                    // Vector3 originalPosition = rb.transform.position;
                    float bottomToCenterDistance = rb.GetComponent<Collider>().bounds.extents.y + pos.y;

                    rb.transform.position = new Vector3(pos.x + relativeOffsets[i].x, Mathf.Max(pos.y + bottomToCenterDistance, minYValue), pos.z + relativeOffsets[i].z);
                    // Action moveAction = new Action(rb.transform, originalPosition, rb.transform.position);
                    // HistoryManager.Instance.RecordAction(moveAction);
                    GameObject indicator = indicators[i];
                    indicator.SetActive(true);
                    //cast a ray down the object and place the object indicator at the hit point
                    if (Physics.Raycast(rb.transform.position, Vector3.down, out hit, Mathf.Infinity))
                    {
                        indicator.transform.position = hit.point;
                        //lock the rotation of the indicator
                        indicator.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                    }
                }
            }
            yield return waitForFixedUpdate;
        }

        indicators.ForEach(indicator => indicator.SetActive(false));

        UnlockRotation();
    }

    void HandleRotationInput()
    {
        //Only rotate object if the right mouse button is held
        if (Input.GetMouseButton(1))
        {
            UnlockRotation();
            float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
            float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;

            foreach (GameObject obj in selectedObjects)
            {
                // Quaternion originalRotation = obj.transform.rotation;
                Rigidbody selectedRb;
                obj.TryGetComponent(out selectedRb);
                if (selectedRb)
                {
                    selectedRb.constraints = RigidbodyConstraints.FreezeRotation;
                }

                obj.transform.Rotate(Vector3.up, -mouseX, Space.World);
                obj.transform.Rotate(Vector3.right, mouseY, Space.World);
                // Quaternion newRotation = obj.transform.rotation;
                // Action rotateAction = new Action(obj.transform, originalRotation, newRotation);
                // HistoryManager.Instance.RecordAction(rotateAction);
            }
        }
    }

    void HandleScaleInput()
    {
        float scrollWheel = Input.GetAxis("Mouse ScrollWheel");
        if (scrollWheel == 0) return;
        foreach (GameObject obj in selectedObjects)
        {
            //Vector3 clamp doesn't have a min value...
            //Im gonna KMS... After the project is finished :^)
            // Vector3 originalScale = obj.transform.localScale;
            Vector3 newScale = new Vector3(scrollWheel, scrollWheel, scrollWheel) * scaleSpeed;
            Vector3 totalScale = obj.transform.localScale + newScale;

            if (totalScale.magnitude < minScaleSize || totalScale.magnitude > maxScaleSize)
                return;

            obj.transform.localScale += newScale;
            // Action scaleAction = new Action(obj.transform, obj.transform.position, obj.transform.position, originalScale, obj.transform.localScale);
            // HistoryManager.Instance.RecordAction(scaleAction);
        }
    }
    public void SpawnObject(GameObject associatedObject)
    {
        GameObject spawnedObject;
        if (Physics.Raycast(Vector3.zero, Vector3.up, out RaycastHit hit, Mathf.Infinity))
        {
            GameObject hitObject = hit.collider.gameObject;

            spawnedObject = Instantiate(associatedObject, new Vector3(0f, associatedObject.GetComponent<Renderer>().bounds.extents.y + hitObject.GetComponent<Renderer>().bounds.size.y, 0f), transform.rotation);
        }
        else
        {
            spawnedObject = Instantiate(associatedObject, new Vector3(0f, associatedObject.GetComponent<Renderer>().bounds.extents.y, 0f), transform.rotation);
        }
        DeselectAllObjects();
        SelectObject(spawnedObject);
        if (!enablePhysics || useGizmo)
        {
            DisablePhysics(spawnedObject);
        }
        spawnedObject.layer = LayerMask.NameToLayer("Objects");
        GameObject indicator = Instantiate(objectIndicator, Vector3.zero, Quaternion.identity);
        indicators.Add(indicator);
        indicator.transform.SetParent(spawnedObject.transform);
        indicator.SetActive(false);
    }

    public void Reset()
    {
        //you can only reset if there is one object selected
        if (selectedObjects.Count != 1) return;
        // Vector3 originalPosition = selectedObjects[0].transform.position;
        // Quaternion originalRotation = selectedObjects[0].transform.rotation;
        // Vector3 originalScale = selectedObjects[0].transform.localScale;

        Rigidbody rb = selectedObjects[0].GetComponent<Rigidbody>();
        if (enablePhysics)
        {
            rb.useGravity = true;
            rb.isKinematic = false;
            Physics.IgnoreCollision(selectedObjects[0].GetComponent<Collider>(), sandbox.GetComponent<Collider>(), false);
        }
        rb.velocity = Vector3.zero;

        UnlockRotation();

        selectedObjects[0].transform.position = new Vector3(0f, selectedObjects[0].GetComponent<Renderer>().bounds.extents.y, 0f);
        selectedObjects[0].transform.rotation = Quaternion.identity;
        selectedObjects[0].transform.localScale = Vector3.one;
        // Action action = new Action(selectedObjects[0].transform, originalPosition, selectedObjects[0].transform.position, originalRotation, selectedObjects[0].transform.rotation, originalScale, selectedObjects[0].transform.localScale);
        // HistoryManager.Instance.RecordAction(action);
    }
    public void Delete()
    {
        if (selectedObjects.Count != 0)
        {
            for (int i = 0; i < selectedObjects.Count; i++)
            {
                objs.Remove(selectedObjects[i]);
                Destroy(selectedObjects[i]);
                indicators.Remove(indicators[i]);
            }
            selectedObjects.Clear();

            GizmoController.Instance.EnableWorkGizmo(false);
        }
    }

    public void ClearAll()
    {
        foreach (var miniature in objs)
        {
            Destroy(miniature);
        }

        objs.Clear();
    }

    public void Bury()
    {
        if (selectedObjects.Count == 0) return;
        foreach (GameObject obj in selectedObjects)
        {
            // Vector3 originalPosition = obj.transform.position;
            if (obj.GetComponent<ObjectController>().IsOnGround() == false) return;
            Rigidbody rb = obj.GetComponent<Rigidbody>();
            rb.velocity = Vector3.zero;
            rb.freezeRotation = true;
            rb.constraints = RigidbodyConstraints.FreezePosition;
            rb.useGravity = false;
            rb.isKinematic = true;

            Physics.IgnoreCollision(obj.GetComponent<Collider>(), sandbox.GetComponent<Collider>());
            obj.transform.position = new Vector3(obj.transform.position.x, obj.transform.position.y - buryDepth, obj.transform.position.z);
            // Vector3 newPosition = obj.transform.position;
            // Action buryAction = new Action(obj.transform, originalPosition, newPosition);
            // HistoryManager.Instance.RecordAction(buryAction);
        }
    }

    private void UnlockRotation()
    {
        foreach (GameObject obj in selectedObjects)
        {
            Rigidbody rb;
            obj.TryGetComponent(out rb);
            if (rb)
            {
                rb.constraints = RigidbodyConstraints.None;
                if (!obj.GetComponent<ObjectController>().lockRotation) return;
                obj.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            }
        }
    }
    public void SetUseGizmo()
    {
        useGizmo = !useGizmo;
        if (useGizmo && selectedObjects.Count == 0) return;
        GizmoController.Instance.EnableWorkGizmo(useGizmo);
        if (useGizmo)
            foreach (GameObject obj in selectedObjects)
            {
                DisablePhysics(obj);
            }
        else
            foreach (GameObject obj in selectedObjects)
            {
                EnablePhysics(obj);
            }
    }
    public bool GetUseGizmo()
    {
        return useGizmo;
    }
    public List<GameObject> GetSelectedObjects()
    {
        return selectedObjects;
    }
    //TODO: Maybe move this to sceneCameraController? Could make sense in the grand scheme
    public void ResetCamera()
    {
        Camera.main.transform.position = cameraPos;
        Camera.main.transform.rotation = Quaternion.Euler(cameraRotation);
        Camera.main.orthographic = false;
    }

    //Called in ObjectController.Start(), as it has to be the concrete game object, and not a reference to the prefab that is added to the list.
    //If associated object is used, it throws an exception
    public void AddObject(GameObject objectToAdd)
    {
        objs.Add(objectToAdd);
    }
    public void RemoveObject(GameObject objectToRemove)
    {
        objs.Remove(objectToRemove);
        selectedObjects.Remove(objectToRemove);
    }
    public List<GameObject> GetObjects()
    {
        return objs;
    }
    public bool IsDragging()
    {
        return isDragging;
    }
    public void IgnoreAllRaycasts()
    {
        foreach (GameObject obj in objs)
        {
            obj.layer = LayerMask.NameToLayer("Ignore Raycast");
        }
    }
    public void RecoverAllRaycasts()
    {
        foreach (GameObject obj in objs)
        {
            obj.layer = LayerMask.NameToLayer("Objects");
        }
    }
    private void DisablePhysics(GameObject obj)
    {
        obj.GetComponent<Rigidbody>().isKinematic = true;
        obj.GetComponent<Rigidbody>().useGravity = false;
        obj.GetComponent<Collider>().isTrigger = true;
    }
    private void DisableAllPhysics()
    {
        foreach (GameObject obj in objs)
        {
            obj.GetComponent<Rigidbody>().isKinematic = true;
            obj.GetComponent<Rigidbody>().useGravity = false;
            obj.GetComponent<Collider>().isTrigger = true;
        }
    }
    public void EnablePhysics(GameObject obj)
    {
        obj.GetComponent<Collider>().isTrigger = false;
        //if the object is above ground
        if (!IsIntersecting(obj, sandbox))
        {
            obj.GetComponent<Rigidbody>().isKinematic = false;
            obj.GetComponent<Rigidbody>().useGravity = true;
            Physics.IgnoreCollision(obj.GetComponent<Collider>(), sandbox.GetComponent<Collider>(), false);
        }
    }
    private void EnableAllPhysics()
    {
        foreach (GameObject obj in objs)
        {
            obj.GetComponent<Collider>().isTrigger = false;
            //if the object is above ground
            if (!IsIntersecting(obj, sandbox))
            {
                obj.GetComponent<Rigidbody>().isKinematic = false;
                obj.GetComponent<Rigidbody>().useGravity = true;
                Physics.IgnoreCollision(obj.GetComponent<Collider>(), sandbox.GetComponent<Collider>(), false);
            }
        }
    }
    public void SetAllPhysics()
    {
        enablePhysics = !enablePhysics;
        if (objs.Count == 0) return;
        if (enablePhysics)
        {
            EnableAllPhysics();
        }
        else
        {
            DisableAllPhysics();
        }
    }
    public bool IsIntersecting(GameObject obj1, GameObject obj2)
    {
        Bounds obj1Bounds = obj1.GetComponent<Collider>().bounds;
        Bounds obj2Bounds = obj2.GetComponent<Collider>().bounds;
        if (obj1Bounds.Intersects(obj2Bounds))
        {
            return true;
        }
        return false;
    }
    public bool GetEnablePhysics()
    {
        return enablePhysics;
    }
    public GameObject GetSandbox()
    {
        return sandbox;
    }
    // public void Undo()
    // {
    //     HistoryManager.Instance.Undo();
    // }
    // public void Redo()
    // {
    //     HistoryManager.Instance.Redo();
    // }
}