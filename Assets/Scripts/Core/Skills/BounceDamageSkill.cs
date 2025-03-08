using UnityEngine;

/// <summary>
/// Bounce damage skill that allows projectiles to bounce between enemies
/// </summary>
[CreateAssetMenu(fileName = "BounceDamageSkill", menuName = "Skills/Bounce Damage")]
public class BounceDamageSkill : SkillData
{
    [Header("Bounce Settings")]
    [SerializeField] private int _bounceCount = 1; // Default number of bounces
    [SerializeField] private int _rageBounceCount = 3; // Number of bounces in rage mode
    
    private int currentBounceCount;
    
    /// <summary>
    /// Returns the current number of bounces based on skill state
    /// </summary>
    public int GetBounceCount()
    {
        return currentBounceCount;
    }
    
    /// <summary>
    /// Applies the rage mode effect to the skill
    /// </summary>
    public override void ApplyRageEffect(bool rageActive)
    {
        base.ApplyRageEffect(rageActive);
        currentBounceCount = rageActive ? _rageBounceCount : _bounceCount;
    }
    
    /// <summary>
    /// Gets the base value of the skill effect (bounce count)
    /// </summary>
    /// <returns>The current bounce count as float</returns>
    public override float GetValue()
    {
        return GetBounceCount();
    }
} 