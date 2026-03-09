using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class StepTile2D : MonoBehaviour
{
    [SerializeField] private float pressDepth = 0.1f;
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float occupancyProbeHeight = 0.2f;
    [SerializeField] private float occupancyProbeInset = 0.05f;

    private Vector3 _startLocalPosition;
    private Rigidbody2D _rb;
    private Collider2D _tileCollider;
    private readonly Collider2D[] _overlapResults = new Collider2D[8];

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _tileCollider = GetComponent<Collider2D>();
        _startLocalPosition = transform.localPosition;
    }

    private void FixedUpdate()
    {
        bool hasPlayerOnTop = HasPlayerOnTop();

        Vector3 targetLocalPosition = hasPlayerOnTop
            ? _startLocalPosition + Vector3.down * pressDepth
            : _startLocalPosition;

        Vector3 targetWorldPosition = transform.parent != null
            ? transform.parent.TransformPoint(targetLocalPosition)
            : targetLocalPosition;

        Vector2 nextPosition = Vector2.MoveTowards(
            _rb.position,
            targetWorldPosition,
            moveSpeed * Time.fixedDeltaTime);

        _rb.MovePosition(nextPosition);
    }

    private bool HasPlayerOnTop()
    {
        Bounds bounds = _tileCollider.bounds;
        float probeWidth = Mathf.Max(0.01f, bounds.size.x - occupancyProbeInset * 2f);
        float probeHeight = Mathf.Max(0.01f, occupancyProbeHeight);
        Vector2 probeSize = new Vector2(probeWidth, probeHeight);
        Vector2 probeCenter = new Vector2(
            bounds.center.x,
            bounds.max.y + probeHeight * 0.5f);

        int hitCount = Physics2D.OverlapBoxNonAlloc(probeCenter, probeSize, 0f, _overlapResults);
        for (int i = 0; i < hitCount; i++)
        {
            Collider2D hit = _overlapResults[i];
            if (hit == null || hit == _tileCollider)
            {
                continue;
            }

            if (IsPlayer(hit))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsPlayer(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            return true;
        }

        if (other.attachedRigidbody != null && other.attachedRigidbody.CompareTag("Player"))
        {
            return true;
        }

        return other.transform.root.CompareTag("Player");
    }
}
