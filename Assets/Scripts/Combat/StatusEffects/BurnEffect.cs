using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Burn status effect applied to enemies
/// </summary>
public class BurnEffect : MonoBehaviour
{
    [SerializeField] private GameObject _burnVfxPrefab;
    
    private Enemy _targetEnemy;
    private List<BurnStack> _activeStacks = new List<BurnStack>();
    private int _maxStacks = 3;
    
    private GameObject _activeVfx;
    
    private void Awake()
    {
        _targetEnemy = GetComponent<Enemy>();
    }
    
    private void Update()
    {
        // Process all active burn effects
        for (int i = _activeStacks.Count - 1; i >= 0; i--)
        {
            BurnStack stack = _activeStacks[i];
            
            // Decrease remaining duration
            stack.remainingDuration -= Time.deltaTime;
            
            // Check if it's time to apply damage
            stack.timeSinceLastTick += Time.deltaTime;
            if (stack.timeSinceLastTick >= 1f) // Apply damage every second
            {
                ApplyBurnDamage(stack.damagePerSecond);
                stack.timeSinceLastTick = 0f;
            }
            
            // Remove stack if duration has ended
            if (stack.remainingDuration <= 0f)
            {
                _activeStacks.RemoveAt(i);
            }
        }
        
        // Remove VFX if no active stacks remain
        if (_activeStacks.Count == 0 && _activeVfx != null)
        {
            Destroy(_activeVfx);
            _activeVfx = null;
        }
    }
    
    /// <summary>
    /// Adds a new burn effect stack
    /// </summary>
    public void AddBurnStack(float duration, float damagePerSecond)
    {
        // Check stack limit
        if (_activeStacks.Count >= _maxStacks)
        {
            // Find the stack with shortest remaining duration
            int shortestIndex = 0;
            float shortestTime = float.MaxValue;
            
            for (int i = 0; i < _activeStacks.Count; i++)
            {
                if (_activeStacks[i].remainingDuration < shortestTime)
                {
                    shortestTime = _activeStacks[i].remainingDuration;
                    shortestIndex = i;
                }
            }
            
            // Replace the shortest duration stack with the new one
            _activeStacks[shortestIndex] = new BurnStack
            {
                remainingDuration = duration,
                damagePerSecond = damagePerSecond,
                timeSinceLastTick = 0f
            };
        }
        else
        {
            // Add new stack
            _activeStacks.Add(new BurnStack
            {
                remainingDuration = duration,
                damagePerSecond = damagePerSecond,
                timeSinceLastTick = 0f
            });
        }
        
        // Create VFX if it doesn't exist
        if (_activeVfx == null && _burnVfxPrefab != null)
        {
            _activeVfx = Instantiate(_burnVfxPrefab, transform);
        }
    }
    
    /// <summary>
    /// Applies burn damage
    /// </summary>
    private void ApplyBurnDamage(float damage)
    {
        if (_targetEnemy != null)
        {
            _targetEnemy.TakeDamage(damage);
        }
    }
    
    /// <summary>
    /// Sets the maximum number of stacks
    /// </summary>
    public void SetMaxStacks(int max)
    {
        _maxStacks = Mathf.Max(1, max);
    }
    
    /// <summary>
    /// Burn stack data
    /// </summary>
    private class BurnStack
    {
        public float remainingDuration;
        public float damagePerSecond;
        public float timeSinceLastTick;
    }
} 