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
       
       //TODO: V Re add all assets here V

       tabController.RefreshLists();
       
       //Get all avatars from the folder
       string[] avatars = AssetDatabase.FindAssets("t:prefab", new [] { "Assets/Prefabs/Miniatures/Avatar/"});
       foreach (var avatarGUID in avatars)
       {
           var path = AssetDatabase.GUIDToAssetPath(avatarGUID);
           GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
           
           CreateMiniatureUIElement(go,0);
       }
    }
    
    public void AddSingleMiniature()
    {
        
    }

    private void CreateMiniatureUIElement(GameObject associatedObject, int categoryID)
    {
        //Spawn UI Item
        GameObject uiButton = PrefabUtility.InstantiatePrefab(miniatureUIButton).GameObject();
        
        //Setup its correct parent, so that it appears in the grid menu in its category
        uiButton.transform.parent = tabController.tabs[categoryID].transform.GetChild(1); //Child 1 is ALWAYS the grid
        
        //Modify button to work correctly
        uiButton.transform.localScale = Vector3.one;
        uiButton.GetComponent<ButtonController>().associatedObject = associatedObject;
        // uiButton.GetComponent<TextMeshPro>().text = associatedObject.gameObject.name; - Name has annoying to setup... But we may also not need it at all?
    }

    private void ClearAllMiniatures()
    {
        //TODO: Clear all
    }
}
