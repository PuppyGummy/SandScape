using RTG;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    private int loadedScenario;

    public GameObject scenePanel;
    public GameObject gameUIPanel;
    public GameObject boxSelectUI;
    public GameObject rtInput;
    public GizmoController gizmoController;
    private static SceneLoader instance;

    public static SceneLoader Instance
    {
        get
        {
            if (!instance)
            {
                instance = FindObjectOfType<SceneLoader>();
            }
            return instance;
        }
    }


    /// <summary>
    /// Loads the scene based on the chosen ID.
    /// </summary>
    /// <param name="sceneID">ID representing the scene. Refer to the 'scenes' list to find the desired ID</param>
    public void LoadScene(int sceneID)
    {
        //Always unload the loaded scenario first to avoid unintended overlaps
        if (SceneManager.GetSceneByBuildIndex(loadedScenario).isLoaded)
            UnloadCurrentScenario();

        //Load scene
        SceneManager.LoadScene(sceneID, LoadSceneMode.Additive);
        loadedScenario = sceneID;

        scenePanel.SetActive(false);
        gameUIPanel.SetActive(true);
        boxSelectUI.SetActive(true);
        rtInput.SetActive(true);
        gizmoController.enabled = true;
    }

    /// <summary>
    /// Unloads the currently loaded scenario
    /// </summary>
    public void UnloadCurrentScenario()
    {
        if (loadedScenario == 0)
            return;

        InteractionManager.Instance.ClearAll();
        SceneManager.UnloadSceneAsync(loadedScenario);

        scenePanel.SetActive(true);
        gameUIPanel.SetActive(false);
        boxSelectUI.SetActive(false);
        rtInput.SetActive(false);
        gizmoController.enabled = false;

        loadedScenario = 0;
    }
    public int GetLoadedScenario()
    {
        return loadedScenario;
    }
}
