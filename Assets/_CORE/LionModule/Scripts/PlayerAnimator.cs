using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerAnimator : MonoBehaviour
{
    [Header("Animator Parameters")]
    [SerializeField] private string paramIsWalking = "isWalking";
    [SerializeField] private string paramIsGrounded = "isGrounded";
    [SerializeField] private string paramJumpTrigger = "jump";
    [SerializeField] private string paramHeadGrab = "isHeadGrab";

    [SerializeField] private Animator _anim;
    private PlayerController _controller;
    private HeadGrabber _headGrabber;

    private int _hashIsWalking;
    private int _hashIsGrounded;
    private int _hashJump;
    private int _hashHeadGrab;

    private void Awake()
    {
        _controller = GetComponent<PlayerController>();
        _headGrabber = GetComponent<HeadGrabber>();

        _hashIsWalking = Animator.StringToHash(paramIsWalking);
        _hashIsGrounded = Animator.StringToHash(paramIsGrounded);
        _hashJump = Animator.StringToHash(paramJumpTrigger);
        _hashHeadGrab = Animator.StringToHash(paramHeadGrab);
    }

    private void OnEnable()
    {
        if (_controller != null)
            _controller.Jumped += OnJumped;
    }

    private void OnDisable()
    {
        if (_controller != null)
            _controller.Jumped -= OnJumped;
    }

    private void Update()
    {
        // Update walking and grounded states each frame based on controller
        bool isGrounded = _controller != null && _controller.IsGrounded;
        bool isWalking = _controller != null && Mathf.Abs(_controller.MoveInput) > 0.01f && isGrounded;
        
        _anim.SetBool(_hashIsGrounded, isGrounded);
        _anim.SetBool(_hashIsWalking, isWalking);

        if (_headGrabber != null)
        {
            _anim.SetBool(_hashHeadGrab, _headGrabber.IsGrabbing);       
        }
    }

    private void OnJumped()
    {
        // Fire jump trigger when the controller actually jumped
        _anim.SetTrigger(_hashJump);
    }
}
