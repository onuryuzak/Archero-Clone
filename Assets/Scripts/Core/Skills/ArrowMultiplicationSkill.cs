using UnityEngine;

/// <summary>
/// Arrow multiplication skill that increases the number of arrows fired
/// </summary>
[CreateAssetMenu(fileName = "ArrowMultiplicationSkill", menuName = "Skills/Arrow Multiplication")]
public class ArrowMultiplicationSkill : SkillData
{
    [Header("Skill Settings")]
    [SerializeField] private int _arrowCount = 2; // Default number of arrows
    [SerializeField] private int _rageArrowCount = 4; // Number of arrows in rage mode
    
    private int currentArrowCount;
    
    /// <summary>
    /// Returns the current number of arrows based on skill state
    /// </summary>
    public int GetArrowCount()
    {
        return currentArrowCount;
    }
    
    /// <summary>
    /// Applies the rage mode effect to the skill
    /// </summary>
    public override void ApplyRageEffect(bool rageActive)
    {
        base.ApplyRageEffect(rageActive);
        currentArrowCount = rageActive ? _rageArrowCount : _arrowCount;
    }
    
    /// <summary>
    /// Gets the base value of the skill effect (arrow count)
    /// </summary>
    /// <returns>The current arrow count as float</returns>
    public override float GetValue()
    {
        return GetArrowCount();
    }
} 