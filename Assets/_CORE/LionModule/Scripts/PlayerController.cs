using System;
using UnityEngine;
using UnityEngine.InputSystem; // New Input System

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    public enum ControlScheme
    {
        KeyboardWASD,
        KeyboardArrows,
    }

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float rotateForce = 12f;
    
    [Header("Ground Check")]
    [Tooltip("Position to check for ground contact (usually placed at feet).")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Input")]
    [SerializeField] private ControlScheme inputScheme = ControlScheme.KeyboardWASD;

    private Rigidbody2D _rb;
    private float _moveInput;
    private bool _jumpRequested;
    private bool _isGrounded;
    private bool _facingRight = true;

    // Exposed read-only properties and jump event for animation/scripts
    public float MoveInput => _moveInput;
    public bool IsGrounded => _isGrounded;
    
    public Rigidbody2D Rigidbody => _rb;
    public event Action Jumped;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        
        // Gather input in Update using the new Input System (frame-rate dependent)
        float move = 0f;
        _jumpRequested = false; // will be set true if pressed this frame

        switch (inputScheme)
        {
            case ControlScheme.KeyboardWASD:
            {
                var kb = Keyboard.current;
                if (kb != null)
                {
                    if (kb.aKey.isPressed) move -= 1f;
                    if (kb.dKey.isPressed) move += 1f;
                    if (_isGrounded && (kb.spaceKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame))
                        _jumpRequested = true;
                }
                
                var pads = Gamepad.all;
                if (pads.Count > 0 && pads[0] != null)
                {
                    var pad = pads[0];
                    float stickX = pad.leftStick.x.ReadValue();
                    float dpadX = pad.dpad.x.ReadValue();
                    float analogX = Mathf.Abs(stickX) > Mathf.Abs(dpadX) ? stickX : dpadX;
                    move = Mathf.Clamp(analogX, -1f, 1f);
                    if (_isGrounded && pad.buttonSouth.wasPressedThisFrame)
                        _jumpRequested = true;
                }
                break;
            }
            case ControlScheme.KeyboardArrows:
            {
                var kb = Keyboard.current;
                if (kb != null)
                {
                    if (kb.leftArrowKey.isPressed) move -= 1f;
                    if (kb.rightArrowKey.isPressed) move += 1f;
                    if (_isGrounded && kb.upArrowKey.wasPressedThisFrame)
                        _jumpRequested = true;
                }
                
                var pads = Gamepad.all;
                if (pads.Count > 1 && pads[1] != null)
                {
                    var pad = pads[1];
                    float stickX = pad.leftStick.x.ReadValue();
                    float dpadX = pad.dpad.x.ReadValue();
                    float analogX = Mathf.Abs(stickX) > Mathf.Abs(dpadX) ? stickX : dpadX;
                    move = Mathf.Clamp(analogX, -1f, 1f);
                    if (_isGrounded && pad.buttonSouth.wasPressedThisFrame)
                        _jumpRequested = true;
                }
                break;
            }
        }

        _moveInput = Mathf.Clamp(move, -1f, 1f); // -1..1

        // Handle sprite facing direction
        if (_moveInput > 0.01f && !_facingRight)
            Flip();
        else if (_moveInput < -0.01f && _facingRight)
            Flip();
    
        //can only flip is rigidbody is kinematic
        if(_rb.bodyType == RigidbodyType2D.Kinematic) return;
        
        // Ground check using an overlap circle
        Vector2 checkPos = (Vector2)transform.position + Vector2.down;
        _isGrounded = Physics2D.OverlapCircle(checkPos, groundCheckRadius, groundLayer);

        // Horizontal movement
        _rb.linearVelocity = new Vector2(_moveInput * moveSpeed, _rb.linearVelocity.y);
        
        // angular movement
        _rb.AddTorque(_moveInput * -rotateForce, ForceMode2D.Force);

        // Jump (impulse) when requested and grounded
        if (_jumpRequested && _isGrounded)
        {
            // Reset any downward velocity before jump for consistent height
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, 0f);
            _rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            // Notify listeners that a jump occurred
            Jumped?.Invoke();
            AudioManager.Instance.PlaySfx("jump");
        }
        _jumpRequested = false;
    }

    private void Flip()
    {
        _facingRight = !_facingRight;
        Vector3 s = transform.localScale;
        s.x *= -1f;
        transform.localScale = s;
    }

    void OnDrawGizmosSelected()
    {
        if (!enabled) return;
        Gizmos.color = Color.yellow;
        Vector3 pos = groundCheck != null ? groundCheck.position : transform.position;
        Gizmos.DrawWireSphere(pos, groundCheckRadius);
    }
}
