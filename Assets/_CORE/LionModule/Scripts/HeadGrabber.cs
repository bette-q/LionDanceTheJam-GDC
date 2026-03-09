using UnityEngine;
using UnityEngine.InputSystem; // New Input System
using DG.Tweening;

[RequireComponent(typeof(Rigidbody2D))]
public class HeadGrabber : MonoBehaviour
{
    public enum ControlScheme
    {
        KeyboardArrow,
        Gamepad1,
    }

    [Header("Interaction Check")]
    [Tooltip("Position to check for ground contact (usually placed at feet).")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private float esp = 0.1f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Input")]
    [SerializeField] private ControlScheme inputScheme = ControlScheme.KeyboardArrow;

    private Rigidbody2D _rb;
    
    private bool _isInteractable;
    public bool IsGrabbing => _rb.bodyType == RigidbodyType2D.Kinematic;

    private bool _isSnapping = false;
    
    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {

        // Define the center of the overlap circle
        Vector2 centerPoint = groundCheck.position;
        
        // Get all colliders within the circle
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(centerPoint, groundCheckRadius, groundLayer);

        _isInteractable = hitColliders.Length > 0;

        if (_isInteractable)
        {
            float closestDistance = float.MaxValue;

            Vector2 closestPoint = groundCheck.position;
            
            foreach (Collider2D hitCollider in hitColliders)
            {
                // Find the closest point on the hit collider to the center of the overlap
                Vector2 contactPoint = hitCollider.ClosestPoint(centerPoint);
                float distance = (contactPoint - centerPoint).magnitude;
                
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPoint = hitCollider.ClosestPoint(centerPoint);
                }
            }
            
            switch (inputScheme)
            {
                case ControlScheme.KeyboardArrow:
                {
                    var kb = Keyboard.current;
                    if (kb != null)
                    {
                        _rb.bodyType = kb.downArrowKey.isPressed ? RigidbodyType2D.Kinematic : RigidbodyType2D.Dynamic;

                        if (!_isSnapping && closestDistance > esp && kb.downArrowKey.isPressed)
                        {
                            SnapToTarget(closestPoint);
                        }
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
                        _rb.bodyType = pad.buttonEast.isPressed ? RigidbodyType2D.Kinematic : RigidbodyType2D.Dynamic;
                        
                        if (!_isSnapping && closestDistance > esp && pad.buttonEast.isPressed)
                        {
                            SnapToTarget(closestPoint);
                        }
                    }
                    break;
                }
            }

            if (_rb.bodyType == RigidbodyType2D.Kinematic)
            {
                _rb.linearVelocity = Vector2.zero;
                _rb.angularVelocity = 0f;            
            }
            
        }
        
    }
    
    private void SnapToTarget(Vector2 targetPosition)
    {
        // Move to target
        Vector3 targetPos3 = new Vector3(targetPosition.x, targetPosition.y, transform.position.z);

        // Compute look angle so local +X faces the target (2D: rotate around Z)
        Vector2 toTarget = (Vector2)(targetPos3 - transform.position);
        float zAngle = Mathf.Atan2(toTarget.y, toTarget.x) * Mathf.Rad2Deg + 90;


        // Tween position and rotation in parallel
        var moveTween = transform.DOMove(targetPos3, 0.25f).SetEase(Ease.OutCubic);
        var rotateTween = transform.DORotate(new Vector3(0f, 0f, zAngle), 0.25f, RotateMode.Fast)
            .SetEase(Ease.OutCubic);

        DOTween.Sequence()
            .Join(moveTween)
            .Join(rotateTween)
            .OnComplete(() => _isSnapping = false);
    }
}
