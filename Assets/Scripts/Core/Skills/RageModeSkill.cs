using UnityEngine;
using System;

/// <summary>
/// Rage mode skill that boosts the effects of all other active skills
/// </summary>
[CreateAssetMenu(fileName = "RageModeSkill", menuName = "Skills/Rage Mode")]
public class RageModeSkill : SkillData
{
    // State flag
    private bool _isRageModeActive = false;
    
    // Event triggered when rage mode state changes
    public event Action<bool> OnRageModeStateChanged;
    
    // Property to check if rage mode is currently active
    public bool IsRageModeActive => _isRageModeActive;

    
    /// <summary>
    /// Activates the skill
    /// </summary>
    public override void Activate()
    {
        base.Activate();
        ActivateRageMode();
    }
    
    /// <summary>
    /// Deactivates the skill
    /// </summary>
    public override void Deactivate()
    {
        base.Deactivate();
        DeactivateRageMode();
    }
    
    /// <summary>
    /// Activates rage mode
    /// </summary>
    private void ActivateRageMode()
    {
        _isRageModeActive = true;
        OnRageModeStateChanged?.Invoke(true);
    }
    
    /// <summary>
    /// Deactivates rage mode
    /// </summary>
    private void DeactivateRageMode()
    {
        _isRageModeActive = false;
        OnRageModeStateChanged?.Invoke(false);
    }
    
    /// <summary>
    /// Applies rage mode effect to this skill
    /// Base implementation does nothing specific
    /// </summary>
    public override void ApplyRageEffect(bool rageActive)
    {
        base.ApplyRageEffect(rageActive);
    }
    
    /// <summary>
    /// Gets the base value of the skill effect
    /// </summary>
    /// <returns>Always returns 1 as there is no cooldown anymore</returns>
    public override float GetValue()
    {
        return 1f;
    }
} 