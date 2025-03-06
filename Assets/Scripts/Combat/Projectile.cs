using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Projectile class that handles movement, collision, and damage application
/// </summary>
public class Projectile : MonoBehaviour
{
    [Header("Physics Settings")]
    [SerializeField] private float _gravity = 9.8f;
    [SerializeField] private float _maxLifetime = 5f;
    [SerializeField] private LayerMask _collisionLayers;
    
    [Header("Visual Effects")]
    [SerializeField] private GameObject _hitEffectPrefab;
    
    private Vector3 _initialVelocity;
    private float _damage;
    private float _elapsedTime;
    private bool _isInitialized;
    
    // Hit event - other components can subscribe to this
    public event Action<GameObject, Vector3> OnHit;
    
    // Properties
    public float Damage => _damage;
    public float Speed => _initialVelocity.magnitude;
    
    public void Initialize(Vector3 direction, float speed, float damageAmount)
    {
        // Calculate initial velocity based on direction and speed
        _initialVelocity = direction * speed;
        _damage = damageAmount;
        _elapsedTime = 0f;
        _isInitialized = true;
        
        // Rotate to face the initial direction
        transform.rotation = Quaternion.LookRotation(direction);
    }
    
    private void Update()
    {
        if (!_isInitialized) return;
        
        // Update elapsed time
        _elapsedTime += Time.deltaTime;
        
        // Calculate position based on physics
        Vector3 currentPosition = transform.position;
        Vector3 previousPosition = currentPosition;
        
        // Apply gravity and calculate new position
        // s = u*t + 0.5*a*t^2
        Vector3 newPosition = transform.position + _initialVelocity * Time.deltaTime;
        newPosition.y -= 0.5f * _gravity * Time.deltaTime * Time.deltaTime;
        
        // Update velocity due to gravity
        _initialVelocity.y -= _gravity * Time.deltaTime;
        
        // Check for collision using raycast
        Vector3 direction = newPosition - currentPosition;
        float distance = direction.magnitude;
        
        RaycastHit hit;
        if (Physics.Raycast(currentPosition, direction.normalized, out hit, distance, _collisionLayers))
        {
            HandleCollision(hit);
            return;
        }
        
        // Update position
        transform.position = newPosition;
        
        // Update rotation to follow trajectory
        if (_initialVelocity.sqrMagnitude > 0.1f)
        {
            transform.rotation = Quaternion.LookRotation(_initialVelocity.normalized);
        }
        
        // Destroy if lifetime exceeds maximum
        if (_elapsedTime >= _maxLifetime)
        {
            Destroy(gameObject);
        }
    }
    
    private void HandleCollision(RaycastHit hit)
    {
        // Check if the hit object has a damageable component
        IDamageable damageable = hit.collider.GetComponent<IDamageable>();
        
        if (damageable != null)
        {
            // Apply damage
            damageable.TakeDamage(_damage);
            
            // Check for burn effect
            ApplyBurnEffect(hit.collider.gameObject);
        }
        
        // Trigger OnHit event
        OnHit?.Invoke(hit.collider.gameObject, hit.point);
        
        // Spawn hit effect
        if (_hitEffectPrefab != null)
        {
            Instantiate(_hitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
        }
        
        // Destroy the projectile
        Destroy(gameObject);
    }
    
    /// <summary>
    /// Applies burn effect to the target if burn damage skill is active
    /// </summary>
    private void ApplyBurnEffect(GameObject target)
    {
        // Find skill system
        PlayerSkillSystem skillSystem = FindObjectOfType<PlayerSkillSystem>();
        if (skillSystem == null) return;
        
        // Check for burn damage skill
        BurnDamageSkill burnSkill = skillSystem.GetSkill<BurnDamageSkill>();
        if (burnSkill != null && burnSkill.IsActive)
        {
            // Get or add BurnEffect component to the target
            BurnEffect burnEffect = target.GetComponent<BurnEffect>();
            if (burnEffect == null)
            {
                burnEffect = target.AddComponent<BurnEffect>();
                burnEffect.SetMaxStacks(burnSkill.GetMaxStacks());
            }
            
            // Apply burn effect
            burnEffect.AddBurnStack(
                burnSkill.GetBurnDuration(),
                burnSkill.GetBurnDamagePerSecond()
            );
        }
    }
} 