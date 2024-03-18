using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

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
    
    //Public lists for customization
    public List<GameObject> hair;
    public List<GameObject> tops;
    public List<GameObject> bottoms;
    public List<GameObject> shoes;
    public List<MeshFilter> expressions;
    public List<MeshFilter> bodyShapes;

    public List<Material> colorOptions; 

    public Customization selectedObject;
    
    public void SetSelectedObject()
    {
        var list = InteractionManager.Instance.GetSelectedObjects();
        if (list.Count > 0) selectedObject = list[0].GetComponent<Customization>();
    }

#if UNITY_EDITOR

    public void ClearList()
    {
        hair?.Clear();
        tops?.Clear();
        bottoms?.Clear();
        shoes?.Clear();
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
           
                //Add item from folder to corresponding list
                switch (folderName)
                {
                    case "Hair":
                        hair.Add(go);
                        break;
                    case "Top":
                        tops.Add(go);
                        break;
                    case "Bottom":
                        bottoms.Add(go);
                        break;
                    case "Shoe":
                        shoes.Add(go);
                        break;
                }
            }   
        }
    }
    
#endif
}

