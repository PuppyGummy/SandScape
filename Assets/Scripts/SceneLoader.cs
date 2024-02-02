using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [Header("Scenarios")]
    public List<string> scenes;

    private string loadedScenario;
    
    /// <summary>
    /// Loads the scene based on the chosen ID.
    /// </summary>
    /// <param name="sceneID">ID representing the scene. Refer to the 'scenes' list to find the desired ID</param>
    public void LoadScene(int sceneID)
    {
        //Always unload the loaded scenario first to avoid unintended overlaps
        if (SceneManager.GetSceneByName(loadedScenario).isLoaded)
            UnloadCurrentScenario();
        
        //Load scene
        SceneManager.LoadScene(scenes[sceneID], LoadSceneMode.Additive);
        loadedScenario = scenes[sceneID];
    }

    /// <summary>
    /// Unloads the currently loaded scenario
    /// </summary>
    public void UnloadCurrentScenario()
    {
        SceneManager.UnloadSceneAsync(loadedScenario);
    }
}
