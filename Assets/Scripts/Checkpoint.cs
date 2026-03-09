using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Checkpoint : MonoBehaviour
{
    [SerializeField] private CheckpointManager manager;
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private string[] validTags = { "Player", "Player1", "Player2" };
    [SerializeField] private bool activateOnce = true;

    private bool _isActivated;

    private void Reset()
    {
        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void Awake()
    {
        if (manager == null)
        {
            manager = CheckpointManager.Instance;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_isActivated && activateOnce)
        {
            return;
        }

        if (!IsValidPlayer(other))
        {
            return;
        }

        if (manager == null)
        {
            manager = CheckpointManager.Instance;
        }

        if (manager == null)
        {
            return;
        }

        manager.SetCheckpoint(respawnPoint != null ? respawnPoint.position : transform.position);
        _isActivated = true;
        Debug.Log("Checkpoint activated: " + transform.position);
    }

    private bool IsValidPlayer(Collider2D other)
    {
        if (other == null)
        {
            return false;
        }

        for (int i = 0; i < validTags.Length; i++)
        {
            string tagName = validTags[i];
            if (string.IsNullOrWhiteSpace(tagName))
            {
                continue;
            }

            if (other.CompareTag(tagName))
            {
                return true;
            }

            if (other.attachedRigidbody != null && other.attachedRigidbody.CompareTag(tagName))
            {
                return true;
            }

            if (other.transform.root.CompareTag(tagName))
            {
                return true;
            }
        }

        return false;
    }
}
