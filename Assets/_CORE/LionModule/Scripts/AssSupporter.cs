using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem; // New Input System

public class AssSupporter : MonoBehaviour
{
    
    public enum ControlScheme
    {
        KeyboardWASD,
        Gamepad2,
    }
    
    [SerializeField] private PlayerController _headPlayer;
    [SerializeField] private HeadGrabber _headGrabber;
    [SerializeField] private PlayerController _assPlayer;
    [SerializeField] private SpringJoint2D _springJoint2D;
    

    [Header("Interaction Check")]
    [SerializeField] private float checkRadius = 0.15f;
    [SerializeField] private Transform _hand;

    [Header("Input")]
    [SerializeField] private ControlScheme inputScheme = ControlScheme.KeyboardWASD;
    [SerializeField] private float _throwPower = 20f;

    private Rigidbody2D _rb;
    
    private bool _isGrabbed = false;
    public bool IsGrabbing => _isGrabbed;
    
    // Update is called once per frame
    void Update()
    {
        // Define the center of the overlap circle
        Vector2 centerPoint = _headPlayer.transform.position;
        bool canGrab = Vector2.Distance(centerPoint, _assPlayer.transform.position) < checkRadius;

        if (canGrab)
        {
            switch (inputScheme)
            {
                case ControlScheme.KeyboardWASD:
                {
                    var kb = Keyboard.current;
                    if (kb != null)
                    {
                        if (kb.eKey.wasReleasedThisFrame && !_isGrabbed)
                        {
                            GrabHead();
                        }
                        else if(_isGrabbed && (kb.eKey.wasReleasedThisFrame || kb.upArrowKey.wasReleasedThisFrame))
                        {
                            Throw();
                        }
                    }
                    break;
                }
                case ControlScheme.Gamepad2:
                {
                    int index = inputScheme == ControlScheme.Gamepad2 ? 1 : 0;
                    var pads = Gamepad.all;
                    if (pads.Count > index && pads[index] != null)
                    {
                        if (pads[index].buttonEast.wasReleasedThisFrame && !_isGrabbed)
                        {
                            GrabHead();
                        }
                        else if(_isGrabbed && pads[index].buttonEast.wasReleasedThisFrame)
                        {
                            Throw();
                        }
                    }
                    break;
                }
            }
        }
    }

    private void FlipUp()
    {
        _assPlayer.transform.DORotate(Vector3.zero, 0.35f, RotateMode.Fast);
    }

    private void GrabHead()
    {
        FlipUp();
        _headGrabber.enabled = false;
        
        _headPlayer.transform.SetParent(_hand);
        _headPlayer.enabled = false;
        
        _headPlayer.Rigidbody.bodyType = RigidbodyType2D.Kinematic;
        _headPlayer.Rigidbody.linearVelocity = Vector2.zero;
        _headPlayer.Rigidbody.angularVelocity = 0f;
        
        _headPlayer.transform.DOLocalMove(Vector3.zero, 0.25f).SetEase(Ease.OutCubic);
        _headPlayer.transform.DOLocalRotate(Vector3.zero, 0.25f).SetEase(Ease.OutCubic)
            .OnComplete(() => _isGrabbed = true);

        _springJoint2D.enabled = false;
        Debug.Log("Grabbed");
    }

    private void Throw()
    {
        FlipUp();
        
        _headPlayer.transform.DOLocalRotate(new Vector3(0, 0, 720), 1f)
            .SetDelay(0.35f)
            .OnStart(() =>
            {
                AudioManager.Instance.PlaySfx("throw");
                _headPlayer.transform.SetParent(null);
                _headPlayer.Rigidbody.bodyType = RigidbodyType2D.Dynamic;
        
                Vector3 dir = _headPlayer.transform.position.x < _assPlayer.transform.position.x ? Vector3.left : Vector3.right;
                dir = (dir + Vector3.up * 2 ) * _throwPower;
                _headPlayer.Rigidbody.AddForce(dir, ForceMode2D.Impulse);
            })
            .SetEase(Ease.OutCubic)
            .OnComplete(() =>
            {
                _headPlayer.enabled = true;
                _headPlayer.transform.localEulerAngles = new Vector3(0, 0, 0); 
                _springJoint2D.enabled = true;
                _headGrabber.enabled = true;
                _isGrabbed = false;
            });
            

        Debug.Log("Throw");
        
    }
}
