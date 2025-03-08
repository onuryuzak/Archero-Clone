using UnityEngine;

/// <summary>
/// Component that adds burning effect to projectiles
/// </summary>
public class BurningProjectile : MonoBehaviour
{
    private float _burnDamagePerSecond = 5f;
    private float _burnDuration = 3f;
    private int _maxStacks = 3;
    
    /// <summary>
    /// Initialize the burning projectile properties
    /// </summary>
    public void Initialize(float burnDamagePerSecond, float burnDuration, int maxStacks)
    {
        _burnDamagePerSecond = burnDamagePerSecond;
        _burnDuration = burnDuration;
        _maxStacks = maxStacks;
    }
    
    /// <summary>
    /// Applies burning effect to the target
    /// </summary>
    public void ApplyBurningEffect(GameObject target)
    {
        if (target == null) return;
        
        // Only apply burning effect to enemies
        Enemy enemy = target.GetComponent<Enemy>();
        if (enemy != null)
        {
            // Add BurnEffect component if it doesn't exist
            BurnEffect burnEffect = enemy.GetComponent<BurnEffect>();
            if (burnEffect == null)
            {
                burnEffect = enemy.gameObject.AddComponent<BurnEffect>();
            }
            
            // Configure and apply the burn stack
            burnEffect.SetMaxStacks(_maxStacks);
            burnEffect.AddBurnStack(_burnDuration, _burnDamagePerSecond);
            
            Debug.Log($"Applied burn effect to {enemy.name}: {_burnDamagePerSecond} dps for {_burnDuration} seconds");
        }
    }
} 