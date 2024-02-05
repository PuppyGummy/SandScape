using UnityEngine;

public class AutoInit : MonoBehaviour
{
    //This script & object sets the sandbox reference in the interaction manager.
    //The interaction manager needs to know what the sandbox is, but now we cannot use hard-referencing anymore, so it has to be done from here.

    public GameObject scenarioSandbox;
    
    //Set the sandbox to this levels sandbox
    void Start()
    {
        InteractionManager.Instance.sandbox = scenarioSandbox;
    }
}
