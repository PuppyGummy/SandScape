using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Serialization;

public class CustomizationItemManager : MonoBehaviour
{
    //Singleton
    private static CustomizationItemManager instance;

    public static CustomizationItemManager Instance
    {
        get
        {
            if (!instance)
            {
                instance = FindObjectOfType<CustomizationItemManager>();
            }
            return instance;
        }
    }

    public List<GameObject> clothingItems;

#if UNITY_EDITOR

    public void ClearList()
    {
        clothingItems?.Clear();
    }

    public void RefreshList()
    {
        ClearList();
        AddAll();
    }

    private void AddAll()
    {
        for (int i = 0; i < 4; i++)
        {
            //Set folder name
            var folderName = i switch
            {
                0 => "Hair",
                1 => "Top",
                2 => "Bottom",
                3 => "Shoe",
                _ => null
            };

            //Get all miniatures from specified folder
            string[] assets = AssetDatabase.FindAssets("t:prefab", new [] { "Assets/Prefabs/Clothes/" + folderName + "/"});
            foreach (var assetGUID in assets)
            {
                var path = AssetDatabase.GUIDToAssetPath(assetGUID);
                GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
           
                //Add item from folder
                clothingItems.Add(go);
            }   
        }
    }
    
#endif
}

