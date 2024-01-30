using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    private GameObject player;
    private Transform orientation;
    private CinemachineFreeLook cinemachineFreeLook;
    
    public float rotationSpeed;

    private bool canChangeMouseState = true;

    private void OnEnable()
    {
        if (!cinemachineFreeLook)
        {
            cinemachineFreeLook = gameObject.GetComponent<CinemachineFreeLook>();
        }
        
        player = InteractionManager.Instance.playerObject;
        orientation = player.transform.GetChild(1).transform;

        cinemachineFreeLook.LookAt = player.transform;
        cinemachineFreeLook.Follow = player.transform;
        
        // SetupCursor();
    }

    /*/// <summary>
    /// Hides cursor and limits it to the game frame.
    /// Useful for when in the 'playing' state.
    /// </summary>
    private void SetupCursor()
    {
        if (!canChangeMouseState) return;
        
        Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? CursorLockMode.Confined : CursorLockMode.Locked;
        Cursor.visible = !Cursor.visible;
    }*/

    void Update()
    {
        //Set forward rotation ot view direction
        var transformPosition = transform.position;
        var playerPosition = player.transform.position;
        Vector3 viewDirection = playerPosition - new Vector3(transformPosition.x, playerPosition.y, transformPosition.z);

        orientation.forward = viewDirection.normalized;

        //If there's any input, turn to new forward direction
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        Vector3 inputDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        
        if(inputDirection != Vector3.zero)
            player.transform.forward = Vector3.Slerp(player.transform.forward, inputDirection.normalized, Time.deltaTime * rotationSpeed);

        /*if (!Input.GetKey(KeyCode.Tab)) return;
        
        SetupCursor();
        canChangeMouseState = false;
        
        Invoke(nameof(EnableChangeMouseState), 0.25f);*/
    }

    /*void EnableChangeMouseState()
    {
        canChangeMouseState = true;
    }*/
}
