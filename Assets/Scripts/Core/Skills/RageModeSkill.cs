using UnityEngine;
using System;

/// <summary>
/// Rage mode skill that boosts the effects of all other active skills
/// </summary>
[CreateAssetMenu(fileName = "RageModeSkill", menuName = "Skills/Rage Mode")]
public class RageModeSkill : SkillData
{
    [Header("Rage Mode Settings")]
    [SerializeField] private float _rageDuration = 10f; // Duration of rage mode in seconds
    [SerializeField] private float _cooldown = 30f; // Cooldown after rage mode ends
    
    // Current timers
    private float _currentRageDuration = 0f;
    private float _currentCooldownTime = 0f;
    
    // State flags
    private bool _isRageModeActive = false;
    private bool _isOnCooldown = false;
    
    // Event triggered when rage mode state changes
    public event Action<bool> OnRageModeStateChanged;
    
    // Property to check if rage mode is currently active
    public bool IsRageModeActive => _isRageModeActive;
    
    // Property to check if skill is on cooldown
    public bool IsOnCooldown => _isOnCooldown;
    
    /// <summary>
    /// Updates the rage mode timers
    /// </summary>
    public void UpdateRageMode(float deltaTime)
    {
        if (!IsActive) return;
        
        if (_isRageModeActive)
        {
            // If rage mode is active, count down the duration
            _currentRageDuration -= deltaTime;
            
            if (_currentRageDuration <= 0f)
            {
                // When rage mode ends, start cooldown
                _isRageModeActive = false;
                _isOnCooldown = true;
                _currentCooldownTime = _cooldown;
                
                OnRageModeStateChanged?.Invoke(false);
            }
        }
        else if (_isOnCooldown)
        {
            // If on cooldown, count down the cooldown timer
            _currentCooldownTime -= deltaTime;
            
            if (_currentCooldownTime <= 0f)
            {
                // When cooldown ends, rage mode can be activated again
                _isOnCooldown = false;
                
                // Start rage mode again if skill is still active
                ActivateRageMode();
            }
        }
        else
        {
            // If neither active nor on cooldown, activate rage mode
            ActivateRageMode();
        }
    }
    
    /// <summary>
    /// Activates rage mode
    /// </summary>
    private void ActivateRageMode()
    {
        _isRageModeActive = true;
        _currentRageDuration = _rageDuration;
        OnRageModeStateChanged?.Invoke(true);
    }
    
    /// <summary>
    /// Forcefully activates rage mode (used for testing)
    /// </summary>
    /// <param name="active">Whether rage mode should be active</param>
    /// <param name="resetTimer">Whether to reset the timer to full duration</param>
    public void ForceRageMode(bool active, bool resetTimer = true)
    {
        _isRageModeActive = active;
        _isOnCooldown = false;
        
        if (active && resetTimer)
        {
            _currentRageDuration = _rageDuration;
        }
        
        OnRageModeStateChanged?.Invoke(active);
        Debug.Log($"[RageModeSkill] Rage mode forcefully set to {(active ? "ACTIVE" : "INACTIVE")}");
    }
    
    /// <summary>
    /// Deactivates the skill and resets its state
    /// </summary>
    public override void Deactivate()
    {
        base.Deactivate();
        _isRageModeActive = false;
        _isOnCooldown = false;
        _currentRageDuration = 0f;
        _currentCooldownTime = 0f;
        OnRageModeStateChanged?.Invoke(false);
    }
    
    /// <summary>
    /// Gets the base value of the skill effect (1 if active, 0 if inactive)
    /// </summary>
    /// <returns>1 if rage mode is active, 0 if inactive</returns>
    public override float GetValue()
    {
        return IsRageModeActive ? 1f : 0f;
    }
} 