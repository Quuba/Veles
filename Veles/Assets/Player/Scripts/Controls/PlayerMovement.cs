using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class PlayerMovement : MonoBehaviour
{
    // public static PlayerMovement Instance;


    [Space(30)] private PlayerControls _playerControls;

    [SerializeField] private Rigidbody2D rb;

    [SerializeField] Vector2 inputVector;

    [SerializeField] private bool canReceiveInput = true;
    public bool CanReceiveInput => canReceiveInput;

    [Header("Basic movement")] [SerializeField]
    private float maxMovementSpeed;

    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 15f;
    [SerializeField] private float airAcceleration = 5f;
    [SerializeField] private float airDeceleration = 5f;

    [Header("Velocity cap")] [SerializeField]
    private bool capVelocity = false;
    
    [Header("Jumping")] [SerializeField] private float jumpForce = 10f;

    [SerializeField] private float jumpTimer = 0f;
    [SerializeField] private float maxJumpTime = 1f;
    [SerializeField] private bool canJump = true;
    public bool CanJump => canJump;

    [SerializeField] private float horizontalJumpBoost = 1f;
    [SerializeField] private Vector2 maxVelocity;

    [Header("Ground checks")] [SerializeField]
    private float groundCheckDistance = 1f;

    [SerializeField] private float currentGroundDistance;

    [SerializeField] private float wallPushForce = 1.5f;

    [SerializeField] private float wallCheckDistance = 1f;
    [SerializeField] private float minGroundDistanceForWallStick = 1f;

    private Vector2 wallTouchDir; // TODO: to Vector2Int
    [SerializeField] private LayerMask levelCollisionLayer;

    private PlayerControls.BaseActionMapActions actionMap;

    public event EventHandler groundTouch;
    public event EventHandler jumpEvent;
    public event EventHandler fallEvent;
    public event EventHandler wallStuckEvent;


    //DEBUG: DEBUGGING STUFF HERE

    [Header("*For debugging*")] [SerializeField]
    private bool DebugMode = false;

    [SerializeField] private float velX;


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
        // Instance = this;
    }

    enum GroundState
    {
        Grounded,
        InAir,
        WallStuck
    }

    enum JumpState
    {
        None,
        Rise,
        Fall
    }

    [SerializeField] private JumpState jumpState;

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
        // PlayerEventManager.Instance.GroundTouchEvent();

        MapActions();
        rb = GetComponent<Rigidbody2D>();

        transform.parent = null;
    }

    // Update is called once per frame
    void Update()
    {
        WallCheck();
        GroundCheck();

        if (canReceiveInput)
        {
            MoveCheck();

            JumpCheck();

            if (capVelocity)
            {
                if (rb.velocity.x > maxVelocity.x)
                {
                    rb.velocity = new Vector2(maxVelocity.x, rb.velocity.y);
                }

                if (rb.velocity.y > maxVelocity.y)
                {
                    rb.velocity = new Vector2(rb.velocity.x, maxVelocity.y);
                }
            }
        }
        


        if (!DebugMode) return;

        currentVelocity = rb.velocity;
    }

    void GroundCheck()
    {
        RaycastHit2D hit2D = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance,
            levelCollisionLayer.value);
        if (hit2D)
        {
            if (groundState == GroundState.InAir && jumpState != JumpState.Rise)
            {
                Debug.Log("db1");

                //First frame after landing
                if (rb.velocity.y <= 0.01f)
                {
                    OnGroundTouch();
                }
            }
        }
        else
        {
            if (groundState == GroundState.Grounded && rb.velocity.y <= 0)
            {
                OnStartFalling();
            }
            else if (groundState == GroundState.InAir && jumpState == JumpState.Rise)
            {
                if (rb.velocity.y <= 0)
                {
                    // Debug.Log("hulp");

                    OnStartFalling();
                }
            }
        }

        // Distance check
        RaycastHit2D distanceHit2D = Physics2D.Raycast(transform.position, Vector2.down, 100f,
            levelCollisionLayer.value);
        if (distanceHit2D)
        {
            currentGroundDistance = Vector2.Distance(transform.position, distanceHit2D.point);
        }

        if (!DebugMode) return;

        //Debug lines
        Debug.DrawLine(transform.position, (Vector2) transform.position + Vector2.down * groundCheckDistance,
            hit2D ? Color.green : Color.red);
    }

    void WallCheck()
    {
        RaycastHit2D leftHit2D = Physics2D.Raycast(transform.position, Vector2.left, wallCheckDistance,
            levelCollisionLayer.value);

        RaycastHit2D rightHit2D = Physics2D.Raycast(transform.position, Vector2.right, wallCheckDistance,
            levelCollisionLayer.value);

        if (leftHit2D)
        {
            wallTouchDir = Vector2.left;
            // isTouchingWall = true;
        }
        else if (rightHit2D)
        {
            wallTouchDir = Vector2.right;
            // isTouchingWall = true;
        }
        else
        {
            wallTouchDir = Vector2.zero;
            // isTouchingWall = false;
        }

        if (wallTouchDir != Vector2.zero)
        {
            OnWallTouch();
        }

        if (!DebugMode) return;

        //Debug lines
        if (wallTouchDir == Vector2.zero)
        {
            Debug.DrawLine(transform.position, transform.position + Vector3.left * wallCheckDistance, Color.red);
            Debug.DrawLine(transform.position, transform.position + Vector3.right * wallCheckDistance, Color.red);
        }
        else if (wallTouchDir == Vector2.right)
        {
            Debug.DrawLine(transform.position, transform.position + Vector3.left * wallCheckDistance, Color.red);
            Debug.DrawLine(transform.position, transform.position + Vector3.right * wallCheckDistance, Color.green);
        }
        else if (wallTouchDir == Vector2.left)
        {
            Debug.DrawLine(transform.position, transform.position + Vector3.left * wallCheckDistance, Color.green);
            Debug.DrawLine(transform.position, transform.position + Vector3.right * wallCheckDistance, Color.red);
        }
    }

    private void MoveCheck()
    {
        inputVector = actionMap.Move.ReadValue<Vector2>();

        if (groundState != GroundState.WallStuck)
        {
            //Acceleration
            if (actionMap.Move.IsPressed())
            {
                isMoving = true;

                //acceleration

                velX = groundState switch
                {
                    GroundState.Grounded => inputVector.x * acceleration * Time.deltaTime,
                    GroundState.InAir => inputVector.x * airAcceleration * Time.deltaTime,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
            else
            {
                velX = 0f;
            }


            if (math.abs(rb.velocity.x) > maxMovementSpeed)
            {
                // rb.velocity = new Vector2(maxVelocity * Mathf.Sign(rb.velocity.x), rb.velocity.y);
                velX = 0f;
            }

            rb.velocity += new Vector2(velX, 0f);


            //Deceleration
            if (rb.velocity.x != 0)
            {
                int startSign = (int) Mathf.Sign(rb.velocity.x);

                rb.velocity -= groundState switch
                {
                    GroundState.Grounded => Vector2.right * Mathf.Sign(rb.velocity.x) * deceleration * Time.deltaTime,
                    GroundState.InAir => Vector2.right * Mathf.Sign(rb.velocity.x) * airDeceleration * Time.deltaTime,
                    _ => throw new ArgumentOutOfRangeException()
                };

                int endSign = (int) Math.Sign(rb.velocity.x);

                if (startSign != endSign)
                {
                    rb.velocity = new Vector2(0f, rb.velocity.y);
                }
            }
        }
    }

    private void JumpCheck()
    {
        if (actionMap.Jump.WasPressedThisFrame() && canJump)
        {
            jumpTimer = 0.01f;

            if (groundState == GroundState.WallStuck)
            {
                // wallTime
                rb.gravityScale = 1f;
                Debug.Log("a");
                rb.velocity = Vector2.right * rb.velocity.x + Vector2.up * jumpForce +
                              Vector2.right * 2 * -wallTouchDir.x;
            }
            else
            {
                rb.velocity += Vector2.right * inputVector.x * horizontalJumpBoost;
            }

            groundState = GroundState.InAir;

            jumpEvent?.Invoke(this, EventArgs.Empty);
            jumpState = JumpState.Rise;

            canJump = false;
        }
        else if (actionMap.Jump.IsPressed() && jumpTimer > 0f && jumpTimer <= maxJumpTime)
        {
            if (canJump)
            {
                canJump = false;
            }

            rb.velocity = Vector2.right * rb.velocity.x + Vector2.up * jumpForce;
            jumpTimer += Time.deltaTime;
        }
        else if (!actionMap.Jump.IsPressed() && groundState == GroundState.InAir)
        {
            canJump = false;
            jumpTimer = 0f;
        }
    }

    private void OnGroundTouch()
    {
        groundState = GroundState.Grounded;
        groundTouch?.Invoke(this, EventArgs.Empty);
        canJump = true;
        jumpState = JumpState.None;
    }

    private void OnWallTouch()
    {
        switch (groundState)
        {
            case GroundState.InAir when inputVector.x == wallTouchDir.x && currentGroundDistance >= 1.75f:
                if (currentGroundDistance < minGroundDistanceForWallStick) return;

                // if player moving towards wall
                if ((wallTouchDir == Vector2.left && rb.velocity.x <= 0f) ||
                    (wallTouchDir == Vector2.right && rb.velocity.x >= 0f))
                {
                    rb.gravityScale = 0f;
                    rb.velocity = Vector2.zero;
                    Debug.Log("Stuck to wall");
                    groundState = GroundState.WallStuck;
                    jumpState = JumpState.None;
                    wallStuckEvent?.Invoke(this, EventArgs.Empty);
                    canJump = true;
                    jumpTimer = 0f;

                    // wallTime = 0f;
                }

                break;

            case GroundState.WallStuck:
                if (inputVector.x == -wallTouchDir.x)
                {
                    Debug.Log(inputVector.x);
                    rb.gravityScale = 1f;
                    groundState = GroundState.InAir;
                    rb.velocity = Vector2.right * (inputVector.x * wallPushForce);
                    OnStartFalling();
                }


                break;
        }
    }

    private void OnStartFalling()
    {
        groundState = GroundState.InAir;
        jumpState = JumpState.Fall;
        fallEvent?.Invoke(this, EventArgs.Empty);
    }

    void MapActions()
    {
        // actionMap.Jump.started += context =>
        // {
        //     jumpTimer = 0f;
        // };
        // actionMap.Jump.performed += context => { canJump = false; };
        // actionMap.Jump.canceled += context => { canJump = false; };

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