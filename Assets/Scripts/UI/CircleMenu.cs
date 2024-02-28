using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleMenu : MonoBehaviour
{
    private GameObject selectedObject;
    public Transform actionMenu;

    // Start is called before the first frame update
    void Start()
    {
        actionMenu = gameObject.transform.GetChild(0);
    }

    // Update is called once per frame
    void Update()
    {
        if (InteractionManager.Instance.GetSelectedObjects().Count == 0)
        {
            actionMenu.gameObject.SetActive(false);
            return;
        }
        if (Input.GetMouseButtonDown(1) && InteractionManager.Instance.IsHoveringObject())
        {
            actionMenu.gameObject.SetActive(!actionMenu.gameObject.activeSelf);
        }
        //TODO: This creates a lot of errors...
        selectedObject = InteractionManager.Instance.GetSelectedObjects()[0];
        if (!selectedObject)
        {
            InteractionManager.Instance.RemoveObject(selectedObject);
            return;
        }
        Vector3 screenPos = Camera.main.WorldToScreenPoint(selectedObject.gameObject.GetComponent<Collider>().bounds.center);

        actionMenu.transform.position = screenPos;
        actionMenu.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
    }
}
