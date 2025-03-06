using UnityEngine;

/// <summary>
/// Attack speed skill that increases the player's attack rate
/// </summary>
[CreateAssetMenu(fileName = "AttackSpeedSkill", menuName = "Skills/Attack Speed")]
public class AttackSpeedSkill : SkillData
{
    [Header("Speed Settings")]
    [SerializeField] private float _speedMultiplier = 2f; // Default speed multiplier
    [SerializeField] private float _rageSpeedMultiplier = 4f; // Speed multiplier in rage mode
    
    private float currentSpeedMultiplier;
    
    /// <summary>
    /// Returns the current speed multiplier based on skill state
    /// </summary>
    public float GetSpeedMultiplier()
    {
        return currentSpeedMultiplier;
    }
    
    /// <summary>
    /// Applies the rage mode effect to the skill
    /// </summary>
    public override void ApplyRageEffect(bool rageActive)
    {
        base.ApplyRageEffect(rageActive);
        currentSpeedMultiplier = rageActive ? _rageSpeedMultiplier : _speedMultiplier;
    }

    public override float GetValue()
    {
        return GetSpeedMultiplier();
    }
} 