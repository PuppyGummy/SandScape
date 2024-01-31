using System;
using System.Collections;
using System.Collections.Generic;
using TPCharacter;
using UnityEngine;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

public class GamemodeManager : MonoBehaviour
{
    //Properties
    [SerializeField] public CharacterManager CharacterManager;
    
    //Fields
    private bool playModeEnabled;
    private Vector3 cameraStartLocation;
    private Quaternion cameraStartRotation;
    private Camera mainCamera;
    private static GamemodeManager instance;

    public static GamemodeManager Instance
    {
        get
        {
            if (!instance)
            {
                instance = FindObjectOfType<GamemodeManager>();
            }
            return instance;
        }
    }
    
    //Methods
    private void Start()
    {
        mainCamera = Camera.main;
        //Save camera transform for later use
        if (mainCamera == null) return;
        
        cameraStartLocation = mainCamera.transform.position;
        cameraStartRotation = mainCamera.transform.rotation;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && playModeEnabled)
        {
            DisableCharacter();
        }
    }

    public void TogglePlayMode()
    {
        //Switch case used in case we need more gamemodes / states in the future
        switch (playModeEnabled)
        {
            case true:
                DisableCharacter();

                break;
            case false:
                EnableCharacter();

                break;
        }
    }

    private void EnableCharacter()
    {
        SetupCursor();
        
        CharacterManager.EnableCharacter();
        playModeEnabled = true;

        InteractionManager.Instance.selectMode = false;
        InteractionManager.Instance.DeselectObject();
    }

    private void DisableCharacter()
    {
        SetupCursor();
        
        CharacterManager.DisableCharacter();
        playModeEnabled = false;
                
        InteractionManager.Instance.selectMode = true;

        ResetCameraTransform();
    }

    private void ResetCameraTransform()
    {
        if (!mainCamera) return;
        
        mainCamera.transform.position = cameraStartLocation;
        mainCamera.transform.rotation = cameraStartRotation;
    }
    
    /// <summary>
    /// Hides cursor and limits it to the game frame.
    /// Useful for when in the 'playing' state.
    /// </summary>
    private void SetupCursor()
    {
        Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? CursorLockMode.Confined : CursorLockMode.Locked;
        Cursor.visible = !Cursor.visible;
    }
}
