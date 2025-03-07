//Source https://www.youtube.com/watch?v=f473C43s8nE&t=1s&ab_channel=Dave%2FGameDevelopment

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    private float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    public float groundDrag;

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;

    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump = true;  // Initialize to true

    [Header("Keybinds")]
    public KeyCode jumpkey = KeyCode.Space;
    public KeyCode sprintkey = KeyCode.LeftShift;
    public KeyCode crouchkey = KeyCode.LeftControl;


    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    static public bool dialogue = false;

    Vector3 moveDirection;

    Rigidbody rb;

    [Header("Gravity")]
    public float extraGravity = 10f;  // Adjust this value to change gravity intensity

    public MovementState state;
    public enum MovementState
    {
        walking,
        sprinting,
        crouching,
        air
    }

    private void StateHandler()
    {
        if (Input.GetKey(crouchkey))
        {
            state = MovementState.crouching;
            moveSpeed = crouchSpeed;
        }
        else if(grounded && Input.GetKey(sprintkey))
        {
            state = MovementState.sprinting;
            moveSpeed = sprintSpeed;
        }

        else if (grounded)
        {
            state = MovementState.walking;
            moveSpeed = walkSpeed;
        }

        else
        {
            state = MovementState.air;
        }
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        readyToJump = true;
        startYScale = transform.localScale.y;
    }

    private void Update()
    {
        if (dialogue)
        {
            // Freeze the player and unlock the cursor during dialogue
            Cursor.lockState = CursorLockMode.None; // Unlock the cursor for interaction
            Cursor.visible = true;
            return; // Skip movement-related logic
        }
        else
        {
            // Normal game controls
            Cursor.lockState = CursorLockMode.Locked; // Lock the cursor for camera control
            Cursor.visible = false;

            // Ground check using a Raycast (more reliable)
            grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f, whatIsGround);
            MyInput();
            SpeedControl();
            StateHandler();

            // Handle drag
            if (grounded)
                rb.drag = groundDrag;
            else
                rb.drag = 0;
        }
    }

    private void FixedUpdate()
    {

        if (!dialogue) 
        {
            MovePlayer();
        }
        

        // Apply additional gravity if the player is not grounded
        if (!grounded)
        {
            rb.AddForce(Vector3.down * extraGravity, ForceMode.Acceleration); // Apply extra gravity when not grounded
        }
    }

    private void MyInput() 
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // Jump logic
        if (Input.GetKey(jumpkey) && readyToJump && grounded)
        {
            Debug.Log("Player has jumped!");
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }
        // start crouch
        if (Input.GetKeyDown(crouchkey))
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }

        // stop crouch
        if (Input.GetKeyUp(crouchkey))
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        }
    }

    private void MovePlayer() 
    {
        // Calculate movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // Apply force based on whether grounded or in air
        if (grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed, ForceMode.VelocityChange);
        else
            rb.AddForce(moveDirection.normalized * moveSpeed * airMultiplier, ForceMode.VelocityChange);
    }

    private void SpeedControl() 
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        // Limit velocity if needed
        if (flatVel.magnitude > moveSpeed) 
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

    private void Jump()
    {
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);  // Reset Y velocity to ensure jump force is applied correctly
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump ()
    {
        readyToJump = true;
    }
}
