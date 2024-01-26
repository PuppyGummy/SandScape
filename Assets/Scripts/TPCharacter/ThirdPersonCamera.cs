using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Properties")] 
    public Transform orientation;
    public Transform player;
    public Transform playerObject;
    public Rigidbody rigidBody;

    public float rotationSpeed;

    private bool canChangeMouseState = true;
    
    private void Start()
    {
        SetupCursor();
    }

    /// <summary>
    /// Hides cursor and limits it to the game frame.
    /// Useful for when in the 'playing' state.
    /// </summary>
    private void SetupCursor()
    {
        if (!canChangeMouseState) return;
        
        Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? CursorLockMode.Confined : CursorLockMode.Locked;
        Cursor.visible = !Cursor.visible;
    }

    void Update()
    {
        //Set forward rotation ot view direction
        var transformPosition = transform.position;
        var playerPosition = player.position;
        Vector3 viewDirection = playerPosition - new Vector3(transformPosition.x, playerPosition.y, transformPosition.z);

        orientation.forward = viewDirection.normalized;

        //If there's any input, turn to new forward direction
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        Vector3 inputDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        
        if(inputDirection != Vector3.zero)
            playerObject.forward = Vector3.Slerp(playerObject.forward, inputDirection.normalized, Time.deltaTime * rotationSpeed);

        if (!Input.GetKey(KeyCode.Tab)) return;
        
        SetupCursor();
        canChangeMouseState = false;
        
        Invoke(nameof(EnableChangeMouseState), 0.25f);
    }

    void EnableChangeMouseState()
    {
        canChangeMouseState = true;
    }
}
