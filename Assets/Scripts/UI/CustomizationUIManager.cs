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

    private int colorID;
    
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
        colorID = 0;
    }

    public void ChangeTop(int id)
    {
        CustomizationItemManager.Instance.selectedObject.SetTop(id);
        colorID = 0;
    }
    
    public void ChangeBottom(int id)
    {
        CustomizationItemManager.Instance.selectedObject.SetBottom(id);
        colorID = 0;
    }
    
    public void ChangeShoe(int id)
    {
        CustomizationItemManager.Instance.selectedObject.SetShoe(id);
        colorID = 0;
    }

    public void ChangeColor(Material newColor)
    {
        CustomizationItemManager.Instance.selectedObject.SetColor(colorID, newColor);
    }

    public void IncrementColorID()
    {
        //Increment
        colorID++;
        
        //Wrap
        if (colorID > CustomizationItemManager.Instance.selectedObject.materials.Count - 1)
        {
            colorID = 0;
        }
    }

    public void DecrementColorID()
    {
        //Decrement
        colorID--;
        
        //Wrap
        if (colorID < 0)
        {
            colorID = CustomizationItemManager.Instance.selectedObject.materials.Count + 1;
        }
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
