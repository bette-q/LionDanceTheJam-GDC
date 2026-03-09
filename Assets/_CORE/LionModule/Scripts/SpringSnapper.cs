using UnityEngine;

public class SpringSnapper : MonoBehaviour
{
    [SerializeField] private SpringJoint2D _springJoint2D;
    [SerializeField] private float _springStrength = 20f;
    [SerializeField] private float _snapDuration = 0.2f;
    [SerializeField] private float _maxDistance = 10f;
    [SerializeField] private Transform _target;

    private float  _oldFrequency;
    private bool _isTooLong = false;
    
    void Start()
    {
        _oldFrequency = _springJoint2D.frequency;
    }
    
    private void Update()
    {
        float dist = Vector2.Distance(_target.position, transform.position);
        
        if (dist > _maxDistance)
        {
            AudioManager.Instance.PlaySfx("shortStretch");
            _springJoint2D.frequency = _springStrength;
            Invoke(nameof(ResetSnap), _snapDuration);
        }else if (dist > _maxDistance * 0.4 && !_isTooLong)
        {
            AudioManager.Instance.PlaySfx("longStretch");
            _isTooLong = true;
        }
    }

    private void ResetSnap()
    {
        _springJoint2D.frequency = _oldFrequency;
        _isTooLong = false;
    }


}
