using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable
{
    [Header("Enemy Settings")]
    [SerializeField] private float _maxHealth = 100f;
    [SerializeField] private GameObject _healthBarPrefab;
    [SerializeField] private Vector3 _healthBarOffset = new Vector3(0, 1.5f, 0);
    
    private float _currentHealth;
    private HealthBar _healthBar;
    
    // Event for when enemy is defeated
    public event Action<Enemy> OnEnemyDefeated;
    
    public void Initialize()
    {
        _currentHealth = _maxHealth;
        InitializeHealthBar();
    }
    
    private void InitializeHealthBar()
    {
        if (_healthBarPrefab != null)
        {
            GameObject healthBarObj = Instantiate(_healthBarPrefab, transform.position + _healthBarOffset, Quaternion.identity);
            healthBarObj.transform.SetParent(transform);
            _healthBar = healthBarObj.GetComponent<HealthBar>();
            
            if (_healthBar != null)
            {
                _healthBar.Initialize(_maxHealth);
            }
            else
            {
                Debug.LogError("HealthBar component not found on healthBarPrefab!");
            }
        }
    }
    
    public void TakeDamage(float damageAmount)
    {
        if (_currentHealth <= 0) return;
        
        _currentHealth -= damageAmount;
        
        // Update health bar
        if (_healthBar != null)
        {
            _healthBar.UpdateHealthBar(_currentHealth, _maxHealth);
        }
        
        if (_currentHealth <= 0)
        {
            Die();
        }
    }
    
    private void Die()
    {
        // Trigger event
        OnEnemyDefeated?.Invoke(this);
        
        // Play death animation or particle effect here
        
        // Destroy game object
        Destroy(gameObject);
    }
} 