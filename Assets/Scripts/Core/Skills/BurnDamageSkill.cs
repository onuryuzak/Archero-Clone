using UnityEngine;

/// <summary>
/// Burn damage skill that applies a damage over time effect to enemies
/// </summary>
[CreateAssetMenu(fileName = "BurnDamageSkill", menuName = "Skills/Burn Damage")]

public class BurnDamageSkill : SkillData
{
    [Header("Burn Settings")]
    [SerializeField] private float _burnDamagePerSecond = 5f; // Damage per second
    [SerializeField] private float _burnDuration = 3f; // Default duration in seconds
    [SerializeField] private float _rageBurnDuration = 6f; // Duration in rage mode
    [SerializeField] private int _maxStacks = 3; // Maximum number of burn stacks that can be applied
    
    private float currentBurnDuration;
    
    /// <summary>
    /// Returns the burn damage per second
    /// </summary>
    public float GetBurnDamagePerSecond()
    {
        return _burnDamagePerSecond;
    }
    
    /// <summary>
    /// Returns the current burn duration based on skill state
    /// </summary>
    public float GetBurnDuration()
    {
        return currentBurnDuration;
    }
    
    /// <summary>
    /// Returns the total burn damage (damage per second * duration)
    /// </summary>
    public float GetTotalBurnDamage()
    {
        return _burnDamagePerSecond * currentBurnDuration;
    }
    
    /// <summary>
    /// Returns the maximum number of burn stacks that can be applied
    /// </summary>
    public int GetMaxStacks()
    {
        return _maxStacks;
    }
    
    /// <summary>
    /// Applies the rage mode effect to the skill
    /// </summary>
    public override void ApplyRageEffect(bool rageActive)
    {
        base.ApplyRageEffect(rageActive);
        currentBurnDuration = rageActive ? _rageBurnDuration : _burnDuration;
    }

    public override float GetValue()
    {
        return 0;
    }
} 