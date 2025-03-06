using UnityEngine;
using System.Collections;

/// <summary>
/// Camera controller for Archero-style games.
/// Provides top-down following behavior with room transitions and zoom capabilities.
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform _target;
    [SerializeField] private Vector3 _offset = new Vector3(0, 15, -8); // Default Archero-like position
    [SerializeField] private float _smoothSpeed = 5f; // Higher values = faster camera movement
    
    [Header("Room Transition")]
    [SerializeField] private float _transitionSpeed = 3f;
    [SerializeField] private float _roomTransitionDuration = 0.75f;
    
    [Header("Zoom Settings")]
    [SerializeField] private float _defaultFOV = 60f;
    [SerializeField] private float _zoomInFOV = 40f;
    [SerializeField] private float _zoomOutFOV = 75f;
    [SerializeField] private float _zoomDuration = 0.5f;
    
    [Header("Boundaries")]
    [SerializeField] private bool _useBoundaries = false;
    [SerializeField] private float _minX = -50f;
    [SerializeField] private float _maxX = 50f;
    [SerializeField] private float _minZ = -50f;
    [SerializeField] private float _maxZ = 50f;
    
    private Vector3 _currentVelocity = Vector3.zero;
    private Camera _mainCamera;
    private Vector3 _targetPosition;
    private bool _isTransitioning = false;
    private Vector3 _transitionTarget;
    
    private void Awake()
    {
        _mainCamera = GetComponent<Camera>();
        
        if (_target == null)
        {
            // Try to find the player automatically
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _target = player.transform;
                Debug.Log("Camera target automatically set to Player");
            }
            else
            {
                Debug.LogWarning("No target assigned to CameraController and no Player found.");
            }
        }
        
        // Initialize starting position if we have a target
        if (_target != null)
        {
            transform.position = _target.position + _offset;
            transform.LookAt(_target.position);
        }
    }
    
    private void LateUpdate()
    {
        if (_isTransitioning)
        {
            // Don't follow the player while transitioning between rooms
            return;
        }
        
        if (_target == null)
        {
            return;
        }
        
        FollowTarget();
    }
    
    /// <summary>
    /// Follows the target with smooth damping
    /// </summary>
    private void FollowTarget()
    {
        // Calculate target position
        _targetPosition = _target.position + _offset;
        
        // Apply boundaries if enabled
        if (_useBoundaries)
        {
            _targetPosition.x = Mathf.Clamp(_targetPosition.x, _minX, _maxX);
            _targetPosition.z = Mathf.Clamp(_targetPosition.z, _minZ, _maxZ);
        }
        
        // Smoothly move towards the target
        transform.position = Vector3.SmoothDamp(transform.position, _targetPosition, ref _currentVelocity, 1f / _smoothSpeed);
        
        // Keep the camera looking at the target or slightly ahead of it
        transform.LookAt(_target.position);
    }
    
    /// <summary>
    /// Transitions the camera to a new room or position
    /// </summary>
    /// <param name="newRoomCenter">The center point of the new room</param>
    public void TransitionToRoom(Vector3 newRoomCenter)
    {
        if (_isTransitioning)
        {
            return;
        }
        
        StartCoroutine(TransitionCoroutine(newRoomCenter));
    }
    
    private IEnumerator TransitionCoroutine(Vector3 newRoomCenter)
    {
        _isTransitioning = true;
        
        // Save original values
        Vector3 startPosition = transform.position;
        Vector3 roomPosition = newRoomCenter + new Vector3(0, _offset.y, _offset.z);
        
        // First, zoom out slightly for a better view of the transition
        float originalFOV = _mainCamera.fieldOfView;
        float targetFOV = _zoomOutFOV;
        
        // Zoom out
        float zoomStartTime = Time.time;
        while (Time.time < zoomStartTime + _zoomDuration * 0.3f)
        {
            float t = (Time.time - zoomStartTime) / (_zoomDuration * 0.3f);
            _mainCamera.fieldOfView = Mathf.Lerp(originalFOV, targetFOV, t);
            yield return null;
        }
        
        // Move to the new room
        float startTime = Time.time;
        float journeyLength = Vector3.Distance(startPosition, roomPosition);
        float distanceCovered = 0;
        
        while (distanceCovered < journeyLength)
        {
            float timeProgress = (Time.time - startTime) / _roomTransitionDuration;
            float smoothProgress = Mathf.SmoothStep(0, 1, timeProgress);
            
            transform.position = Vector3.Lerp(startPosition, roomPosition, smoothProgress);
            distanceCovered = Vector3.Distance(startPosition, transform.position);
            
            yield return null;
        }
        
        // Zoom back to normal
        zoomStartTime = Time.time;
        while (Time.time < zoomStartTime + _zoomDuration * 0.3f)
        {
            float t = (Time.time - zoomStartTime) / (_zoomDuration * 0.3f);
            _mainCamera.fieldOfView = Mathf.Lerp(targetFOV, _defaultFOV, t);
            yield return null;
        }
        
        // Reset transition state
        _isTransitioning = false;
        
        // Update the target reference if needed
        // You might need to update the target reference here if the player has teleported
    }
    
    /// <summary>
    /// Zooms the camera in for dramatic effect
    /// </summary>
    public void ZoomIn()
    {
        StartCoroutine(ZoomCoroutine(_zoomInFOV));
    }
    
    /// <summary>
    /// Zooms the camera out to show more of the surroundings
    /// </summary>
    public void ZoomOut()
    {
        StartCoroutine(ZoomCoroutine(_zoomOutFOV));
    }
    
    /// <summary>
    /// Resets the camera zoom to default
    /// </summary>
    public void ResetZoom()
    {
        StartCoroutine(ZoomCoroutine(_defaultFOV));
    }
    
    /// <summary>
    /// Handles the smooth zoom transition
    /// </summary>
    private IEnumerator ZoomCoroutine(float targetFOV)
    {
        float startFOV = _mainCamera.fieldOfView;
        float startTime = Time.time;
        
        while (Time.time < startTime + _zoomDuration)
        {
            float t = (Time.time - startTime) / _zoomDuration;
            _mainCamera.fieldOfView = Mathf.Lerp(startFOV, targetFOV, t);
            yield return null;
        }
        
        _mainCamera.fieldOfView = targetFOV;
    }
    
    /// <summary>
    /// Shakes the camera for impact effect
    /// </summary>
    /// <param name="duration">Duration of the shake effect</param>
    /// <param name="magnitude">Strength of the shake</param>
    public void ShakeCamera(float duration = 0.2f, float magnitude = 0.5f)
    {
        StartCoroutine(ShakeCameraCoroutine(duration, magnitude));
    }
    
    private IEnumerator ShakeCameraCoroutine(float duration, float magnitude)
    {
        Vector3 originalPosition = transform.position;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float z = Random.Range(-1f, 1f) * magnitude;
            
            transform.position = originalPosition + new Vector3(x, 0, z);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        transform.position = originalPosition;
    }
    
    /// <summary>
    /// Sets a new target for the camera to follow
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        _target = newTarget;
    }
    
    /// <summary>
    /// Updates the camera's offset from target
    /// </summary>
    public void SetOffset(Vector3 newOffset)
    {
        _offset = newOffset;
    }
    
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // Draw the camera's field of view
        Gizmos.color = Color.yellow;
        if (_target != null)
        {
            Gizmos.DrawLine(transform.position, _target.position);
            Gizmos.DrawSphere(_target.position, 0.5f);
        }
        
        // Draw boundaries if enabled
        if (_useBoundaries)
        {
            Gizmos.color = Color.red;
            Vector3 center = new Vector3((_minX + _maxX) / 2, 0, (_minZ + _maxZ) / 2);
            Vector3 size = new Vector3(_maxX - _minX, 0.1f, _maxZ - _minZ);
            Gizmos.DrawWireCube(center, size);
        }
    }
#endif
} 