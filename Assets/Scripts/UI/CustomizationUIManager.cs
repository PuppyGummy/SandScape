using System.Collections;
using System.Collections.Generic;
using RTG;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class CustomizationUIManager : MonoBehaviour
{
    [SerializeField] private GameObject colorOptions;
    [SerializeField] private GameObject colorUIPrefab;
    
    // Start is called before the first frame update
    void Start()
    {
        SetupColorOptions();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeExpression(int id)
    {
        CustomizationItemManager.Instance.selectedObject.SetFacialExpression(id);
    }
    
    public void ChangeShape(int id)
    {
        CustomizationItemManager.Instance.selectedObject.SetBodyShape(id);
    }
    
    public void ChangeHair(int id)
    {
        CustomizationItemManager.Instance.selectedObject.SetHair(id);
    }

    public void ChangeTop(int id)
    {
        CustomizationItemManager.Instance.selectedObject.SetTop(id);
    }
    
    public void ChangeBottom(int id)
    {
        CustomizationItemManager.Instance.selectedObject.SetBottom(id);
    }
    
    public void ChangeShoe(int id)
    {
        CustomizationItemManager.Instance.selectedObject.SetShoe(id);
    }

    public void FocusOnModel()
    {
        RTFocusCamera.Get.Focus(InteractionManager.Instance.selectedObjects);
    }

    private void SetupColorOptions()
    {
        foreach (var color in CustomizationItemManager.Instance.colorOptions)
        {
            GameObject colorUIObject = PrefabUtility.InstantiatePrefab(colorUIPrefab).GameObject();
            colorUIObject.GetComponent<RectTransform>().SetParent(colorOptions.GetComponent<RectTransform>());
            colorUIObject.GetComponent<UI_Color>().material = color;
        }
    }
}
