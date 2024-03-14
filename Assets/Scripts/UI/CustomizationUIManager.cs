using System.Collections;
using System.Collections.Generic;
using RTG;
using UnityEngine;

public class CustomizationUIManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
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
}
