using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerMovementController : MonoBehaviour
{
    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    
    [Header("Movement")]
    public float speed;
    public float jumpForce;
    public float jumpCoolDown;
    public float airMultiplier;
    private bool readyToJump = true;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask ground;
    public float groundDrag;
    private bool grounded;

    [Header("Properties")]
    public Rigidbody rigidbody;
    
    public Transform orientation;

    private float horizontalInput;
    private float verticalInput;

    private Vector3 moveDirection;
    private void Start()
    {
        rigidbody.freezeRotation = true;
    }

    private void Update()
    {
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, ground);
        
        HandleInput();
        LimitVelocity();

        if (grounded)
            rigidbody.drag = groundDrag;
        else
            rigidbody.drag = 0.0f;
    }

    private void FixedUpdate()
    {
        AddMovementInput();
    }

    void HandleInput()
    {
        if (Input.GetKey(KeyCode.Escape))
            Application.Quit();
        
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (!Input.GetKey(jumpKey) || !readyToJump || !grounded) return;


        readyToJump = false;
        Jump();

        Invoke(nameof(ResetJump), jumpCoolDown);
    }

    void AddMovementInput()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        
        if(grounded)
            rigidbody.AddForce(moveDirection.normalized * (speed * 10.0f), ForceMode.Force);
        else
            rigidbody.AddForce(moveDirection.normalized * (speed * 10.0f * airMultiplier), ForceMode.Force);
    }

    void LimitVelocity()
    {
        var rigidbodyVelocity = rigidbody.velocity;
        Vector3 flatVelocity = new Vector3(rigidbodyVelocity.x, 0.0f, rigidbodyVelocity.z);

        if (flatVelocity.magnitude > speed)
        {
            Vector3 limitedVelocity = flatVelocity.normalized * speed;
            rigidbody.velocity = new Vector3(limitedVelocity.x, rigidbodyVelocity.y, limitedVelocity.z);
        }
    }

    void Jump()
    {
        var velocity = rigidbody.velocity;
        velocity = new Vector3(velocity.x, 0.0f, velocity.z);
        rigidbody.velocity = velocity;
        
        rigidbody.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    void ResetJump()
    {
        readyToJump = true;
    }
}
