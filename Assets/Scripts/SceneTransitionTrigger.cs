using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionTrigger : MonoBehaviour
{
    [SerializeField] private string targetSceneName;
    [SerializeField] private Transform playerPartA;
    [SerializeField] private Transform playerPartB;
    [SerializeField] private bool triggerOnce = true;

    private bool _hasTriggered;
    private bool _isPartAInside;
    private bool _isPartBInside;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_hasTriggered && triggerOnce)
        {
            return;
        }

        bool changed = false;
        if (IsColliderFromTarget(other, playerPartA))
        {
            _isPartAInside = true;
            changed = true;
        }

        if (IsColliderFromTarget(other, playerPartB))
        {
            _isPartBInside = true;
            changed = true;
        }

        if (changed)
        {
            Debug.Log($"[SceneTransitionTrigger] Enter: A={_isPartAInside}, B={_isPartBInside}, collider={other.name}", this);
        }

        TryLoadScene();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        bool changed = false;
        if (IsColliderFromTarget(other, playerPartA))
        {
            _isPartAInside = false;
            changed = true;
        }

        if (IsColliderFromTarget(other, playerPartB))
        {
            _isPartBInside = false;
            changed = true;
        }

        if (changed)
        {
            Debug.Log($"[SceneTransitionTrigger] Exit: A={_isPartAInside}, B={_isPartBInside}, collider={other.name}", this);
        }
    }

    private void TryLoadScene()
    {
        if (!_isPartAInside || !_isPartBInside)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(targetSceneName))
        {
            Debug.LogWarning("[SceneTransitionTrigger] targetSceneName is empty. Scene load skipped.", this);
            return;
        }

        Debug.Log($"[SceneTransitionTrigger] Loading scene: {targetSceneName}", this);
        _hasTriggered = true;
        SceneManager.LoadScene(targetSceneName);
    }

    private static bool IsColliderFromTarget(Collider2D other, Transform target)
    {
        if (other == null || target == null)
        {
            return false;
        }

        if (other.transform == target || other.transform.IsChildOf(target))
        {
            return true;
        }

        if (other.attachedRigidbody != null)
        {
            Transform rbTransform = other.attachedRigidbody.transform;
            if (rbTransform == target || rbTransform.IsChildOf(target))
            {
                return true;
            }
        }

        return false;
    }
}
