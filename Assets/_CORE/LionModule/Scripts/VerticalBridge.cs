using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(HingeJoint))]
public class VerticalBridge : MonoBehaviour
{
	[Header("Lock Target")]
	[SerializeField] private float _targetAngle = 0f;
	[SerializeField] private float _angleThreshold = 2f;
	[SerializeField] private float _maxAngularVelocityToLock = 0.25f;

	[Header("Lock Behavior")]
	[SerializeField] private bool _makeKinematicOnLock = true;

	private Rigidbody _rigidbody;
	private HingeJoint _hingeJoint;
	private bool _isLocked;

	private void Awake()
	{
		_rigidbody = GetComponent<Rigidbody>();
		_hingeJoint = GetComponent<HingeJoint>();
	}

	private void FixedUpdate()
	{
		if (_isLocked) {
			return;
		}

		float angleDelta = Mathf.Abs(Mathf.DeltaAngle(_hingeJoint.angle, _targetAngle));
		float angularVelocity = _rigidbody.angularVelocity.magnitude;

		if (angleDelta > _angleThreshold || angularVelocity > _maxAngularVelocityToLock) {
			return;
		}

		LockBridge();
	}

	[ContextMenu("Lock Bridge")]
	public void LockBridge()
	{
		if (_isLocked) {
			return;
		}

		JointLimits limits = _hingeJoint.limits;
		limits.min = _targetAngle;
		limits.max = _targetAngle;
		limits.bounciness = 0f;
		_hingeJoint.limits = limits;
		_hingeJoint.useLimits = true;

		_rigidbody.angularVelocity = Vector3.zero;

		if (_makeKinematicOnLock) {
			_rigidbody.isKinematic = true;
		}

		_isLocked = true;
	}
}
