using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    [Header("Components")] 
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
