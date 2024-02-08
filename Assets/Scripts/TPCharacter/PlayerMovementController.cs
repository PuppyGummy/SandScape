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
    public float turnSmoothTime = 0.1f;
    public float turnSmoothVelocity;
    public Vector3 direction;
    public Vector3 targetVelocity;

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
        if (horizontalInput == 0 && verticalInput == 0)
        {
            targetVelocity = Vector3.zero;
        }
    }

    void AddMovementInput()
    {

        // moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        direction = new Vector3(horizontalInput, 0.0f, verticalInput).normalized;
        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + Camera.main.transform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0.0f, angle, 0.0f);
            moveDirection = Quaternion.Euler(0.0f, targetAngle, 0.0f) * Vector3.forward;

            targetVelocity = moveDirection.normalized * speed;

            if (grounded)
                // characterController.Move(moveDirection * speed * Time.deltaTime);
                rb.velocity = new Vector3(targetVelocity.x, rb.velocity.y, targetVelocity.z);
            // rb.MovePosition(rb.position + moveDirection * speed * Time.deltaTime);
            // rb.AddForce(moveDirection.normalized * (speed * 10.0f) * Time.deltaTime, ForceMode.Force);
            else
                // rb.MovePosition(rb.position + moveDirection * speed * airMultiplier * Time.deltaTime);
                // characterController.Move(moveDirection * speed * airMultiplier * Time.deltaTime);
                rb.velocity = new Vector3(targetVelocity.x * airMultiplier, rb.velocity.y, targetVelocity.z * airMultiplier);
            // rb.AddForce(moveDirection.normalized * (speed * airMultiplier * 10.0f) * Time.deltaTime, ForceMode.Force);
        }
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
