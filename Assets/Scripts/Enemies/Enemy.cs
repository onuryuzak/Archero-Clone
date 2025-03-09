using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable
{
    [Header("Enemy Settings")]
    [SerializeField] private float _maxHealth = 100f;
    
    private float _currentHealth;
    private HealthBar _healthBar;
    
    public void Initialize()
    {
        _currentHealth = _maxHealth;
        // If a health bar reference is provided, initialize it
        if (_healthBar != null)
        {
            _healthBar.Initialize(_maxHealth, transform);
        }
        // If no health bar reference, try to find one in children
        else
        {
            _healthBar = GetComponentInChildren<HealthBar>();
            if (_healthBar != null)
            {
                _healthBar.Initialize(_maxHealth, transform);
            }
        }
        
        // Trigger enemy spawned event
        GameEvents.OnEnemySpawned?.Invoke(this);
    }
    
    public void TakeDamage(float damage)
    {
        _currentHealth -= damage;
        _currentHealth = Mathf.Max(0, _currentHealth);
        
        // Update health bar
        if (_healthBar != null)
        {
            _healthBar.UpdateHealth(_currentHealth);
        }
        
        // Check if dead
        if (_currentHealth <= 0)
        {
            Die();
        }
    }
    
    private void Die()
    {
        
        // Trigger GameEvent
        GameEvents.OnEnemyDefeated?.Invoke(this);
        Debug.Log($"[Enemy] {gameObject.name} defeated - OnEnemyDefeated event raised");
        
        // Play death animation or particle effect here
        
        // Destroy game object
        Destroy(gameObject);
    }
} 