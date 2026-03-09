using System;
using UnityEngine;

public class Collectable : MonoBehaviour
{
    [SerializeField] private Transform _collectVFX;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Destroy(gameObject);
            Instantiate(_collectVFX, transform.position, Quaternion.identity);
        }
    }
}
