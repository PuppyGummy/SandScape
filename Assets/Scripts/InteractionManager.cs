using UnityEngine;
using System.Collections;

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
    private float bottomToCenterDistance;
    // private float distanceToGround;

    private WaitForFixedUpdate waitForFixedUpdate = new WaitForFixedUpdate();
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

    void HandleSelectionInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                GameObject hitObject = hit.collider.gameObject;

                if (hitObject.CompareTag("Interactable"))
                {
                    if (selectedObject != null)
                    {
                        Outline priorOutline = selectedObject.GetComponent<Outline>();
                        if (priorOutline != null)
                        {
                            priorOutline.enabled = false;
                            selectedObject = null;
                        }
                    }
                    selectedObject = hitObject;

                    Outline outline = selectedObject.GetComponent<Outline>();
                    if (outline != null)
                    {
                        outline.enabled = true;
                    }
                    StartCoroutine(DragObject(selectedObject));
                }
                else
                {
                    if (selectedObject != null)
                    {
                        Outline outline = selectedObject.GetComponent<Outline>();
                        if (outline != null)
                        {
                            outline.enabled = false;
                            selectedObject = null;
                        }
                    }
                }
            }
        }
    }
    private IEnumerator DragObject(GameObject selectedObject)
    {
        float distance = Vector3.Distance(selectedObject.transform.position, Camera.main.transform.position);
        selectedObject.TryGetComponent(out Rigidbody rb);

        if (rb != null)
        {
            rb.freezeRotation = true;
        }

        while (Input.GetMouseButton(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (rb != null)
            {
                float bottomToCenterDistance = selectedObject.GetComponent<Collider>().bounds.extents.y;
                Vector3 direction = ray.GetPoint(distance) - selectedObject.transform.position;
                rb.velocity = direction * dragSpeed;
                selectedObject.transform.position = new Vector3(selectedObject.transform.position.x, Mathf.Max(selectedObject.transform.position.y - bottomToCenterDistance, minYValue), selectedObject.transform.position.z);
            }
            yield return waitForFixedUpdate;
        }

        if (rb != null)
        {
            rb.freezeRotation = false;
        }
    }


    // void HandleDragInput()
    // {
    //     if (isDragging)
    //     {
    //         Debug.Log("Dragging");
    //         Vector3 mousePosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane));
    //         selectedObject.transform.position = new Vector3(mousePosition.x + offset.x, selectedObject.transform.position.y, mousePosition.z + offset.z);
    //     }

    //     if (Input.GetMouseButtonUp(0))
    //     {
    //         isDragging = false;
    //     }

    //     if (Input.GetMouseButtonDown(0))
    //     {
    //         RaycastHit hit;
    //         Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

    //         if (Physics.Raycast(ray, out hit))
    //         {
    //             if (hit.collider.CompareTag("Interactable"))
    //             {
    //                 StartDragging(selectedObject);
    //             }
    //         }
    //     }
    // }

    // void StartDragging(GameObject objectToDrag)
    // {
    //     Vector3 mousePosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane));
    //     offset = objectToDrag.transform.position - mousePosition;
    //     isDragging = true;
    // }

    // void HandleRotationInput()
    // {
    //     if (Input.GetMouseButtonDown(1))
    //     {
    //         isRotating = true;
    //         rotationOrigin = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    //     }

    //     if (Input.GetMouseButtonUp(1))
    //     {
    //         isRotating = false;
    //     }

    //     if (isRotating)
    //     {
    //         Debug.Log("Rotating");
    //         Vector3 currentMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    //         float angle = Vector2.SignedAngle(rotationOrigin, currentMousePosition);

    //         selectedObject.transform.Rotate(Vector3.forward, angle);
    //         rotationOrigin = currentMousePosition;
    //     }
    // }

    void HandleRotationInput()
    {
        if (Input.GetMouseButton(1)) // 使用右键进行旋转
        {
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
    public void Reset()
    {
        if (selectedObject != null)
        {
            selectedObject.transform.position = new Vector3(0f, 2f, 0f);
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
}
