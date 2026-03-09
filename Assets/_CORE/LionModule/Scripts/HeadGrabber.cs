using System;
using UnityEngine;
using UnityEngine.InputSystem; // New Input System

[RequireComponent(typeof(Rigidbody2D))]
public class HeadGrabber : MonoBehaviour
{
    public enum ControlScheme
    {
        KeyboardWASD,
        Gamepad1,
    }

    [Header("Interaction Check")]
    [Tooltip("Position to check for ground contact (usually placed at feet).")]
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Input")]
    [SerializeField] private ControlScheme inputScheme = ControlScheme.KeyboardWASD;

    private Rigidbody2D _rb;
    
    private bool _isInteractable;
    public bool IsGrounded => _isInteractable;
    
    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Gather input in Update using the new Input System (frame-rate dependent)
        float move = 0f;
    
        switch (inputScheme)
        {
            case ControlScheme.KeyboardWASD:
            {
                var kb = Keyboard.current;
                if (kb != null)
                {
                    _rb.bodyType = _isInteractable && kb.eKey.wasPressedThisFrame ? RigidbodyType2D.Kinematic : RigidbodyType2D.Dynamic;
                }
                break;
            }
            case ControlScheme.Gamepad1:
            {
                int index = inputScheme == ControlScheme.Gamepad1 ? 0 : 1;
                var pads = Gamepad.all;
                if (pads.Count > index && pads[index] != null)
                {
                    var pad = pads[index];
                    _rb.bodyType = _isInteractable && pad.buttonEast.wasPressedThisFrame ? RigidbodyType2D.Kinematic : RigidbodyType2D.Dynamic;
                }
                break;
            }
        }
    
        // Ground check using an overlap circle
        _isInteractable = Physics2D.OverlapCircle(transform.position, groundCheckRadius, groundLayer);

        
        
    }
}
