using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionTrigger : MonoBehaviour
{
    [SerializeField] private string targetSceneName;
    [SerializeField] private string player1Tag = "Player1";
    [SerializeField] private string player2Tag = "Player2";
    [SerializeField] private bool triggerOnce = true;

    private bool _hasTriggered;
    private bool _player1Inside;
    private bool _player2Inside;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_hasTriggered && triggerOnce)
        {
            return;
        }

        if (other.CompareTag(player1Tag))
        {
            _player1Inside = true;
        }
        else if (other.CompareTag(player2Tag))
        {
            _player2Inside = true;
        }
        else
        {
            return;
        }

        TryLoadScene();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(player1Tag))
        {
            _player1Inside = false;
        }
        else if (other.CompareTag(player2Tag))
        {
            _player2Inside = false;
        }
    }

    private void TryLoadScene()
    {
        if (!_player1Inside || !_player2Inside)
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
}
