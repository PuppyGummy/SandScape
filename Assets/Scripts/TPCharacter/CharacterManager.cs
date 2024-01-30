using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;

public class CharacterManager : MonoBehaviour
{
    [Header("Components")] 
    public GameObject playerObject;
    public GameObject cameraObject;

    private PlayerMovementController playerMovementController;
    
    private bool isEnabled;

    public void Start()
    {
        playerMovementController = playerObject.GetComponent<PlayerMovementController>();
    }

    public void ToggleCharacter()
    {
        if (isEnabled)
        {
            DisableCharacter();
        }
        else
        {
            EnableCharacter();
        }
    }
    
    /// <summary>
    /// Enables ability to move, and also sets the freelook cam as main cam
    /// </summary>
    public void EnableCharacter()
    {
        if(playerMovementController == null)
            return;
        
        //Reset rotation
        playerObject.transform.rotation = new Quaternion(0.0f, playerObject.transform.rotation.y, 0.0f, 0.0f);
        playerMovementController.enabled = true;
        //Enable camera
        cameraObject.SetActive(true);
    }

    /// <summary>
    /// Disables ability to move, and also disables the freelook cam
    /// </summary>
    public void DisableCharacter()
    {
        if(playerMovementController == null)
            return;
        
        playerMovementController.enabled = false;
        cameraObject.SetActive(false);
    }
}
