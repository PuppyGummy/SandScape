using System.Collections.Generic;
using UI;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using Image = UnityEngine.UI.Image;

#if UNITY_EDITOR

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
    [SerializeField] private List<Sprite> watermarks;

    public void RefreshAllMiniatures()
    {
       Debug.Log("Refreshed yo");
       ClearAllMiniatures();

       tabController.RefreshLists();

       for (int i = 0; i < tabController.tabs.Count; i++)
       {
           UpdateCategory(i);
       }
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
            5 => "Furniture",
            6 => "Spiritual",
            _ => null
        };

        //Get all miniatures from specified folder
        string[] assets = AssetDatabase.FindAssets("t:prefab", new [] { "Assets/Prefabs/Miniatures/" + folderName + "/"});
        foreach (var assetGUID in assets)
        {
            var path = AssetDatabase.GUIDToAssetPath(assetGUID);
            GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
           
            //Create miniature UI from found miniature prefab
            CreateMiniatureUIElement(go, categoryID, folderName);
        }
    }

    private void CreateMiniatureUIElement(GameObject associatedObject, int categoryID, string folderName)
    {
        //Spawn UI Item
        GameObject uiButton = PrefabUtility.InstantiatePrefab(miniatureUIButton).GameObject();
        
        //Setup its correct parent, so that it appears in the grid menu in its category
        uiButton.transform.parent = GetTabGrid(categoryID); //Child 1 is ALWAYS the grid

        //Modify button to work correctly
        uiButton.transform.localScale = Vector3.one;
        uiButton.GetComponent<ButtonController>().associatedObject = associatedObject;
        
        //IMAGES
        
        //Set Watermark
        uiButton.GetComponent<Image>().sprite = watermarks[categoryID];
        
        string[] images = AssetDatabase.FindAssets("t:Texture2D", new [] { "Assets/UI_Assets/Icons/" + folderName + "/"});
        
        //Set miniature image
        foreach (var imageName in images)
        {
            Sprite miniatureThumbnail = null;
            
            var path = AssetDatabase.GUIDToAssetPath(imageName);
            Debug.Log("path: " + path);
            Debug.Log("Obje: " + associatedObject.name + ".png");

            miniatureThumbnail = path.Contains(associatedObject.name + ".png") ? AssetDatabase.LoadAssetAtPath<Sprite>(path) : null;

            if (!miniatureThumbnail)
            {
                Debug.LogWarning("Icon not set for " + associatedObject.name + "...\n" +
                                 "Maybe you named an asset wrong, or forgot to set image type to sprite??");
            }
            
            Image iconImage = uiButton.transform.GetChild(0).GetComponent<Image>();
        
            if(miniatureThumbnail)
            {
                iconImage.sprite = miniatureThumbnail;
                Debug.Log("Set Icon for: " + associatedObject.name);
            }
        }
    }

    public void ClearAllMiniatures()
    {
        //List to keep track of all assets to delete
        List<GameObject> objectsToDestroy = new List<GameObject>();
        
        //For each category, get all inventory buttons and put them in the list
        for (int i = 0; i < tabController.tabs.Count; i++)
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
        return tabController.tabs[categoryID].transform.GetChild(0);
    }
}


#endif