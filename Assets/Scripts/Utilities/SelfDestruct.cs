using UnityEngine;

/// <summary>
/// Simple utility component that destroys the GameObject after a specified lifetime.
/// Useful for visual effects and temporary objects.
/// </summary>
public class SelfDestruct : MonoBehaviour
{
    [SerializeField] private float _lifetime = 2f;
    [SerializeField] private bool _scaleDownBeforeDestroy = false;
    [SerializeField] private float _scaleDownDuration = 0.5f;
    
    private float _elapsedTime = 0f;
    private bool _isScalingDown = false;
    private Vector3 _originalScale;
    
    private void Start()
    {
        _originalScale = transform.localScale;
    }
    
    private void Update()
    {
        _elapsedTime += Time.deltaTime;
        
        // If scale down is enabled and it's time to start scaling down
        if (_scaleDownBeforeDestroy && !_isScalingDown && _elapsedTime >= (_lifetime - _scaleDownDuration))
        {
            _isScalingDown = true;
        }
        
        // Handle scale down effect
        if (_isScalingDown)
        {
            float remainingTime = _lifetime - _elapsedTime;
            float scalePercent = Mathf.Max(0, remainingTime / _scaleDownDuration);
            transform.localScale = _originalScale * scalePercent;
        }
        
        // Destroy when lifetime is reached
        if (_elapsedTime >= _lifetime)
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Set the lifetime programmatically
    /// </summary>
    public void SetLifetime(float lifetime)
    {
        _lifetime = lifetime;
    }
} 