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
    public Rigidbody rb;

    public Transform orientation;

    private float horizontalInput;
    private float verticalInput;

    /// <summary>
    /// Determines whether or not the current player is being possessed.
    /// True = possessed, false = not possessed.
    /// </summary>
    public bool active;
    // private CharacterController characterController;

    private Vector3 moveDirection;
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        // characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        grounded = Physics.Raycast(transform.position, Vector3.down, 0.1f);
        HandleInput();
        LimitVelocity();

        //Selector for choosing drag / friction
        rb.drag = grounded ? groundDrag : 0.0f;
    }

    private void FixedUpdate()
    {
        AddMovementInput();
    }

    void HandleInput()
    {
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
        Vector3 targetVelocity = moveDirection * speed;

        if (grounded)
            // characterController.Move(moveDirection * speed * Time.deltaTime);
            rb.velocity = new Vector3(targetVelocity.x, rb.velocity.y, targetVelocity.z);
        else
            // characterController.Move(moveDirection * speed * airMultiplier * Time.deltaTime);
            rb.velocity = new Vector3(targetVelocity.x * airMultiplier, rb.velocity.y, targetVelocity.z * airMultiplier);
    }

    void LimitVelocity()
    {
        var rigidbodyVelocity = rb.velocity;
        Vector3 flatVelocity = new Vector3(rigidbodyVelocity.x, 0.0f, rigidbodyVelocity.z);

        if (flatVelocity.magnitude > speed)
        {
            Vector3 limitedVelocity = flatVelocity.normalized * speed;
            rb.velocity = new Vector3(limitedVelocity.x, rigidbodyVelocity.y, limitedVelocity.z);
        }
    }

    void Jump()
    {
        var velocity = rb.velocity;
        velocity = new Vector3(velocity.x, 0.0f, velocity.z);
        rb.velocity = velocity;

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    void ResetJump()
    {
        readyToJump = true;
    }

    //If active player is destroyed, we want to exit play mode
    private void OnDestroy()
    {
        if (!active)
            return;

        GamemodeManager.Instance.TogglePlayMode();
    }
}
