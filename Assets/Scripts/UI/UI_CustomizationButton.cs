using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_CustomizationButton : MonoBehaviour
{
    public Button customizationButton;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Disable and return, if nothing is selected
        if (InteractionManager.Instance.GetSelectedObjects().Count != 1)
        {
            customizationButton.interactable = false;
            return;
        }
        
        //If we have something selected, check for customization
        Customization customization = InteractionManager.Instance.GetSelectedObjects()[0].GetComponent<Customization>();
        if (!customization)
        {
            customizationButton.interactable = false;
            return;
        }

        //All is good, enable button!
        customizationButton.interactable = true;
    }
}
