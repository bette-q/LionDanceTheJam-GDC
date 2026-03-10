using UnityEngine;

public class EndingCheck : MonoBehaviour
{
    [SerializeField] private Transform _player;
    [SerializeField] private float _endingShowHeight = 2500;
    [SerializeField] private GameObject[] _endingGOs;
    
    private bool isEnded = false;
    
    // Update is called once per frame
    void Update()
    {
        if (!isEnded && _player.position.y > _endingShowHeight)
        {
            foreach (GameObject endingGO in _endingGOs)
            {
                endingGO.SetActive(true);
            }

            isEnded = true;
        }
    }
}
