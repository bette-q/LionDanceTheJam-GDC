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
    [SerializeField] private PlayerController _assPlayer;
    

    [Header("Interaction Check")]
    [SerializeField] private float checkRadius = 0.15f;
    [SerializeField] private Transform _hand;

    [Header("Input")]
    [SerializeField] private ControlScheme inputScheme = ControlScheme.KeyboardWASD;
    [SerializeField] private float _throwPower = 20f;

    private Rigidbody2D _rb;
    
    private bool _isGrabbed;
    public bool IsGrabbing => _isGrabbed;
    
    // Update is called once per frame
    void Update()
    {
        // Define the center of the overlap circle
        Vector2 centerPoint = _headPlayer.transform.position;
        bool canGrab = Vector2.Distance(centerPoint, transform.position) < checkRadius;

        if (canGrab)
        {
            switch (inputScheme)
            {
                case ControlScheme.KeyboardWASD:
                {
                    var kb = Keyboard.current;
                    if (kb != null)
                    {
                        if (kb.sKey.isPressed && !_isGrabbed)
                        {
                            GrabHead();
                        }
                        else if(_isGrabbed && !kb.eKey.isPressed)
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
                        if (pads[index].buttonEast.isPressed && !_isGrabbed)
                        {
                            GrabHead();
                        }
                        else if(_isGrabbed && !pads[index].buttonEast.isPressed)
                        {
                            Throw();
                        }
                    }
                    break;
                }
            }
        }
    }

    private void GrabHead()
    {
        _headPlayer.transform.SetParent(_hand);
        _headPlayer.enabled = false;
        
        _headPlayer.Rigidbody.bodyType = RigidbodyType2D.Kinematic;
        _headPlayer.Rigidbody.linearVelocity = Vector2.zero;
        _headPlayer.Rigidbody.angularVelocity = 0f;
        
        _headPlayer.transform.DOLocalMove(Vector3.zero, 0.25f).SetEase(Ease.OutCubic);
        _headPlayer.transform.DOLocalRotate(Vector3.zero, 0.25f).SetEase(Ease.OutCubic);
        
        _isGrabbed = true;
    }

    private void Throw()
    {
        _headPlayer.transform.SetParent(null);
        _headPlayer.enabled = true;
        _headPlayer.Rigidbody.bodyType = RigidbodyType2D.Dynamic;
        
        _headPlayer.Rigidbody.AddForce((transform.right + Vector3.up) * _throwPower, ForceMode2D.Impulse);
        
    }
}
