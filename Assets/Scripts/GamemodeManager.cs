using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GamemodeManager : MonoBehaviour
{
    //Properties
    [SerializeField] public GameObject playerContainer;
    
    //Fields
    private bool playModeEnabled;
    private Vector3 cameraStartLocation;
    private Quaternion cameraStartRotation;
    
    //Methods
    private void Start()
    {
        //Save camera transform for later use
        if (Camera.main == null)
        {
            Debug.Log("NoCam");
            return;
        }
        
        cameraStartLocation = Camera.main.transform.position;
        cameraStartRotation = Camera.main.transform.rotation;
        Debug.Log("Saved pos!");
        Debug.Log("Position is: " + cameraStartLocation);
    }

    public void TogglePlayMode()
    {
        //Switch case used in case we need more gamemodes / states in the future
        switch (playModeEnabled)
        {
            case true:
                playerContainer.SetActive(false);
                playModeEnabled = false;
                
                ResetCameraTransform();
                
                break;
            case false:
                playerContainer.SetActive(true);
                playModeEnabled = true;

                break;
        }
    }

    private void ResetCameraTransform()
    {
        if (Camera.main == null) return;
        
        Camera.main.transform.position = cameraStartLocation;
        Camera.main.transform.rotation = cameraStartRotation;
        
        Debug.Log("Reset transform!");
        Debug.Log(Camera.main.transform.position);
        Debug.Log(cameraStartLocation);
    }
}