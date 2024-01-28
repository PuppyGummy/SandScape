using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;

public class CharacterManager : MonoBehaviour
{
    [Header("Components")] 
    public GameObject playerObject;
    public PlayerMovementController playerMovementController;
    public GameObject cameraObject;

    private bool isEnabled;
    
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
        //Reset rotation
        playerObject.transform.rotation = new Quaternion(0.0f, playerObject.transform.rotation.y, 0.0f, 0.0f);
        //Enable camera
        playerMovementController.enabled = true;
        cameraObject.SetActive(true);
    }

    /// <summary>
    /// Disables ability to move, and also disables the freelook cam
    /// </summary>
    public void DisableCharacter()
    {
        playerMovementController.enabled = false;
        cameraObject.SetActive(false);
    }
}
