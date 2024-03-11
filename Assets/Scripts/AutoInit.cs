using UnityEngine;

public class AutoInit : MonoBehaviour
{
    //This script & object sets the sandbox reference in the interaction manager.
    //The interaction manager needs to know what the sandbox is, but now we cannot use hard-referencing anymore, so it has to be done from here.

    public GameObject scenarioSandbox;
    public Vector3 defaultCameraPos = new Vector3(0.0f,19.4532013f,-32.7898941f);
    public Vector3 defaultCameraRot = new Vector3(38.250103f,0.0f,0.0f);
    
    //Set the sandbox to this levels sandbox
    void Start()
    {
        InteractionManager.Instance.sandbox = scenarioSandbox;

        //Setup default camera position
        Camera.main.transform.position = defaultCameraPos;
        Camera.main.transform.rotation = Quaternion.Euler(defaultCameraRot);
    }
}
