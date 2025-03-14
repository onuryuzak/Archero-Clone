using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// System that manages player skills
/// </summary>
public class PlayerSkillSystem : MonoBehaviour
{
    [Header("Skills")]
    [SerializeField] private SkillData[] _availableSkills;
    
    [Header("References")]
    [SerializeField] private WeaponController _weaponController;
    
    private Dictionary<GameEnums.SkillType, SkillData> _skillMap = new Dictionary<GameEnums.SkillType, SkillData>();
    
    private void Awake()
    {
        if (_weaponController == null)
            _weaponController = GetComponentInChildren<WeaponController>();
        
        InitializeSkills();
        
        // Oyun başlangıcında tüm becerileri pasif duruma getir
        ResetAllSkills();
    }
    
    private void Update()
    {
        // Rage beceriyi güncelle
        UpdateRageSkill(Time.deltaTime);
        
        // If attack speed skill is active, update the attack speed
        UpdateAttackSpeed();
    }
    
    /// <summary>
    /// Initialize skills
    /// </summary>
    private void InitializeSkills()
    {
        // Add skills to the map
        _skillMap.Clear();
        foreach (var skill in _availableSkills)
        {
            if (skill != null)
            {
                _skillMap[skill.SkillType] = skill;
                
                // Subscribe to skill events
                skill.OnSkillActivated += OnSkillActivated;
                skill.OnSkillDeactivated += OnSkillDeactivated;
            }
        }
    }
    
    /// <summary>
    /// Activates a skill
    /// </summary>
    public void ToggleSkill(GameEnums.SkillType skillType)
    {
        if (_skillMap.TryGetValue(skillType, out SkillData skill))
        {
            if (skill.IsActive)
            {
                skill.Deactivate(); // If skill is active, deactivate it
            }
            else
            {
                skill.Activate(); // If skill is passive, activate it
            }
        }
    }
    
    /// <summary>
    /// Called when a skill is activated
    /// </summary>
    private void OnSkillActivated(SkillData skill)
    {
        Debug.Log($"Skill Activated: {skill.SkillName}");
        
        // If Rage Mode is activated, apply rage effect to all other active skills
        if (skill.SkillType == GameEnums.SkillType.RageMode)
        {
            foreach (var s in _skillMap.Values)
            {
                if (s != skill)
                {
                    s.ApplyRageEffect(true);
                    // If the skill is active, reapply its effect with rage mode
                    if (s.IsActive)
                    {
                        switch (s.SkillType)
                        {
                            case GameEnums.SkillType.ArrowMultiplication:
                                ApplyArrowMultiplication();
                                break;
                            case GameEnums.SkillType.BounceDamage:
                                ApplyBounceDamage();
                                break;
                            case GameEnums.SkillType.AttackSpeedIncrease:
                                UpdateAttackSpeed();
                                break;
                            case GameEnums.SkillType.BurnDamage:
                                ApplyBurnDamage();
                                break;
                        }
                    }
                }
            }
        }
        
        switch (skill.SkillType)
        {
            case GameEnums.SkillType.ArrowMultiplication:
                ApplyArrowMultiplication();
                break;
                
            case GameEnums.SkillType.BounceDamage:
                Debug.Log("[PlayerSkillSystem] Bounce skill activated");
                ApplyBounceDamage();
                break;
                
            case GameEnums.SkillType.BurnDamage:
                Debug.Log("[PlayerSkillSystem] Burn damage skill activated");
                ApplyBurnDamage();
                break;
                
            case GameEnums.SkillType.AttackSpeedIncrease:
                UpdateAttackSpeed();
                break;
                
            case GameEnums.SkillType.RageMode:
                // Rage mode effect is already applied above
                break;
        }
    }
    
    /// <summary>
    /// Called when a skill is deactivated
    /// </summary>
    private void OnSkillDeactivated(SkillData skill)
    {
        Debug.Log($"Skill Deactivated: {skill.SkillName}");
        
        switch (skill.SkillType)
        {
            case GameEnums.SkillType.ArrowMultiplication:
                // When arrow multiplication is turned off, check if other skills are active
                if (IsSkillActive(GameEnums.SkillType.BounceDamage)) 
                {
                    ApplyBounceDamage();
                }
                else if (IsSkillActive(GameEnums.SkillType.BurnDamage))
                {
                    ApplyBurnDamage();
                }
                else if (IsSkillActive(GameEnums.SkillType.AttackSpeedIncrease))
                {
                    UpdateAttackSpeed();
                }
                else
                {
                    _weaponController.SetProjectileStrategy(new StandardProjectileStrategy());
                }
                break;
                
            case GameEnums.SkillType.BounceDamage:
                Debug.Log("[PlayerSkillSystem] Bounce skill deactivated");
                // When bounce is turned off, check if other skills are active
                if (IsSkillActive(GameEnums.SkillType.ArrowMultiplication)) 
                {
                    ApplyArrowMultiplication();
                }
                else if (IsSkillActive(GameEnums.SkillType.BurnDamage))
                {
                    ApplyBurnDamage();
                }
                else if (IsSkillActive(GameEnums.SkillType.AttackSpeedIncrease))
                {
                    UpdateAttackSpeed();
                }
                else
                {
                    _weaponController.SetProjectileStrategy(new StandardProjectileStrategy());
                }
                break;
                
            case GameEnums.SkillType.BurnDamage:
                Debug.Log("[PlayerSkillSystem] Burn damage skill deactivated");
                // When burn damage is turned off, check if other skills are active
                if (IsSkillActive(GameEnums.SkillType.ArrowMultiplication)) 
                {
                    ApplyArrowMultiplication();
                }
                else if (IsSkillActive(GameEnums.SkillType.BounceDamage))
                {
                    ApplyBounceDamage();
                }
                else if (IsSkillActive(GameEnums.SkillType.AttackSpeedIncrease))
                {
                    UpdateAttackSpeed();
                }
                else
                {
                    _weaponController.SetProjectileStrategy(new StandardProjectileStrategy());
                }
                break;
                
            case GameEnums.SkillType.AttackSpeedIncrease:
                UpdateAttackSpeed();
                break;
                
            case GameEnums.SkillType.RageMode:
                // When rage mode is turned off, update all skills
                foreach (var s in _skillMap.Values)
                {
                    if (s != skill)
                    {
                        s.ApplyRageEffect(false);
                    }
                }
                
                // Reapply all active skills without rage effect
                if (IsSkillActive(GameEnums.SkillType.ArrowMultiplication))
                {
                    ApplyArrowMultiplication();
                }
                else if (IsSkillActive(GameEnums.SkillType.BounceDamage))
                {
                    ApplyBounceDamage();
                }
                else if (IsSkillActive(GameEnums.SkillType.BurnDamage))
                {
                    ApplyBurnDamage();
                }
                else if (IsSkillActive(GameEnums.SkillType.AttackSpeedIncrease))
                {
                    UpdateAttackSpeed();
                }
                break;
        }
    }
    
    /// <summary>
    /// Applies arrow multiplication skill
    /// </summary>
    private void ApplyArrowMultiplication()
    {
        if (_weaponController == null) return;
        
        ArrowMultiplicationSkill arrowSkill = GetSkill<ArrowMultiplicationSkill>();
        if (arrowSkill != null)
        {
            // Rage durumunu uygula - mevcut rage değerini kullan
            arrowSkill.ApplyRageEffect(arrowSkill.IsRageActive);
            
            // Skill aktifse çoklu ok stratejisini uygula
            if (arrowSkill.IsActive)
            {
                // Ok sayısını al
                int arrowCount = arrowSkill.GetArrowCount();
                Debug.Log($"[PlayerSkillSystem] Applying Arrow Multiplication with {arrowCount} arrows");
                
                // Check bounce skill while applying arrow count
                BounceDamageSkill bounceSkill = GetSkill<BounceDamageSkill>();
                if (bounceSkill != null && bounceSkill.IsActive)
                {
                    // If both arrow multiplication and bounce are active, use multishot and bouncing combination
                    object[] parameters = new object[] { arrowCount, 15f };
                    IProjectileStrategy multiShotStrategy = ProjectileStrategyFactory.CreateStrategy(
                        GameEnums.ProjectileStrategyType.MultiShot,
                        parameters
                    );
                    _weaponController.SetProjectileStrategy(multiShotStrategy);
                    
                    Debug.Log($"[PlayerSkillSystem] Applied MultiShot strategy with Bounce, arrow count: {arrowCount}");
                }
                else
                {
                    // If only arrow multiplication is active
                    object[] parameters = new object[] { arrowCount, 15f };
                    IProjectileStrategy multiShotStrategy = ProjectileStrategyFactory.CreateStrategy(
                        GameEnums.ProjectileStrategyType.MultiShot,
                        parameters
                    );
                    _weaponController.SetProjectileStrategy(multiShotStrategy);
                    
                    Debug.Log($"[PlayerSkillSystem] Applied MultiShot strategy, arrow count: {arrowCount}");
                }
            }
            else
            {
                // Skill pasifse standart atış stratejisine dön
                _weaponController.SetProjectileStrategy(new StandardProjectileStrategy());
                Debug.Log("[PlayerSkillSystem] Arrow Multiplication deactivated, using standard strategy");
            }
        }
    }
    
    /// <summary>
    /// Applies bounce damage skill
    /// </summary>
    private void ApplyBounceDamage()
    {
        if (_weaponController == null) return;
        
        BounceDamageSkill bounceSkill = GetSkill<BounceDamageSkill>();
        if (bounceSkill != null && bounceSkill.IsActive)
        {
            // Apply rage effect based on RageMode skill state
            bool isRageActive = IsSkillActive(GameEnums.SkillType.RageMode);
            bounceSkill.ApplyRageEffect(isRageActive);
            
            Debug.Log($"[PlayerSkillSystem] Configuring bouncing projectile strategy. Rage Active: {isRageActive}, Bounce Count: {bounceSkill.GetBounceCount()}");
            
            // Check arrow multiplication skill
            ArrowMultiplicationSkill arrowSkill = GetSkill<ArrowMultiplicationSkill>();
            if (arrowSkill != null && arrowSkill.IsActive)
            {
                // If both arrow multiplication and bounce are active, use multishot and bouncing combination
                object[] parameters = new object[] { arrowSkill.GetArrowCount(), 15f };
                IProjectileStrategy multiShotStrategy = ProjectileStrategyFactory.CreateStrategy(
                    GameEnums.ProjectileStrategyType.MultiShot,
                    parameters
                );
                _weaponController.SetProjectileStrategy(multiShotStrategy);
            }
            else
            {
                // If only bounce is active
                object[] parameters = new object[] { bounceSkill.GetBounceCount(), 0.25f };
                IProjectileStrategy bouncingStrategy = ProjectileStrategyFactory.CreateStrategy(
                    GameEnums.ProjectileStrategyType.Bouncing,
                    parameters
                );
                _weaponController.SetProjectileStrategy(bouncingStrategy);
                
                Debug.Log($"[PlayerSkillSystem] Applied Bouncing strategy with bounce count: {bounceSkill.GetBounceCount()}");
            }
        }
        else 
        {
            Debug.Log("[PlayerSkillSystem] Bouncing projectile strategy disabled");
        }
    }
    
    /// <summary>
    /// Applies attack speed skill
    /// </summary>
    private void UpdateAttackSpeed()
    {
        if (_weaponController == null) return;
        
        // Get speed skill data
        AttackSpeedSkill speedSkill = GetSkill<AttackSpeedSkill>();
        
        if (speedSkill != null && speedSkill.IsActive)
        {
            // Apply rage effect based on RageMode skill state
            bool isRageActive = IsSkillActive(GameEnums.SkillType.RageMode);
            speedSkill.ApplyRageEffect(isRageActive);
            
            // Use strategy pattern to apply attack speed
            float speedMultiplier = speedSkill.GetSpeedMultiplier();
            object[] parameters = new object[] { speedMultiplier };
            
            IProjectileStrategy attackSpeedStrategy = ProjectileStrategyFactory.CreateStrategy(
                GameEnums.ProjectileStrategyType.AttackSpeed,
                parameters
            );
            
            _weaponController.SetProjectileStrategy(attackSpeedStrategy);
            
            Debug.Log($"[PlayerSkillSystem] Applied AttackSpeed strategy with multiplier: {speedMultiplier}");
        }
        else
        {
            // If skill is not active, reset to standard attack rate
            _weaponController.SetAttackRateMultiplier(1f);
            
            // Check if any other strategies are active
            if (!IsSkillActive(GameEnums.SkillType.ArrowMultiplication) && 
                !IsSkillActive(GameEnums.SkillType.BounceDamage) &&
                !IsSkillActive(GameEnums.SkillType.BurnDamage))
            {
                // If no other strategies are active, set to standard strategy
                _weaponController.SetProjectileStrategy(new StandardProjectileStrategy());
            }
        }
    }
    
    /// <summary>
    /// Applies burn damage skill
    /// </summary>
    private void ApplyBurnDamage()
    {
        if (_weaponController == null) return;
        
        BurnDamageSkill burnSkill = GetSkill<BurnDamageSkill>();
        if (burnSkill != null && burnSkill.IsActive)
        {
            // Apply rage effect based on RageMode skill state
            bool isRageActive = IsSkillActive(GameEnums.SkillType.RageMode);
            burnSkill.ApplyRageEffect(isRageActive);
            
            Debug.Log($"[PlayerSkillSystem] Configuring burning projectile strategy. Rage Active: {isRageActive}, Burn Duration: {burnSkill.GetBurnDuration()}");
            
            // Use strategy pattern to apply burn damage
            object[] parameters = new object[] { 
                burnSkill.GetBurnDamagePerSecond(),
                burnSkill.GetBurnDuration(),
                burnSkill.GetMaxStacks() 
            };
            
            IProjectileStrategy burningStrategy = ProjectileStrategyFactory.CreateStrategy(
                GameEnums.ProjectileStrategyType.Burning,
                parameters
            );
            
            _weaponController.SetProjectileStrategy(burningStrategy);
            
            Debug.Log($"[PlayerSkillSystem] Applied Burning strategy with damage: {burnSkill.GetBurnDamagePerSecond()}, duration: {burnSkill.GetBurnDuration()}");
        }
        else
        {
            // Check if any other strategies are active
            if (!IsSkillActive(GameEnums.SkillType.ArrowMultiplication) && 
                !IsSkillActive(GameEnums.SkillType.BounceDamage) &&
                !IsSkillActive(GameEnums.SkillType.AttackSpeedIncrease))
            {
                // If no other strategies are active, set to standard strategy
                _weaponController.SetProjectileStrategy(new StandardProjectileStrategy());
            }
            
            Debug.Log("[PlayerSkillSystem] Burning projectile strategy disabled");
        }
    }
    
    /// <summary>
    /// Reapplies a skill effect (for testing purposes)
    /// </summary>
    public void ReapplySkill(GameEnums.SkillType skillType)
    {
        if (_skillMap.TryGetValue(skillType, out SkillData skill))
        {
            Debug.Log($"Reapplying skill effect: {skill.SkillName}");
            
            switch (skillType)
            {
                case GameEnums.SkillType.ArrowMultiplication:
                    ApplyArrowMultiplication();
                    break;
                    
                case GameEnums.SkillType.BounceDamage:
                    ApplyBounceDamage();
                    break;
                    
                case GameEnums.SkillType.BurnDamage:
                    ApplyBurnDamage();
                    break;
                    
                case GameEnums.SkillType.AttackSpeedIncrease:
                    UpdateAttackSpeed();
                    break;
                
                case GameEnums.SkillType.RageMode:
                    // Force update all skills with rage effect
                    RageModeSkill rageSkill = GetSkill<RageModeSkill>();
                    if (rageSkill != null)
                    {
                        bool rageActive = rageSkill.IsRageModeActive;
                        foreach (var s in _skillMap.Values)
                        {
                            if (s != rageSkill)
                            {
                                s.ApplyRageEffect(rageActive);
                            }
                        }
                    }
                    break;
            }
        }
    }
    
    /// <summary>
    /// Get a skill by its type
    /// </summary>
    /// <typeparam name="T">Type of skill to get</typeparam>
    /// <returns>The skill, or null if not found</returns>
    public T GetSkill<T>() where T : SkillData
    {
        foreach (var skill in _skillMap.Values)
        {
            if (skill is T typedSkill)
            {
                return typedSkill;
            }
        }
        return null;
    }
    
    /// <summary>
    /// Returns the skill of the specified type
    /// </summary>
    public SkillData GetSkill(GameEnums.SkillType skillType)
    {
        if (_skillMap.TryGetValue(skillType, out SkillData skill))
        {
            return skill;
        }
        return null;
    }
    
    /// <summary>
    /// Belirli bir becerinin aktif olup olmadığını kontrol eder
    /// </summary>
    /// <param name="skillType">Kontrol edilecek beceri tipi</param>
    /// <returns>Beceri aktifse true, değilse false</returns>
    public bool IsSkillActive(GameEnums.SkillType skillType)
    {
        if (_skillMap.TryGetValue(skillType, out SkillData skill))
        {
            return skill.IsActive;
        }
        return false;
    }
    
    /// <summary>
    /// Tüm becerileri pasif duruma getirir (oyun başlangıcında çağrılır)
    /// </summary>
    private void ResetAllSkills()
    {
        foreach (var skill in _skillMap.Values)
        {
            if (skill.IsActive)
            {
                // Eğer beceri aktifse, devre dışı bırak
                skill.Deactivate();
                Debug.Log($"[PlayerSkillSystem] Reset skill state: {skill.SkillName}");
            }
        }
        
        // Standart atış stratejisine dön
        if (_weaponController != null)
        {
            _weaponController.SetProjectileStrategy(new StandardProjectileStrategy());
        }
        
        Debug.Log("[PlayerSkillSystem] All skills reset to inactive state");
    }
    
    /// <summary>
    /// Updates the rage mode skill
    /// </summary>
    private void UpdateRageSkill(float deltaTime)
    {
        // Get the rage mode skill
        RageModeSkill rageSkill = GetSkill<RageModeSkill>();
        if (rageSkill != null)
        {
            // Apply rage effect based on whether the skill is active
            bool rageActive = rageSkill.IsActive && rageSkill.IsRageModeActive;
            
            // Apply rage mode to all skills
            foreach (var skill in _skillMap.Values)
            {
                if (skill != rageSkill)
                {
                    skill.ApplyRageEffect(rageActive);
                }
            }
        }
    }
} 