using System.Collections;
using System.Collections.Generic;
using TMPro;
using UI;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

public class MiniatureManager : MonoBehaviour
{
    //Singleton
    private static MiniatureManager instance;

    public static MiniatureManager Instance
    {
        get
        {
            if (!instance)
            {
                instance = FindObjectOfType<MiniatureManager>();
            }
            return instance;
        }
    }

    [SerializeField] private GameObject miniatureUIButton;
    [SerializeField] private TabController tabController;

    public void RefreshAllMiniatures()
    {
       Debug.Log("Refreshed yo");
       ClearAllMiniatures();

       tabController.RefreshLists();
       
       UpdateCategory(0);
       UpdateCategory(1);
       UpdateCategory(2);
       UpdateCategory(3);
       UpdateCategory(4);
    }

    private void UpdateCategory(int categoryID)
    {
        //Set folder name
        var folderName = categoryID switch
        {
            0 => "Avatar",
            1 => "Animal",
            2 => "Nature",
            3 => "Building",
            4 => "Monster",
            _ => null
        };

        //Get all miniatures from specified folder
        string[] assets = AssetDatabase.FindAssets("t:prefab", new [] { "Assets/Prefabs/Miniatures/" + folderName + "/"});
        foreach (var assetGUID in assets)
        {
            var path = AssetDatabase.GUIDToAssetPath(assetGUID);
            GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
           
            //Create miniature UI from found miniature prefab
            CreateMiniatureUIElement(go, categoryID);
        }
    }

    private void CreateMiniatureUIElement(GameObject associatedObject, int categoryID)
    {
        //Spawn UI Item
        GameObject uiButton = PrefabUtility.InstantiatePrefab(miniatureUIButton).GameObject();
        
        //Setup its correct parent, so that it appears in the grid menu in its category
        uiButton.transform.parent = GetTabGrid(categoryID); //Child 1 is ALWAYS the grid
        
        //Modify button to work correctly
        uiButton.transform.localScale = Vector3.one;
        uiButton.GetComponent<ButtonController>().associatedObject = associatedObject;
        // uiButton.GetComponent<TextMeshPro>().text = associatedObject.gameObject.name; - Name has annoying to setup... But we may also not need it at all?
    }

    public void ClearAllMiniatures()
    {
        //List to keep track of all assets to delete
        List<GameObject> objectsToDestroy = new List<GameObject>();
        
        //For each category, get all inventory buttons and put them in the list
        for (int i = 0; i < 5; i++)
        {
            //The inventory grid
            Transform tabGrid = GetTabGrid(i);
            int children = tabGrid.childCount;

            //Add each inventory button to the list
            for (int childIndex = 0; childIndex < children; childIndex++)
            {
                objectsToDestroy.Add(tabGrid.GetChild(childIndex).gameObject);
            }
        }

        //Destroy all buttons
        foreach (var objectToDestroy in objectsToDestroy)
        {
            DestroyImmediate(objectToDestroy.gameObject);
        }
        
        //Clear list
        objectsToDestroy.Clear();
    }

    /// <summary>
    /// Simple method to grab the miniature grid, based on the desired tab.
    /// Mostly for avoiding repetitions
    /// </summary>
    /// <param name="categoryID"></param>
    /// <returns></returns>
    private Transform GetTabGrid(int categoryID)
    {
        return tabController.tabs[categoryID].transform.GetChild(1);
    }
}
