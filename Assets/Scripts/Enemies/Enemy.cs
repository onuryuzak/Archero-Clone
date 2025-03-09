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
    private bool _isDead = false; // Ölüm durumunu takip etmek için
    
    public void Initialize()
    {
        _currentHealth = _maxHealth;
        _isDead = false;
        
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
        // Ölmüşse hasar almaya devam etmesin
        if (_isDead) return;
        
        _currentHealth -= damage;
        _currentHealth = Mathf.Max(0, _currentHealth);
        
        // Update health bar
        if (_healthBar != null)
        {
            _healthBar.UpdateHealth(_currentHealth);
        }
        
        // Check if dead
        if (_currentHealth <= 0 && !_isDead)
        {
            Die();
        }
    }
    
    private void Die()
    {
        // Birden fazla kez ölme olayının tetiklenmesini önle
        if (_isDead) return;
        _isDead = true;
        
        // Trigger GameEvent
        GameEvents.OnEnemyDefeated?.Invoke(this);
        Debug.Log($"[Enemy] {gameObject.name} defeated - OnEnemyDefeated event raised");
        
        // Play death animation or particle effect here
        
        // Destroy game object after a small delay to allow event processing
        Destroy(gameObject, 0.1f);
    }
} 