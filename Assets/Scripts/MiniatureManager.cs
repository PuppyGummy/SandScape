using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

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

    public void RefreshAllMiniatures()
    {
       Debug.Log("Refreshed yo");
       ClearAllMiniatures();
       
       //TODO: V Re add all assets here V

       //Get all avatars from the folder
       string[] avatars = AssetDatabase.FindAssets("t:prefab", new [] { "Assets/Prefabs/Miniatures/Avatar/"});
       foreach (var avatarGUID in avatars)
       {
           var path = AssetDatabase.GUIDToAssetPath(avatarGUID);
           
       }
    }
    
    public void AddSingleMiniature()
    {
        
    }

    private void ClearAllMiniatures()
    {
        //TODO: Clear all
    }
}
