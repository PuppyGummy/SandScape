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
        if (!InteractionManager.Instance.GetSelectedObject())
        {
            actionMenu.gameObject.SetActive(false);
            return;
        }

        actionMenu.gameObject.SetActive(true);
        
        selectedObject = InteractionManager.Instance.GetSelectedObject();
        Vector3 screenPos = Camera.main.WorldToScreenPoint(selectedObject.gameObject.transform.position);

        actionMenu.transform.position = screenPos;
        actionMenu.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
    }
}
