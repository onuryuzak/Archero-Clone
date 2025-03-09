using UnityEngine;
using System;

/// <summary>
/// Base ScriptableObject class containing skill data
/// </summary>
public abstract class SkillData : ScriptableObject
{
    [Header("Basic Skill Information")]
    [SerializeField] private string _skillName;
    [SerializeField] private string _description;
    [SerializeField] private Sprite _icon;
    [SerializeField] private GameEnums.SkillType _skillType; // Multiplier for effect value when in rage mode
    
    // Properties
    public string SkillName => _skillName;
    public string Description => _description;
    public Sprite Icon => _icon;
    public GameEnums.SkillType SkillType => _skillType;
    
    // Active state
    private bool isActive = false;
    public bool IsActive => isActive;
    
    // Rage mode effect state
    protected bool _isRageActive = false;
    
    // Events called when skill is activated and deactivated (legacy approach - can still be used alongside GameEvents)
    public event Action<SkillData> OnSkillActivated;
    public event Action<SkillData> OnSkillDeactivated;
    
    /// <summary>
    /// Activates the skill
    /// </summary>
    public virtual void Activate()
    {
        isActive = true;
        
        // Trigger legacy event
        OnSkillActivated?.Invoke(this);
        
        // Trigger GameEvent
        GameEvents.OnSkillActivated?.Invoke(this);
        Debug.Log($"[SkillData] {_skillName} activated - OnSkillActivated event raised");
    }
    
    /// <summary>
    /// Deactivates the skill
    /// </summary>
    public virtual void Deactivate()
    {
        isActive = false;
        
        // Trigger legacy event
        OnSkillDeactivated?.Invoke(this);
        
        // Trigger GameEvent
        GameEvents.OnSkillDeactivated?.Invoke(this);
        Debug.Log($"[SkillData] {_skillName} deactivated - OnSkillDeactivated event raised");
    }
    
    /// <summary>
    /// Applies rage mode effect
    /// </summary>
    public virtual void ApplyRageEffect(bool rageActive)
    {
        _isRageActive = rageActive;
    }
    
    /// <summary>
    /// Gets the base value of the skill effect
    /// </summary>
    /// <returns>The float value representing the skill's effect strength</returns>
    public abstract float GetValue();
} 