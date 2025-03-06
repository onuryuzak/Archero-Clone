using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Component that enables projectiles to bounce between targets
/// </summary>
[RequireComponent(typeof(Projectile))]
public class BouncingProjectile : MonoBehaviour
{
    private int _remainingBounces = 0;
    private float _damageFalloffPercentage = 0.25f;
    private float _originalDamage = 0f;
    private Projectile _projectile;
    
    // Store hit targets to avoid bouncing to the same target
    private List<GameObject> _hitTargets = new List<GameObject>();
    
    // Bounce range - how far to look for targets to bounce to
    [SerializeField] private float _bounceRange = 10f;
    [SerializeField] private LayerMask _targetLayers; // Layers of potential bounce targets
    
    private void Awake()
    {
        _projectile = GetComponent<Projectile>();
        
        // Subscribe to the projectile's OnHit event
        if (_projectile != null)
        {
            _projectile.OnHit += HandleHit;
        }
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from the event when destroyed
        if (_projectile != null)
        {
            _projectile.OnHit -= HandleHit;
        }
    }
    
    /// <summary>
    /// Initializes the bouncing projectile with bounce parameters
    /// </summary>
    /// <param name="bounceCount">Number of bounces</param>
    /// <param name="falloffPercentage">Damage reduction per bounce (0-1)</param>
    /// <param name="baseDamage">Original projectile damage</param>
    public void Initialize(int bounceCount, float falloffPercentage, float baseDamage)
    {
        _remainingBounces = bounceCount;
        _damageFalloffPercentage = Mathf.Clamp01(falloffPercentage);
        _originalDamage = baseDamage;
        
        // Set target layers if not already set
        if (_targetLayers == 0)
        {
            // Default to Enemy layer
            _targetLayers = LayerMask.GetMask("Enemy");
        }
    }
    
    /// <summary>
    /// Handles the hit event from the projectile
    /// </summary>
    /// <param name="hitObject">The object that was hit</param>
    /// <param name="hitPosition">Position where the hit occurred</param>
    private void HandleHit(GameObject hitObject, Vector3 hitPosition)
    {
        // Add hit object to the list
        if (hitObject != null && !_hitTargets.Contains(hitObject))
        {
            _hitTargets.Add(hitObject);
        }
        
        // Try to bounce if we have remaining bounces
        if (_remainingBounces > 0)
        {
            BounceToNextTarget(hitPosition);
        }
        
        // Destroy this projectile after processing the hit
        Destroy(gameObject);
    }
    
    /// <summary>
    /// Finds the next target and bounces to it
    /// </summary>
    /// <param name="hitPosition">Position to bounce from</param>
    private void BounceToNextTarget(Vector3 hitPosition)
    {
        // Find all potential targets in range
        Collider[] potentialTargets = Physics.OverlapSphere(hitPosition, _bounceRange, _targetLayers);
        GameObject bestTarget = null;
        float closestDistance = float.MaxValue;
        
        // Find the closest target that hasn't been hit yet
        foreach (Collider col in potentialTargets)
        {
            // Skip targets we've already hit
            if (_hitTargets.Contains(col.gameObject))
                continue;
                
            float distance = Vector3.Distance(hitPosition, col.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                bestTarget = col.gameObject;
            }
        }
        
        // If we found a valid target, create a new projectile to bounce to it
        if (bestTarget != null)
        {
            // Calculate new damage with falloff
            float newDamage = _originalDamage * (1f - (_damageFalloffPercentage * (_hitTargets.Count)));
            
            // Get direction to the new target
            Vector3 direction = (bestTarget.transform.position - hitPosition).normalized;
            
            // Create a new projectile
            GameObject newProjectile = Instantiate(gameObject, hitPosition, Quaternion.LookRotation(direction));
            
            // Get and configure the new bouncing component
            BouncingProjectile bouncer = newProjectile.GetComponent<BouncingProjectile>();
            if (bouncer != null)
            {
                // Copy the hit targets to avoid hitting the same targets
                bouncer._hitTargets = new List<GameObject>(_hitTargets);
                
                // Reduce remaining bounces and initialize
                bouncer.Initialize(_remainingBounces - 1, _damageFalloffPercentage, _originalDamage);
            }
            
            // Configure the new projectile
            Projectile proj = newProjectile.GetComponent<Projectile>();
            if (proj != null)
            {
                proj.Initialize(direction, _projectile.Speed, newDamage);
                
                // Prevent the OnHit event from firing immediately - set to Ignore Raycast layer if exists
                int ignoreRaycastLayer = LayerMask.NameToLayer("Ignore Raycast");
                if (ignoreRaycastLayer >= 0 && ignoreRaycastLayer <= 31) {
                    proj.gameObject.layer = ignoreRaycastLayer;
                } else {
                    Debug.LogWarning("'Ignore Raycast' layer not found. This is a built-in Unity layer and should exist.");
                }
                
                // Reset the layer after a short delay to prevent hitting the same target
                proj.StartCoroutine(DelayedLayerReset(proj.gameObject));
            }
        }
    }
    
    /// <summary>
    /// Coroutine to reset the layer after a short delay
    /// </summary>
    private System.Collections.IEnumerator DelayedLayerReset(GameObject obj)
    {
        // Wait a small amount of time to ensure we don't immediately hit
        yield return new WaitForSeconds(0.1f);
        
        // Reset to the default projectile layer
        if (obj != null)
        {
            int projectileLayer = LayerMask.NameToLayer("Projectile");
            if (projectileLayer >= 0 && projectileLayer <= 31) {
                obj.layer = projectileLayer;
            } else {
                // "Projectile" layer tanımlı değilse varsayılan layer kullanılır
                Debug.LogWarning("\"Projectile\" layer not found. Using \"Default\" layer instead. Consider adding a \"Projectile\" layer in Project Settings.");
                obj.layer = LayerMask.NameToLayer("Default");
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