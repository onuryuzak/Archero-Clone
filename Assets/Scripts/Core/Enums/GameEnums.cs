/// <summary>
/// Oyundaki tüm enum tanımlamalarını içerir
/// </summary>
public static class GameEnums
{
    /// <summary>
    /// Yetenek türlerini tanımlar
    /// </summary>
    public enum SkillType
    {
        ArrowMultiplication,
        BounceDamage,
        BurnDamage,
        AttackSpeedIncrease,
        RageMode
    }
    
    /// <summary>
    /// Mermi stratejisi türlerini tanımlar
    /// </summary>
    public enum ProjectileStrategyType
    {
        Standard,
        MultiShot,
        Bouncing,
        Burning,
        Piercing,
        Homing,
        Explosive
    }
    
    /// <summary>
    /// Hasar türlerini tanımlar
    /// </summary>
    public enum DamageType
    {
        Normal,
        Burn,
        Poison,
        Ice,
        Lightning
    }
} 