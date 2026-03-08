using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

public class RopeBridge : MonoBehaviour
{

	#region Struct

	public struct RopeSegment
	{
		public Vector2 posNow;
		public Vector2 posOld;

		public RopeSegment(Vector2 pos)
		{
			this.posNow = pos;
			this.posOld = pos;
		}
	}

	#endregion

	#region Variables

	public Transform StartPoint;
	public Transform EndPoint;

	public Transform firstRotation;
	public Transform endRotation;

	public Vector2 EndOffset;
	[Space]
	public float ropeSegLen = 0.25f;
	[FormerlySerializedAs("segmentLength")][SerializeField] private int _ropeSegmentCount = 35;
	public int ropeSegmentCount { get { return _ropeSegmentCount; } set { SetRopeSegmentCount(value); } }   // Use a property to modify this, as just setting the values will cause errors
	public float lineWidth = 0.1f;
	[Space]
	public Vector2 forceGravity = new Vector2(0f, -1f);

	// Internal
	private LineRenderer _lineRenderer;
	private RopeSegment[] _ropeSegments = new RopeSegment[0];
	private Vector3[] _ropePositions = new Vector3[0];

	#endregion

	#region Start / Reset

	void Start()
	{
		_lineRenderer = GetComponent<LineRenderer>();
		SetRopeSegmentCount(ropeSegmentCount);
	}

	#endregion

	#region Update

	private void Update()
	{
		Simulate();
		DrawRope(); 
	}

	#endregion

	#region Set

	public void SetRopeSegmentCount(int segmentCount)
	{
		// Get our start point
		_ropeSegmentCount = segmentCount;
		Vector3 ropeStartPoint = StartPoint.position;

		// Modify the size of both of our arrays
		_ropeSegments = new RopeSegment[segmentCount];
		_ropePositions = new Vector3[segmentCount];

		// Then Initialize each of our points
		for (int i = 0; i < segmentCount; i++) {
			_ropeSegments[i] = new RopeSegment(ropeStartPoint);
			ropeStartPoint.y -= ropeSegLen;
		}

		// Update our Position Count
		_lineRenderer.positionCount = _ropePositions.Length;
	}

	public Vector3[] GetRopePositions()
	{
		return _ropePositions;
	}

	#endregion

	#region Simulate + Draw

	public void RefreshVisual() {
		enabled = true;
		//toggle ropebridge off in 1.5 seconds
		Invoke(nameof(ToggleRopeBridge), 1.5f);
	}
	private void ToggleRopeBridge()
	{
		enabled = false;
	}

	private void Simulate()
	{
		// SIMULATION

		for (int i = 1; i < ropeSegmentCount; i++) {
			RopeSegment firstSegment = _ropeSegments[i];
			Vector2 velocity = firstSegment.posNow - firstSegment.posOld;
			firstSegment.posOld = firstSegment.posNow;
			firstSegment.posNow += velocity;
			firstSegment.posNow += forceGravity * Time.fixedDeltaTime;
			_ropeSegments[i] = firstSegment;
		}

		//CONSTRAINTS
		for (int i = 0; i < 50; i++)
			ApplyConstraint();
	}

	private void ApplyConstraint()
	{
		//Constrant to First Point 
		RopeSegment firstSegment = _ropeSegments[0];
		firstSegment.posNow = StartPoint.position;
		_ropeSegments[0] = firstSegment;


		//Constrant to Second Point 
		RopeSegment endSegment = _ropeSegments[_ropeSegments.Length - 1];
		endSegment.posNow = EndPoint.position + EndPoint.TransformVector(EndOffset);
		_ropeSegments[_ropeSegments.Length - 1] = endSegment;

		for (int i = 0; i < ropeSegmentCount - 1; i++) {
			RopeSegment firstSeg = _ropeSegments[i];
			RopeSegment secondSeg = _ropeSegments[i + 1];

			float dist = (firstSeg.posNow - secondSeg.posNow).magnitude;
			float error = Mathf.Abs(dist - ropeSegLen);
			Vector2 changeDir = Vector2.zero;

			if (dist > ropeSegLen) {
				changeDir = (firstSeg.posNow - secondSeg.posNow).normalized;
			}
			else if (dist < ropeSegLen) {
				changeDir = (secondSeg.posNow - firstSeg.posNow).normalized;
			}

			Vector2 changeAmount = changeDir * error;
			if (i != 0) {
				firstSeg.posNow -= changeAmount * 0.5f;
				_ropeSegments[i] = firstSeg;
				secondSeg.posNow += changeAmount * 0.5f;
				_ropeSegments[i + 1] = secondSeg;
			}
			else {
				secondSeg.posNow += changeAmount;
				_ropeSegments[i + 1] = secondSeg;
			}
		}
	}

	private void DrawRope()
	{
		float lineWidth = this.lineWidth;
		_lineRenderer.startWidth = lineWidth;
		_lineRenderer.endWidth = lineWidth;

		// Then update our Rope Positions array
		for (int i = 0; i < _ropePositions.Length; i++)
			_ropePositions[i] = _ropeSegments[i].posNow;

		_lineRenderer.SetPositions(_ropePositions);

		Vector3 direction = _ropePositions[1] - _ropePositions[0];
		float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

		if (firstRotation != null) {
			// Make the start point lerp rotate towards the first segment in 2D space
			Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.forward);
			firstRotation.rotation = Quaternion.Lerp(firstRotation.rotation, targetRotation, Time.deltaTime * 20f);
		}

		if (endRotation != null) {
			endRotation.gameObject.SetActive(true);
			// Make the end point rotate towards the second last segment in 2D space
			direction = _ropePositions[_ropePositions.Length - 2] - _ropePositions[_ropePositions.Length - 1];
			angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
			Quaternion targetEndRotation = Quaternion.AngleAxis(angle, Vector3.forward);
			endRotation.rotation = Quaternion.Lerp(endRotation.rotation, targetEndRotation, Time.deltaTime * 20f);
			endRotation.position = _ropePositions[_ropePositions.Length - 1];

		}

	}

	private void OnDisable()
	{
		if (EndPoint == null && endRotation != null) {
			endRotation.position = StartPoint.position;
			endRotation.gameObject.SetActive(EndPoint != null);
		}
	}

	#endregion

#if UNITY_EDITOR

	[ContextMenu("RedrawBridge")]
	private void RedrawBridge()
	{
		_lineRenderer = GetComponent<LineRenderer>();
		SetRopeSegmentCount(ropeSegmentCount);
		Simulate();
		DrawRope();
	}

#endif

}
