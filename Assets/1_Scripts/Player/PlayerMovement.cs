using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using UnityEngine.Events;


[RequireComponent(typeof(Rigidbody2D)), RequireComponent(typeof(BoxCollider2D))]
public class PlayerMovement : MonoBehaviour
{
    PlayerInput playerInput;
    public PlayerInput PlayerInput { get { return playerInput; } set { playerInput = value; } }
    
    PlayerInputSettings playerInputSettings;
    public PlayerInputSettings PlayerInputSettings { get { return playerInputSettings; } set { playerInputSettings = value; } }

    Rigidbody2D rigid2D;
    BoxCollider2D boxCollider2D;
    EventDictionary eventDictionary;

    /// <summary>
    /// An action map must be set before taking control.
    /// </summary>
    InputActionMap inputActionMap;
    public InputActionMap InputActionMap
    {
        get {  return inputActionMap; }
        set { inputActionMap = value;
            inputActionMap.Enable();
            inputActionMap.FindAction("Movement").performed += Movement_performed;
            inputActionMap.FindAction("Movement").canceled += Movement_canceled;
            inputActionMap.FindAction("Jump").started += Jump_started;
            inputActionMap.FindAction("Jump").canceled += Jump_canceled;
            inputActionMap.FindAction("DashLeft").started += DashLeft_started;
            inputActionMap.FindAction("DashRight").started += DashRight_started;
        }
    }

    [Header("CONTROLLER")]
    [SerializeField]
    string actionMap = "Player";

    [Header("COLLISION")]
    [SerializeField]
    LayerMask ground;    
    [SerializeField]
    float rayLength = 0.1f;

    [Header("GROUND MOVEMENT")]
    [SerializeField]
    float maxSpeed = 1f;
    [SerializeField]
    float minSpeed = 0.1f;
    [SerializeField]
    float acceleration = 1f;
    [SerializeField]
    float deceleration = 1f;

    [Header("AERIAL MOVEMENT")]
    [SerializeField]
    float aerialAcceleration = 1f;
    [SerializeField]
    float aerialDeceleration = 1f;

    [Header("JUMPING")]
    [SerializeField]
    float jumpForce = 1f;
    [SerializeField]
    float jumpReleaseForce = 1f;
    [SerializeField]
    int multiJumps = 0;
    [SerializeField]
    float multiJumpFallingThreshold = -1f;

    [Header("DASHING")]
    [SerializeField]
    float dashDuration = 1f;
    [SerializeField]
    float dashDistance = 1f;
    [SerializeField]
    Ease dashCurve = Ease.Linear;
    [SerializeField]
    float dashCooldown = 1f;
    [SerializeField]
    LayerMask obstacles;

    [Header("WALLS")]
    bool checkForWallCollision = false;

    Vector2 boxExtent = Vector2.one;
    float movementInput = 0f;
    bool grounded = false;
    RaycastHit2D[] hits;

    bool jumping = false;
    bool jump_release = false;
    int jumpCount = 0;
    bool CanJump { get
        {
            return jumpCount < multiJumps;
        } }

    bool canDash = true;
    int dash = 0;
    float dashCounter = 0f;
    Tween dashTween = null;

    string state;

    Vector3 Velocity { get { return rigid2D.velocity; } }
    bool IsGrounded { get { return grounded; } }
    bool InAir { get { return !IsGrounded; } }
    bool IsJumping { get { return jumping; } }
    bool IsElevating { get { return !IsGrounded && Velocity.y >= 0f; } } 
    bool IsFalling { get { return !IsGrounded && Velocity.y < 0f; } }
    bool InputRecieved { get { return Mathf.Abs(movementInput) > 0.01f; } }
    bool IsDashing { get { return dash != 0; } }

    bool lastFalling = true;

    private void Awake()
    {        
        rigid2D = GetComponent<Rigidbody2D>();
        boxCollider2D = GetComponent<BoxCollider2D>();
        boxExtent = boxCollider2D.bounds.size / 2f;
        eventDictionary = GetComponent<EventDictionary>();
    }

    private void Start()
    {
        //settings = FindObjectOfType< InputManager.Instance.PlayerInputSettings;

        if(checkForWallCollision)
            hits = new RaycastHit2D[9];
        else
            hits = new RaycastHit2D[3];   
    }

    private void OnDestroy()
    {
        inputActionMap.FindAction("Movement").performed -= Movement_performed;
        inputActionMap.FindAction("Movement").canceled -= Movement_canceled;
        inputActionMap.FindAction("Jump").started -= Jump_started;
        inputActionMap.FindAction("Jump").canceled -= Jump_canceled;
        inputActionMap.FindAction("DashLeft").started -= DashLeft_started;
        inputActionMap.FindAction("DashRight").started -= DashRight_started;
    }

    private void UpdateState(string newState)
    {
        if(newState != state)
        {
            state = newState;
            eventDictionary?.Invoke(state);
        }
    }

    private void Movement_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        movementInput = obj.ReadValue<float>();

        if (!IsDashing)
        {
            if (IsGrounded)
                UpdateState("Run");

            if (movementInput > 0f)
            {
                UpdateState("Right");
            }
            else
            {
                UpdateState("Left");
            }
        }
    }

    private void Movement_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        movementInput = 0f;

        if(IsGrounded)
            UpdateState("Stop");
    }    

    private void Jump_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        Jump();
    }

    private void Jump_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        jump_release = true;
    }

    private void DashRight_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        Dash(true);
    }    

    private void DashLeft_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        Dash(false);        
    }    

    private void Dash(bool right)
    {
        if (!canDash) return;

        dash = right ? 1 : -1;
        rigid2D.velocity = new Vector2(0f, 0f);
        canDash = false;

        float target = dashDistance;

        RaycastHit2D checkForWall = Physics2D.Raycast(transform.position, Vector2.right * dash, dashDistance, obstacles);
        if(checkForWall.collider == null) checkForWall = Physics2D.Raycast((Vector2)transform.position - new Vector2(0f, boxExtent.y), Vector2.right * dash, dashDistance, obstacles);
        if (checkForWall.collider == null) checkForWall = Physics2D.Raycast((Vector2)transform.position + new Vector2(0f, boxExtent.y), Vector2.right * dash, dashDistance, obstacles);

        if (checkForWall.collider != null && checkForWall.collider != boxCollider2D)
        {
            target = checkForWall.distance - boxExtent.x;
        }

        float duration = (target / dashDistance) * dashDuration;
        UpdateState("Dash");

        dashTween = transform.DOMoveX(transform.position.x + target * dash, duration)
            .SetEase(dashCurve)
            .OnComplete(() =>
            {
                dash = 0;
                UpdateState("StopDash");
                dashTween.Kill();
            });        
    }
    

    private void Jump()
    {
        if(!CanJump) return;

        if (!IsGrounded)
        {
            if(jumping && Velocity.y > multiJumpFallingThreshold && jump_release && jumpCount < multiJumps)
            {
                rigid2D.velocity = new Vector2(Velocity.x, 0f);
                rigid2D.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                UpdateState("MultiJump");
                jumpCount++;
            }

            jump_release = false;
            return;
        }

        ResetDash();

        jumping = true;
        rigid2D.velocity = new Vector2(Velocity.x, 0f);
        rigid2D.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        UpdateState("Jump");
    }
    

    private void FixedUpdate()
    {
        //Dashes override other movement, handle first.
        if (IsDashing)
        {
            rigid2D.velocity = Vector2.zero;
            return;
        }
        else if(!canDash && !dashTween.IsActive())
        {
            dashCounter += 0.016f;
            if(dashCounter > dashCooldown)
            {
                ResetDash();
            }
        }

        bool wasGrounded = IsGrounded;

        //Check if feet hit the ground
        Vector2 pos = transform.position;
        Vector2 feet = new Vector3(pos.x, pos.y - boxExtent.y);
        hits[0] = Physics2D.Raycast(feet, Vector2.down, rayLength, ground);
        hits[1] = Physics2D.Raycast(feet + new Vector2(boxExtent.x, 0f), Vector2.down, rayLength, ground);
        hits[2] = Physics2D.Raycast(feet - new Vector2(boxExtent.x, 0f), Vector2.down, rayLength, ground);
        //Any hits?
        for (int i = 0; i < hits.Length; i++)
        {
            grounded = hits[i].collider != null && hits[i].transform ;
            if (IsGrounded) break;            
        }

        if(IsGrounded && !wasGrounded)
        {
            Landed(jumping);
        }

        //Handle Jump
        if(jumping && InAir)
        {
            if (jump_release)
                rigid2D.AddForce(-Vector2.up * jumpReleaseForce);
        }

        //Handle movement
        if (IsGrounded)
            GroundMovement();
        else
            AerialMovement();
    }

    private void ResetDash()
    {
        dashCounter = 0f;
        canDash = true;
    }

    private void Landed(bool wasJumping)
    {
        if (wasJumping)
        {
            jumpCount = 0;
            if (!jump_release)
                Jump();
            else
            {
                jumping = false;
                jump_release = false;
                UpdateState("Land");
            }            
        }
        else
            UpdateState("Land");
    }

    private void GroundMovement()
    {
        float target = movementInput * maxSpeed;

        float speedDifference = target - Velocity.x;

        float accelRate = (Mathf.Abs(target) > 0.01f) ? acceleration : deceleration;

        float movement = Mathf.Pow(Mathf.Abs(speedDifference) * accelRate, 1f) * Mathf.Sign(speedDifference);

        rigid2D.AddForce(movement * Vector2.right);

        if (InputRecieved && Mathf.Abs(movementInput) > 0f && Mathf.Abs(Velocity.x) < minSpeed)
            rigid2D.velocity = new Vector2(movementInput * minSpeed, Velocity.y);
    }

    private void AerialMovement()
    {
        float target = movementInput * maxSpeed;

        float speedDifference = target - Velocity.x;

        float accelRate = (Mathf.Abs(target) > 0.01f) ? aerialAcceleration : aerialDeceleration;

        float movement = Mathf.Pow(Mathf.Abs(speedDifference) * accelRate, 1f) * Mathf.Sign(speedDifference);

        rigid2D.AddForce(movement * Vector2.right);
    }
}
