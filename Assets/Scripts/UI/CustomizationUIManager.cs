using System;
using System.Collections;
using System.Collections.Generic;
using RTG;
using UI;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class CustomizationUIManager : MonoBehaviour
{
    [SerializeField] private GameObject colorOptions;
    [SerializeField] private GameObject colorUIPrefab;
    [SerializeField] private Slider slider;
    [SerializeField] private List<UI_TabImage> expressions; //While the expressions are not tabs, this is a hack to make them toggleable
    [SerializeField] private Image currentColorDisplay;

    [SerializeField] private List<GameObject> tabButtons;
    [SerializeField] private List<GameObject> tabs;
    [SerializeField] private TabController tabController;

    [SerializeField] private List<List<GameObject>> activeTabs = new List<List<GameObject>>();

    private int colorID;
    private int bodyID;
    private int expressionID;

    // Start is called before the first frame update
    void Start()
    {
        // SetupColorOptions();
    }

    /// <summary>
    /// Method called upon opening the UI - Reads values from customization component and sets UI values accordingly
    /// </summary>
    public void Init()
    {
        // Debug.LogWarning("Started init!");

        Customization selectedObject = CustomizationItemManager.Instance.selectedObject;

        if (activeTabs.Count > 0)
        {
            //Disable all
            foreach (var tab in activeTabs)
            {
                foreach (var tabObj in tab)
                {
                    tabObj.SetActive(false);
                    // Debug.Log("Disabled tab: " + tabObj.name);
                }
            }
        }

        activeTabs.Clear();

        //Add relevant tabs to list of active tabs
        if (selectedObject.allowShapeChange)
        {
            activeTabs.Add(new List<GameObject>() { tabs[0], tabButtons[0] });
            // Debug.Log("Added shape tab");
        }

        if (selectedObject.allowStyleChange)
        {
            activeTabs.Add(new List<GameObject>() { tabs[1], tabButtons[1] });
            // Debug.Log("Added style tab");
        }

        if (selectedObject.allowColorChange)
        {
            activeTabs.Add(new List<GameObject>() { tabs[2], tabButtons[2] });
            // Debug.Log("Added color tab");
        }

        //Enable tab one
        activeTabs[0][0].SetActive(true); //Enable tab
        activeTabs[0][1].SetActive(true); //Enable tab button
        // activeTabs[0][1].gameObject.GetComponent<UI_TabImage>().SetActive(); //Set tab button as active

        for (int i = 0; i < activeTabs.Count; i++)
        {
            activeTabs[i][1].SetActive(true);
            // Debug.Log("Enabled tab: " + activeTabs[i][1].name);
        }

        // tabController.RefreshLists();
        tabController.ButtonPressed(activeTabs[0][1]); //Set tab button as active by emulating click

        expressionID = selectedObject.currentFaceID;
        bodyID = (int)selectedObject.shape;

        ClearChosenExpressions();
        expressions[expressionID].SetActive();

        slider.value = bodyID;

        FocusOnModel();

        colorID = 0;
        selectedObject.CountMaterials();
        currentColorDisplay.color = selectedObject.materials[colorID].color;
    }

    public void ChangeExpression(int id)
    {
        HistoryManager.Instance.SaveState(new List<GameObject> { CustomizationItemManager.Instance.selectedObject.gameObject }, Operation.Customize);

        CustomizationItemManager.Instance.selectedObject.SetFacialExpression(id);
        expressionID = id;

        ClearChosenExpressions();
        expressions[expressionID].SetActive();
        HistoryManager.Instance.SaveState(new List<GameObject> { CustomizationItemManager.Instance.selectedObject.gameObject }, Operation.Customize);
    }

    public void ChangeShape()
    {
        HistoryManager.Instance.SaveState(new List<GameObject> { CustomizationItemManager.Instance.selectedObject.gameObject }, Operation.Customize);
        CustomizationItemManager.Instance.selectedObject.SetBodyShape(bodyID);
        HistoryManager.Instance.SaveState(new List<GameObject> { CustomizationItemManager.Instance.selectedObject.gameObject }, Operation.Customize);
    }

    public void ShapeSliderChanged()
    {
        bodyID = (int)slider.value;
        ChangeShape();
    }

    public void IncrementShape()
    {
        bodyID++;
        bodyID = Math.Clamp(bodyID, 0, 2);
        slider.value = bodyID;

        ChangeShape();
    }

    public void DecrementShape()
    {
        bodyID--;
        bodyID = Math.Clamp(bodyID, 0, 2);
        slider.value = bodyID;

        ChangeShape();
    }

    public void ChangeHair(int id)
    {
        HistoryManager.Instance.SaveState(new List<GameObject> { CustomizationItemManager.Instance.selectedObject.gameObject }, Operation.Customize);

        CustomizationItemManager.Instance.selectedObject.SetHair(id);
        colorID = 0;
        currentColorDisplay.color = CustomizationItemManager.Instance.selectedObject.materials[colorID].color;
        HistoryManager.Instance.SaveState(new List<GameObject> { CustomizationItemManager.Instance.selectedObject.gameObject }, Operation.Customize);
    }

    public void ChangeTop(int id)
    {
        HistoryManager.Instance.SaveState(new List<GameObject> { CustomizationItemManager.Instance.selectedObject.gameObject }, Operation.Customize);

        CustomizationItemManager.Instance.selectedObject.SetTop(id);
        colorID = 0;
        currentColorDisplay.color = CustomizationItemManager.Instance.selectedObject.materials[colorID].color;
        HistoryManager.Instance.SaveState(new List<GameObject> { CustomizationItemManager.Instance.selectedObject.gameObject }, Operation.Customize);
    }

    public void ChangeBottom(int id)
    {
        HistoryManager.Instance.SaveState(new List<GameObject> { CustomizationItemManager.Instance.selectedObject.gameObject }, Operation.Customize);

        CustomizationItemManager.Instance.selectedObject.SetBottom(id);
        colorID = 0;
        currentColorDisplay.color = CustomizationItemManager.Instance.selectedObject.materials[colorID].color;
        HistoryManager.Instance.SaveState(new List<GameObject> { CustomizationItemManager.Instance.selectedObject.gameObject }, Operation.Customize);
    }

    public void ChangeShoe(int id)
    {
        HistoryManager.Instance.SaveState(new List<GameObject> { CustomizationItemManager.Instance.selectedObject.gameObject }, Operation.Customize);

        CustomizationItemManager.Instance.selectedObject.SetShoe(id);
        colorID = 0;
        currentColorDisplay.color = CustomizationItemManager.Instance.selectedObject.materials[colorID].color;
        HistoryManager.Instance.SaveState(new List<GameObject> { CustomizationItemManager.Instance.selectedObject.gameObject }, Operation.Customize);
    }

    public void ChangeColor(Material newColor)
    {
        HistoryManager.Instance.SaveState(new List<GameObject> { CustomizationItemManager.Instance.selectedObject.gameObject }, Operation.Customize);

        CustomizationItemManager.Instance.selectedObject.SetColor(colorID, newColor);

        currentColorDisplay.color = CustomizationItemManager.Instance.selectedObject.materials[colorID].color;
        HistoryManager.Instance.SaveState(new List<GameObject> { CustomizationItemManager.Instance.selectedObject.gameObject }, Operation.Customize);
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

        currentColorDisplay.color = CustomizationItemManager.Instance.selectedObject.materials[colorID].color;
    }

    public void DecrementColorID()
    {
        //Decrement
        colorID--;

        //Wrap
        if (colorID < 0)
        {
            colorID = CustomizationItemManager.Instance.selectedObject.materials.Count - 1;
        }

        currentColorDisplay.color = CustomizationItemManager.Instance.selectedObject.materials[colorID].color;
    }

    public void FocusOnModel()
    {
        RTFocusCamera.Get.Focus(InteractionManager.Instance.selectedObjects);
    }

    private void ClearChosenExpressions()
    {
        foreach (var expression in expressions)
        {
            expression.SetInactive();
        }
    }
    public void RefreshUI()
    {
        Customization selectedObject = CustomizationItemManager.Instance.selectedObject;

        if (selectedObject == null)
        {
            return;
        }
        if (selectedObject.allowStyleChange)
        {
            expressionID = selectedObject.currentFaceID;
            bodyID = (int)selectedObject.shape;

            ClearChosenExpressions();
            expressions[expressionID].SetActive();

            slider.value = bodyID;
        }

        currentColorDisplay.color = selectedObject.materials[colorID].color;
    }

#if UNITY_EDITOR
    public void SetupColorOptions()
    {
        for (int c = 0; c < colorOptions.transform.childCount; c++)
        {
            GameObject colorObject = colorOptions.transform.GetChild(c).gameObject;
            DestroyImmediate(colorObject);
        }

        foreach (var color in CustomizationItemManager.Instance.colorOptions)
        {
            GameObject colorUIObject = PrefabUtility.InstantiatePrefab(colorUIPrefab).GameObject();
            colorUIObject.GetComponent<RectTransform>().SetParent(colorOptions.GetComponent<RectTransform>());
            colorUIObject.GetComponent<UI_Color>().material = color;
            colorUIObject.GetComponent<Image>().color = color.color;
        }
    }
#endif
}
