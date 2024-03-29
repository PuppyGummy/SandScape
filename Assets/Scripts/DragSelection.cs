using UnityEngine;
using UnityEngine.EventSystems;

public class DragSelection : MonoBehaviour
{
    [SerializeField] private RectTransform selectionBox;
    private Vector2 startPos;
    private Vector2 endPos;
    private bool isDragSelecting = false;

    void Start()
    {
        startPos = Vector2.zero;
        endPos = Vector2.zero;
        selectionBox.sizeDelta = Vector2.zero;
    }

    void Update()
    {
        if (!GizmoController.Instance.enabled)
            return;
        if (!EventSystem.current.IsPointerOverGameObject() && !InteractionManager.Instance.IsDragging() && !GizmoController.Instance.IsHoveringGizmo() && !InteractionManager.Instance.isDraggingSpawnedObject && !InteractionManager.Instance.IsHoveringObject())
        {
            if (Input.GetMouseButtonDown(0))
            {
                isDragSelecting = true;
                startPos = Input.mousePosition;
                selectionBox.gameObject.SetActive(true);
            }
            if (Input.GetMouseButton(0))
            {
                endPos = Input.mousePosition;
                InteractionManager.Instance.IgnoreAllRaycasts();
                BoxSelect();
            }
        }

        if (Input.GetMouseButtonUp(0) && isDragSelecting)
        {
            isDragSelecting = false;
            InteractionManager.Instance.RecoverAllRaycasts();
            startPos = Vector2.zero;
            endPos = Vector2.zero;
            selectionBox.gameObject.SetActive(false);
            selectionBox.sizeDelta = Vector2.zero;
        }

    }
    public void BoxSelect()
    {
        Vector2 boxStart = startPos;
        Vector2 boxEnd = endPos;

        Vector2 boxCenter = (boxStart + boxEnd) / 2;
        selectionBox.position = boxCenter;
        Vector2 boxSize = new Vector2(Mathf.Abs(boxStart.x - boxEnd.x), Mathf.Abs(boxStart.y - boxEnd.y));
        selectionBox.sizeDelta = boxSize;

        Vector2 min = boxCenter - (boxSize / 2);
        Vector2 max = boxCenter + (boxSize / 2);

        foreach (GameObject obj in InteractionManager.Instance.GetObjects())
        {
            if (!obj) { InteractionManager.Instance.RemoveObject(obj); continue; }

            Vector3 screenPos = Camera.main.WorldToScreenPoint(obj.transform.position);
            if (screenPos.x > min.x && screenPos.x < max.x && screenPos.y > min.y && screenPos.y < max.y)
            {
                InteractionManager.Instance.SelectObject(obj);
            }
        }
    }
}
