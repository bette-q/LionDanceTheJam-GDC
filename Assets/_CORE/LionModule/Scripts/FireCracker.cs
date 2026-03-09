using System;
using DG.Tweening;
using UnityEngine;

public class FireCracker : MonoBehaviour
{
    [SerializeField] private Transform _collectVFX;
    private bool _isExploding = false;
    
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!_isExploding && other.CompareTag("Fire"))
        {
            _isExploding = true;
            transform.DOScale(1.4f, 0.35f)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    Destroy(gameObject);
                    Instantiate(_collectVFX, transform.position, Quaternion.identity);
                });
        }
    }
}
