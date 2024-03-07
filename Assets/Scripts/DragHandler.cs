using UnityEngine;
using UnityEngine.EventSystems;

public class DragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private GameObject objectToSpawn;
    private GameObject draggedInstance;
    private Vector3 pos;
    private CanvasGroup inventoryUI;
    private bool isDragging = false;
    private void Start()
    {
        objectToSpawn = GetComponent<ButtonController>().associatedObject;
        inventoryUI = InteractionManager.Instance.inventoryUI;
    }
    private void Update()
    {
        pos = InteractionManager.Instance.GetHitPosition();
        if (isDragging)
            UpdateObjectPosition();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        InteractionManager.Instance.isDraggingSpawnedObject = true;
        inventoryUI.alpha = 0f;
        InteractionManager.Instance.DeselectAllObjects();
        draggedInstance = InteractionManager.Instance.SpawnObject(objectToSpawn);
        draggedInstance.transform.position = GetWorldPositionOnPlane(pos);
        draggedInstance.layer = LayerMask.NameToLayer("Ignore Raycast");
        InteractionManager.Instance.DisablePhysics(draggedInstance);

    }

    public void OnDrag(PointerEventData eventData)
    {
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        InteractionManager.Instance.isDraggingSpawnedObject = false;
        isDragging = false;
        inventoryUI.alpha = 1f;

        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(eventData.position);

        if (Physics.Raycast(ray, out hit))
        {
            draggedInstance.transform.position = GetWorldPositionOnPlane(pos);
            draggedInstance.GetComponent<Rigidbody>().velocity = Vector3.zero;
            draggedInstance.transform.GetChild(0).gameObject.SetActive(false);
        }
        else
        {
            Destroy(draggedInstance); // If it doesn't hit anything, destroy it
        }
        draggedInstance.layer = LayerMask.NameToLayer("Objects");
        InteractionManager.Instance.EnablePhysics(draggedInstance);
    }

    private Vector3 GetWorldPositionOnPlane(Vector3 hitpos)
    {
        float bottomToCenterDistance = objectToSpawn.GetComponent<Collider>().bounds.extents.y + hitpos.y;
        return new Vector3(hitpos.x, Mathf.Max(hitpos.y + bottomToCenterDistance, InteractionManager.Instance.minYValue), hitpos.z);
    }
    private void UpdateObjectPosition()
    {
        RaycastHit hit;
        draggedInstance.transform.position = GetWorldPositionOnPlane(pos);
        GameObject indicator = draggedInstance.transform.GetChild(0).gameObject;
        if (indicator != null)
        {
            indicator.SetActive(true);
            //cast a ray down the object and place the object indicator at the hit point
            if (Physics.Raycast(draggedInstance.transform.position, Vector3.down, out hit, Mathf.Infinity))
            {
                indicator.transform.position = hit.point;
                //lock the rotation of the indicator
                indicator.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
            }
        }
    }
}