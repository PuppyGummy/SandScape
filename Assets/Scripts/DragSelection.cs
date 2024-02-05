using UnityEngine;

public class DragSelection : MonoBehaviour
{
    [SerializeField] private RectTransform selectionBox;
    private Vector2 startPos;
    private Vector2 endPos;

    void Start()
    {
        startPos = Vector2.zero;
        endPos = Vector2.zero;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            startPos = Input.mousePosition;
            selectionBox.gameObject.SetActive(true);
        }
        if (Input.GetMouseButton(0))
        {
            endPos = Input.mousePosition;
            // DrawBox();
            BoxSelect();
        }
        if (Input.GetMouseButtonUp(0))
        {
            startPos = Vector2.zero;
            endPos = Vector2.zero;
            selectionBox.gameObject.SetActive(false);
        }

    }
    public void DrawBox()
    {
        Vector2 boxStart = startPos;
        Vector2 boxEnd = endPos;

        Vector2 boxCenter = (boxStart + boxEnd) / 2;
        selectionBox.position = boxCenter;

        Vector2 boxSize = new Vector2(Mathf.Abs(boxStart.x - boxEnd.x), Mathf.Abs(boxStart.y - boxEnd.y));
        selectionBox.sizeDelta = boxSize;
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

        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Interactable"))
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(go.transform.position);
            if (screenPos.x > min.x && screenPos.x < max.x && screenPos.y > min.y && screenPos.y < max.y)
            {
                InteractionManager.Instance.SelectObject(go);
            }
        }
    }
}
