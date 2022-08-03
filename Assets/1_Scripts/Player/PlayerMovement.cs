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
    InputActionMap inputActionMap;
    PlayerInputSettings settings;
    Rigidbody2D rigid2D;
    BoxCollider2D boxCollider2D;
    EventDictionary eventDictionary;

    [Header("CONTROLLER")]
    [SerializeField]
    string actionMap = "Player";

    [Header("COLLISION")]
    [SerializeField]
    LayerMask ground;
    [SerializeField]
    Vector2 boxExtent = Vector2.one;
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

    [Header("LOCAL MULTIPLAYER")]
    [SerializeField]
    bool localMultiplayer = false;    

    float movementInput = 0f;
    bool grounded = false;
    RaycastHit2D[] hits;
    bool jumping = false;
    bool jump_release = false;
    int jumpCount = 0;
    bool canDash = true;
    int dash = 0;
    float dashCounter = 0f;
    Tween dashTween = null;

    Vector3 Velocity { get { return rigid2D.velocity; } }
    bool IsGrounded { get { return grounded; } }
    bool InAir { get { return !IsGrounded; } }
    bool IsJumping { get { return jumping; } }
    bool IsElevating { get { return !IsGrounded && Velocity.y >= 0f; } } 
    bool IsFalling { get { return !IsGrounded && Velocity.y < 0f; } }
    bool InputRecieved { get { return Mathf.Abs(movementInput) > 0.01f; } }
    bool IsDashing { get { return dash != 0; } }

    private void Awake()
    {        
        if (!localMultiplayer)
            inputActionMap = playerInput.actions.FindActionMap(actionMap);

        rigid2D = GetComponent<Rigidbody2D>();
        boxCollider2D = GetComponent<BoxCollider2D>();
        eventDictionary = GetComponent<EventDictionary>();
    }

    public void SetPlayerInput(PlayerInput input)
    {
        playerInput = input;
    }

    public void ChangeInputActionMap(InputActionMap actionMap)
    {
        Debug.Log("Setting action map for " + gameObject.name);

        if (actionMap != null)
        {
            inputActionMap = actionMap;
            inputActionMap.Enable();
          
            inputActionMap.FindAction("Movement").performed += Movement_performed;
            inputActionMap.FindAction("Movement").canceled += Movement_canceled;
            inputActionMap.FindAction("Jump").started += Jump_started;
            inputActionMap.FindAction("Jump").canceled += Jump_canceled;
            inputActionMap.FindAction("DashLeft").started += DashLeft_started;
            inputActionMap.FindAction("DashRight").started += DashRight_started;
        }
        else
            Debug.LogWarning("InputActionMap Null!");
    }

    PlayerMovement secondKeyboard;
    public bool HasSecondKeyboard { get { return secondKeyboard != null; } }
    public void AttachSecondKeyboard(PlayerMovement second)
    {
        secondKeyboard = second;
        inputActionMap.FindAction("Movement2").performed += second.Movement_performed;
        inputActionMap.FindAction("Movement2").canceled += second.Movement_canceled;
        inputActionMap.FindAction("Jump2").started += second.Jump_started;
        inputActionMap.FindAction("Jump2").canceled += second.Jump_canceled;
        inputActionMap.FindAction("DashLeft2").started += second.DashLeft_started;
        inputActionMap.FindAction("DashRight2").started += second.DashRight_started;
    }

    private void Start()
    {
        settings = InputManager.Instance.PlayerInputSettings;

        if(checkForWallCollision)
            hits = new RaycastHit2D[9];
        else
            hits = new RaycastHit2D[3];   
        

        //settings.Player.Movement.performed += context => Movement(context);
        //settings.Player.Movement.canceled += context => Movement_canceled(context);
        //settings.Player.Jump.started += context => Jump(context);
        //settings.Player.Jump.canceled += context => Jump_canceled(context);
        //settings.Player.DashLeft.started += context => DashLeft_started(context);
        //settings.Player.DashRight.started += context => DashRight_started(context);
    }    
    private void Movement_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        movementInput = obj.ReadValue<float>();
    }

    private void Movement_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        movementInput = 0f;
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

        dashTween = transform.DOMoveX(transform.position.x + target * dash, duration)
            .SetEase(dashCurve)
            .OnComplete(() =>
            {
                dash = 0;
                dashTween.Kill();
            });

        eventDictionary?.Invoke("Dash");
    }
    

    private void Jump()
    {
        if (!IsGrounded)
        {
            if(jumping && Velocity.y > multiJumpFallingThreshold && jump_release && jumpCount < multiJumps)
            {
                rigid2D.velocity = new Vector2(Velocity.x, 0f);
                rigid2D.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                eventDictionary?.Invoke("MultiJump");
                jumpCount++;
            }

            jump_release = false;
            return;
        }

        ResetDash();

        jumping = true;
        rigid2D.velocity = new Vector2(Velocity.x, 0f);
        rigid2D.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        eventDictionary?.Invoke("Jump");
    }
    

    private void FixedUpdate()
    {
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


        Vector2 pos = transform.position;
        Vector2 feet = new Vector3(pos.x, pos.y - boxExtent.y);

        //Raycast feet
        hits[0] = Physics2D.Raycast(feet, Vector2.down, rayLength, ground);
        hits[1] = Physics2D.Raycast(feet + new Vector2(boxExtent.x, 0f), Vector2.down, rayLength, ground);
        hits[2] = Physics2D.Raycast(feet - new Vector2(boxExtent.x, 0f), Vector2.down, rayLength, ground);

        bool wasGrounded = IsGrounded;

        //Any hits?
        for(int i = 0; i < hits.Length; i++)
        {
            grounded = hits[i].collider != null;
            if (IsGrounded) break;            
        }

        if(IsGrounded && !wasGrounded)
        {
            Landed(jumping);
        }

        //Handle Jump
        if(jumping && IsGrounded)
        {

        }
        else if(jumping && InAir)
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
        if(wasJumping)
        {
            jumpCount = 0;
            if (!jump_release)
                Jump();
            else
            {
                jumping = false;
                jump_release = false;
            }
        }
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
