using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class FallingBlock2D : MonoBehaviour
{
    [SerializeField] private float fallDelaySeconds = 1f;

    private Rigidbody2D _rb;
    private bool _hasTriggered;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.bodyType = RigidbodyType2D.Static;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryStartFall(collision.collider);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryStartFall(other);
    }

    private void TryStartFall(Collider2D other)
    {
        if (_hasTriggered)
        {
            return;
        }

        if (!IsPlayer(other))
        {
            return;
        }

        _hasTriggered = true;
        StartCoroutine(FallAfterDelay());
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

    private IEnumerator FallAfterDelay()
    {
        if (fallDelaySeconds > 0f)
        {
            yield return new WaitForSeconds(fallDelaySeconds);
        }

        _rb.bodyType = RigidbodyType2D.Dynamic;
    }
}
