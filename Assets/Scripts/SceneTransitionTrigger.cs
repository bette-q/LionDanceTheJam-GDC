using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionTrigger : MonoBehaviour
{
    [SerializeField] private string targetSceneName;
    [SerializeField] private string player1Tag = "Player";
    [SerializeField] private string player2Tag = "Player";
    [SerializeField] private int requiredPlayersInside = 2;
    [SerializeField] private bool useSpecificPlayerParts = false;
    [SerializeField] private Transform playerPartA;
    [SerializeField] private Transform playerPartB;
    [SerializeField] private bool triggerOnce = true;

    private bool _hasTriggered;
    private readonly HashSet<Transform> _playersInside = new HashSet<Transform>();
    private bool _isPartAInside;
    private bool _isPartBInside;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_hasTriggered && triggerOnce)
        {
            return;
        }

        if (useSpecificPlayerParts)
        {
            if (IsColliderFromTarget(other, playerPartA))
            {
                _isPartAInside = true;
            }

            if (IsColliderFromTarget(other, playerPartB))
            {
                _isPartBInside = true;
            }

            TryLoadScene();
            return;
        }

        Transform playerRoot = GetMatchingPlayerRoot(other);
        if (playerRoot == null)
        {
            return;
        }

        _playersInside.Add(playerRoot);
        TryLoadScene();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (useSpecificPlayerParts)
        {
            if (IsColliderFromTarget(other, playerPartA))
            {
                _isPartAInside = false;
            }

            if (IsColliderFromTarget(other, playerPartB))
            {
                _isPartBInside = false;
            }

            return;
        }

        Transform playerRoot = GetMatchingPlayerRoot(other);
        if (playerRoot == null)
        {
            return;
        }

        _playersInside.Remove(playerRoot);
    }

    private void TryLoadScene()
    {
        if (useSpecificPlayerParts)
        {
            if (!_isPartAInside || !_isPartBInside)
            {
                return;
            }
        }
        else if (_playersInside.Count < requiredPlayersInside)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(targetSceneName))
        {
            return;
        }

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

    private Transform GetMatchingPlayerRoot(Collider2D other)
    {
        if (other == null)
        {
            return null;
        }

        Transform root = other.transform.root;
        if (IsMatchingTag(root))
        {
            return root;
        }

        if (other.attachedRigidbody != null)
        {
            Transform rbRoot = other.attachedRigidbody.transform.root;
            if (IsMatchingTag(rbRoot))
            {
                return rbRoot;
            }
        }

        return null;
    }

    private bool IsMatchingTag(Transform target)
    {
        if (target == null)
        {
            return false;
        }

        string targetTag = target.tag;
        return targetTag == player1Tag || targetTag == player2Tag;
    }
}
