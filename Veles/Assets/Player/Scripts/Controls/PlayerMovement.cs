using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private PlayerControls _playerControls;
    [SerializeField] private PlayerAnimator playerAnimator;

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float speed = 10f;
    [SerializeField] private float jumpForce = 10f;

    private float jumpTimer = 0f;
    [SerializeField] private float maxJumpTime = 1f;
    [SerializeField] private bool canJump = true;
    public bool CanJump => canJump;

    // [Header("Sprites")] [SerializeField] private SpriteRenderer renderer;
    // [SerializeField] private Sprite rightSprite;

    // [SerializeField] private Sprite leftSprite;


    [SerializeField] private float maxVelocity;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 15f;
    [SerializeField] private float airAcceleration = 5f;
    [SerializeField] private float airDeceleration = 5f;


    private PlayerControls.BaseActionMapActions actionMap;

    //DEBUG: DEBUGGING STUFF HERE

    [Header("*For debugging*")] [SerializeField]
    private float velX;

    [SerializeField] private Vector2 lastDirection;
    public Vector2 LastDirection => lastDirection;

    [SerializeField] private bool isMoving;
    public bool IsMoving => isMoving;

    [SerializeField] private Vector2 currentVelocity;
    // Start is called before the first frame update
    private void Awake()
    {
        _playerControls = new PlayerControls();
        actionMap = _playerControls.BaseActionMap;
    }

    enum GroundState
    {
        Grounded,
        InAir
    }

    [SerializeField] private GroundState groundState;

    private void OnEnable()
    {
        _playerControls.Enable();
    }

    private void OnDisable()
    {
        _playerControls.Disable();
    }

    void Start()
    {
        MapActions();
        rb = GetComponent<Rigidbody2D>();

        transform.parent = null;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 inputVector = actionMap.Move.ReadValue<Vector2>();

        if (actionMap.Move.IsPressed())
        {
            isMoving = true;

            //acceleration

            switch (groundState)
            {
                case GroundState.Grounded:
                    velX = inputVector.x * acceleration * Time.deltaTime;

                    break;
                case GroundState.InAir:
                    velX = inputVector.x * airAcceleration * Time.deltaTime;

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        else
        {
            velX = 0f;
        }

        rb.velocity += new Vector2(velX, 0f);

        if (math.abs(rb.velocity.x) > maxVelocity)
        {
            rb.velocity = new Vector2(maxVelocity * Mathf.Sign(rb.velocity.x), rb.velocity.y);
        }
        
        int startSign = (int)Mathf.Sign(rb.velocity.x);
        
        switch (groundState)
        {
            case GroundState.Grounded:
                rb.velocity -= Vector2.right * Mathf.Sign(rb.velocity.x) * deceleration * Time.deltaTime;
                break;
            case GroundState.InAir:
                rb.velocity -= Vector2.right * Mathf.Sign(rb.velocity.x) * airDeceleration * Time.deltaTime;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        int endSign = (int)Math.Sign(rb.velocity.x);
        
        if (startSign != endSign)
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
        }

        //DEBUG: velocity
        currentVelocity = rb.velocity;

        if (actionMap.Jump.IsPressed())
        {
            if (!canJump)
            {
                return;
            }

            groundState = GroundState.InAir;
            // TODO: add small jump && big jump
            if (jumpTimer < maxJumpTime)
            {
                rb.velocity = Vector2.right * rb.velocity.x + Vector2.up * jumpForce;
                jumpTimer += Time.deltaTime;
            }

        }
    }

    public void onGroundTouch()
    {
        canJump = true;
        groundState = GroundState.Grounded;
    }

    void Jump()
    {
    }

    void SmallJump()
    {
    }

    void MapActions()
    {
        actionMap.Jump.started += context => { jumpTimer = 0f; };
        actionMap.Jump.performed += context => { canJump = false; };
        actionMap.Jump.canceled += context => { canJump = false; };
        actionMap.Move.performed += context =>
        {
            // velX = 0f;
            Vector2 vec = actionMap.Move.ReadValue<Vector2>();

            if (vec.x == 0)
            {
                return;
            }
            else
            {
                if (vec.x > 0)
                {

                    lastDirection = Vector2.right;
                }
                else if (vec.x < 0)
                {

                    lastDirection = Vector2.left;
                }
            }
        };
    }
}