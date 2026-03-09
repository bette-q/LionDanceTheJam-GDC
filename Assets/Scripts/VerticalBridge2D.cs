using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(HingeJoint2D))]
public class VerticalBridge2D : MonoBehaviour
{
	[Header("Lock Target")]
	[SerializeField] private float _targetAngle = 0f;
	[SerializeField] private float _angleThreshold = 2f;
	[SerializeField] private float _maxAngularVelocityToLock = 10f;

	[Header("Lock Behavior")]
	[SerializeField] private bool _makeKinematicOnLock = true;
	[SerializeField] private bool _enforceRotationOnly = true;

	private Rigidbody2D _rigidbody;
	private HingeJoint2D _hingeJoint;
	private bool _isLocked;

	private void Awake()
	{
		_rigidbody = GetComponent<Rigidbody2D>();
		_hingeJoint = GetComponent<HingeJoint2D>();

		if (_enforceRotationOnly) {
			RigidbodyConstraints2D constraints = _rigidbody.constraints;
			constraints |= RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY;
			constraints &= ~RigidbodyConstraints2D.FreezeRotation;
			_rigidbody.constraints = constraints;
		}
	}

	private void FixedUpdate()
	{
		if (_isLocked) {
			return;
		}

		float angleDelta = Mathf.Abs(Mathf.DeltaAngle(_hingeJoint.jointAngle, _targetAngle));
		float angularVelocity = Mathf.Abs(_rigidbody.angularVelocity);

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

		JointAngleLimits2D limits = _hingeJoint.limits;
		limits.min = _targetAngle;
		limits.max = _targetAngle;
		_hingeJoint.limits = limits;
		_hingeJoint.useLimits = true;

		_rigidbody.angularVelocity = 0f;

		if (_makeKinematicOnLock) {
			_rigidbody.bodyType = RigidbodyType2D.Kinematic;
		}

		_isLocked = true;
	}
}
