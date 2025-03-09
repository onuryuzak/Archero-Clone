using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Component that enables projectiles to bounce between targets
/// </summary>
[RequireComponent(typeof(Projectile))]
public class BouncingProjectile : MonoBehaviour
{
    // References
    private Projectile _projectile;
    
    [Header("Bounce Settings")]
    [SerializeField] private float _bounceRange = 10f;
    [SerializeField] private LayerMask _targetLayers;
    
    // Bounce parameters
    private int _remainingBounces = 0;
    private float _damageFalloffPercentage = 0.25f;
    private float _originalDamage = 0f;
    private float _currentDamage;
    
    // Target tracking
    private List<GameObject> _hitTargets = new List<GameObject>();
    
    private void Awake()
    {
        _projectile = GetComponent<Projectile>();
        
        if (_projectile != null)
        {
            // Subscribe to the projectile's OnHit event
            _projectile.OnHit += OnProjectileHit;
            
            // Prevent projectile from destroying itself on collision 
            // to allow bouncing between targets
            _projectile.DestroyOnCollision = false;
        }
        
        // Set default target layers if not set
        if (_targetLayers == 0)
        {
            _targetLayers = LayerMask.GetMask("Enemy");
        }
    }
    
    private void OnDestroy()
    {
        if (_projectile != null)
        {
            _projectile.OnHit -= OnProjectileHit;
        }
    }
    
    /// <summary>
    /// Initialize the bouncing projectile with the given parameters
    /// </summary>
    public void Initialize(int bounceCount, float falloffPercentage, float baseDamage)
    {
        _remainingBounces = Mathf.Max(0, bounceCount);
        _damageFalloffPercentage = Mathf.Clamp01(falloffPercentage);
        _originalDamage = baseDamage;
        _currentDamage = baseDamage;
        
        // Clear hit targets when initializing
        _hitTargets.Clear();
        
        Debug.Log($"[BouncingProjectile] [ID:{GetInstanceID()}] Initialized with {_remainingBounces} bounces, damage:{_currentDamage}, falloff:{_damageFalloffPercentage}");
    }
    

    
    /// <summary>
    /// Called when the projectile hits something
    /// </summary>
    private void OnProjectileHit(GameObject hitObject, Vector3 hitPosition)
    {
        if (hitObject == null) return;
        
        Debug.Log($"[BouncingProjectile] [ID:{GetInstanceID()}] Hit object: {hitObject.name}, Remaining bounces: {_remainingBounces}");
        
        // Handle the hit if this is a valid target (on correct layer)
        if (((1 << hitObject.layer) & _targetLayers) != 0)
        {
            // If we're not already tracking this target, handle the hit
            if (!_hitTargets.Contains(hitObject))
            {
                HitTarget(hitObject, hitPosition);
            }
            else
            {
                Debug.Log($"[BouncingProjectile] [ID:{GetInstanceID()}] Already hit {hitObject.name}, ignoring");
            }
        }
        else
        {
            // If we hit something that's not a valid target, just continue
            Debug.Log($"[BouncingProjectile] [ID:{GetInstanceID()}] Hit non-target object on layer {LayerMask.LayerToName(hitObject.layer)}");
        }
    }
    
    /// <summary>
    /// Process a hit on a target
    /// </summary>
    private void HitTarget(GameObject hitObject, Vector3 hitPosition)
    {
        // Add to hit targets
        if (!_hitTargets.Contains(hitObject))
        {
            _hitTargets.Add(hitObject);
        }
        
        // Apply damage to the target if it's damageable
        IDamageable damageable = hitObject.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(_currentDamage);
            Debug.Log($"[BouncingProjectile] Applied damage: {_currentDamage} to {hitObject.name}");
        }
        
        // Calculate reduced damage for next hit
        _currentDamage = _originalDamage * (1f - (_damageFalloffPercentage * _hitTargets.Count));
        
        // Check if we should continue bouncing
        if (_remainingBounces > 0)
        {
            _remainingBounces--;
            Debug.Log($"[BouncingProjectile] [ID:{GetInstanceID()}] Bounces remaining: {_remainingBounces}");
            
            // Pause the projectile momentarily to prepare for bounce
            _projectile.PauseMovement();
            
            // Find next target
            StartCoroutine(FindAndBounceToNextTarget(hitPosition));
        }
        else
        {
            // No more bounces, allow the projectile to be destroyed on next collision
            if (_projectile != null)
            {
                Debug.Log($"[BouncingProjectile] [ID:{GetInstanceID()}] No more bounces, setting DestroyOnCollision=true");
                _projectile.DestroyOnCollision = true;
            }
        }
    }
    
    /// <summary>
    /// Find and bounce to the next target
    /// </summary>
    private IEnumerator FindAndBounceToNextTarget(Vector3 currentPosition)
    {
        // Small delay to ensure hit processing is complete
        yield return new WaitForSeconds(0.05f);
        
        // Find all potential targets in range
        Collider[] potentialTargets = Physics.OverlapSphere(currentPosition, _bounceRange, _targetLayers);
        
        // Filter out targets we've already hit
        List<GameObject> validTargets = new List<GameObject>();
        foreach (Collider col in potentialTargets)
        {
            if (col.gameObject != null && !_hitTargets.Contains(col.gameObject))
            {
                validTargets.Add(col.gameObject);
            }
        }
        
        Debug.Log($"[BouncingProjectile] [ID:{GetInstanceID()}] Found {validTargets.Count} valid targets");
        
        // If we have valid targets, select the closest one
        if (validTargets.Count > 0)
        {
            // Sort by distance
            validTargets.Sort((a, b) => 
                Vector3.Distance(currentPosition, a.transform.position)
                .CompareTo(Vector3.Distance(currentPosition, b.transform.position)));
            
            // Select closest target
            GameObject nextTarget = validTargets[0];
            
            // Calculate direction to target
            Vector3 direction = (nextTarget.transform.position - currentPosition).normalized;
            
            Debug.Log($"[BouncingProjectile] [ID:{GetInstanceID()}] Bouncing to target: {nextTarget.name}");
            
            // Temporarily ignore collisions
            StartCoroutine(TemporarilyIgnoreCollisions(0.2f));
            
            // Offset the position slightly to avoid getting stuck
            Vector3 offsetPosition = currentPosition + (direction * 0.5f);
            
            // Move the projectile to the offset position
            transform.position = offsetPosition;
            
            // Disable gravity for bounced projectiles
            if (_projectile != null)
            {
                _projectile.SetGravity(false);
                
                // Resume projectile movement with the new direction and original speed
                _projectile.Initialize(direction, _projectile.Speed, _currentDamage);
                _projectile.ResumeMovement();
            }
        }
        else
        {
            // No valid targets, let the projectile continue on its current path
            Debug.Log($"[BouncingProjectile] [ID:{GetInstanceID()}] No valid targets, continuing on current path");
            if (_projectile != null)
            {
                _projectile.ResumeMovement();
            }
        }
    }
    
    /// <summary>
    /// Temporarily ignore collisions by changing the layer
    /// </summary>
    private IEnumerator TemporarilyIgnoreCollisions(float duration)
    {
        // Cache original layer
        int originalLayer = gameObject.layer;
        
        // Change to ignore raycast layer
        int ignoreLayer = LayerMask.NameToLayer("Ignore Raycast");
        if (ignoreLayer >= 0)
        {
            gameObject.layer = ignoreLayer;
            
            // Wait for the specified duration
            yield return new WaitForSeconds(duration);
            
            // Restore original layer if the object still exists
            if (gameObject != null)
            {
                gameObject.layer = originalLayer;
            }
        }
    }
    
    /// <summary>
    /// Visualize the bounce range in the editor
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, _bounceRange);
    }
} 