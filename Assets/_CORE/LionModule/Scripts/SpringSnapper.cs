using UnityEngine;

public class SpringSnapper : MonoBehaviour
{
    [SerializeField] private SpringJoint2D _springJoint2D;
    [SerializeField] private float _springStrength = 20f;
    [SerializeField] private float _snapDuration = 0.2f;
    [SerializeField] private float _maxDistance = 10f;
    [SerializeField] private Transform _target;

    private float  _oldFrequency;
    
    void Start()
    {
        _oldFrequency = _springJoint2D.frequency;
    }
    
    private void Update()
    {
        if (Vector2.Distance(_target.position, transform.position) > _maxDistance)
        {
            _springJoint2D.frequency = _springStrength;
            Invoke(nameof(ResetSnap), _snapDuration);
        }
    }

    private void ResetSnap()
    {
        _springJoint2D.frequency = _oldFrequency;        
    }


}
