using System.Collections;
using UnityEngine;

/// <summary>
/// Controls the weapon behavior including shooting and projectile strategies
/// </summary>
public class WeaponController : InjectedMonoBehaviour
{
    [Header("Projectile Settings")] [SerializeField]
    private GameObject _projectilePrefab;

    [SerializeField] private Transform _firePoint;

    // Variables that will be set from PlayerData
    private float _projectileSpeed = 20f; // Defaults to 20, but will be updated from PlayerData
    private float _baseDamage = 10f;
    private float _baseAttackRate = 1.0f; // Attacks per second
    private float _attackRateMultiplier = 1.0f; // Multiplier from skills
    private bool _useMultiShotByDefault = true;

    private float _attackCooldown;
    private bool _canAttack = true;
    private IProjectileStrategy _projectileStrategy;
    private PlayerData _playerData;

    // Property to calculate the actual attack rate with multiplier
    public float ActualAttackRate => Mathf.Max(0.1f, _baseAttackRate * _attackRateMultiplier);

    protected override void InjectDependencies()
    {
        // Try to inject PlayerData if not already set
        if (_playerData == null)
        {
            _playerData = Resolve<PlayerData>();
            
            // If successfully resolved, initialize with it
            if (_playerData != null)
            {
                Initialize(_playerData);
            }
        }
        
        // Set dependencies injected flag
        DependenciesInjected = true; // We can function without PlayerData initially
    }

    protected override void Awake()
    {
        // Call base implementation to inject dependencies
        base.Awake();
        
        // If no fire point is set, use this transform
        if (_firePoint == null)
        {
            _firePoint = transform;
        }
        
        // Initialize cooldown to allow immediate firing
        _attackCooldown = 0f;
        _canAttack = true;
    }

    /// <summary>
    /// Initialize weapon with player data
    /// </summary>
    /// <param name="playerData">The player data to use for initialization</param>
    public void Initialize(PlayerData playerData)
    {
        if (playerData == null)
        {
            Debug.LogError("PlayerData is null in WeaponController.Initialize!");
            return;
        }
        
        _playerData = playerData;
        
        // Get values from PlayerData
        _projectileSpeed = _playerData.ProjectileSpeed;
        _baseDamage = _playerData.BaseDamage * _playerData.DamageMultiplier;
        _baseAttackRate = _playerData.BaseAttackRate;
        _useMultiShotByDefault = _playerData.UseMultiShotByDefault;
        
        // Set default strategy based on player data
        if (_useMultiShotByDefault)
        {
            _projectileStrategy = new MultiShotProjectileStrategy(_playerData);
        }
        else
        {
            _projectileStrategy = new StandardProjectileStrategy();
        }
        
        // Ensure weapon is ready to fire immediately at start
        _canAttack = true;
    }

    private void Start()
    {
        // Check if we have been initialized through PlayerController or DI container
        if (_playerData == null)
        {
            // Try to get player data from DI container again (in case it was registered after our Awake)
            _playerData = Resolve<PlayerData>();
            
            if (_playerData != null)
            {
                Initialize(_playerData);
            }
            else
            {
                // Try to find player controller and initialize
                var playerController = Resolve<PlayerController>();
                
                if (playerController == null)
                {
                    // Last resort: try to find it in the scene
                    playerController = FindObjectOfType<PlayerController>();
                    
                    // If found, register it for future use
                    if (playerController != null)
                    {
                        Register<PlayerController>(playerController);
                    }
                }
                
                if (playerController != null && playerController.PlayerData != null)
                {
                    Initialize(playerController.PlayerData);
                }
                else
                {
                    Debug.LogWarning("WeaponController not initialized with PlayerData!");
                }
            }
        }
        
        // Initialize cooldown to allow immediate firing
        _attackCooldown = 0f;
    }

    private void Update()
    {
        // Debug the attack rate values
        if (Time.frameCount % 60 == 0) // Log every 60 frames
        {
            Debug.Log($"Attack Rate: Base={_baseAttackRate}, Multiplier={_attackRateMultiplier}, Actual={ActualAttackRate}");
        }
        
        // Update cooldown
        if (!_canAttack)
        {
            // Decrease cooldown over time
            _attackCooldown -= Time.deltaTime;
            
            // When cooldown reaches zero, allow attacking again
            if (_attackCooldown <= 0f)
            {
                _attackCooldown = 0f; // Ensure it's exactly zero to avoid floating point errors
                _canAttack = true;
                Debug.Log("Weapon ready to fire again");
            }
        }
    }

    /// <summary>
    /// Attempts to fire the weapon. Will only succeed if not on cooldown.
    /// </summary>
    /// <param name="targetPosition">Position to aim at</param>
    /// <returns>True if attack was successful</returns>
    public bool TryAttack(Vector3 targetPosition)
    {
        
        if (!_canAttack || _projectilePrefab == null)
            return false;

        Vector3 direction = (targetPosition - _firePoint.position).normalized;
        direction.y = 0; // Keep projectiles level with the ground

        if (direction == Vector3.zero)
            direction = transform.forward;

        // Use strategy pattern to fire projectiles
        _projectileStrategy.FireProjectile(this, _firePoint.position, direction, _projectileSpeed, _baseDamage);

        // Start cooldown
        _canAttack = false;
        _attackCooldown = 1f / ActualAttackRate; // Reset the cooldown timer

        return true;
    }

    /// <summary>
    /// Sets the projectile strategy to use
    /// </summary>
    public void SetProjectileStrategy(IProjectileStrategy strategy)
    {
        if (strategy != null)
        {
            _projectileStrategy = strategy;
        }
    }

    /// <summary>
    /// Sets the attack rate multiplier (used by skills)
    /// </summary>
    public void SetAttackRateMultiplier(float multiplier)
    {
        float oldMultiplier = _attackRateMultiplier;
        _attackRateMultiplier = Mathf.Max(0.1f, multiplier); // Ensure it doesn't go too low
        
        // If we're currently on cooldown, adjust the cooldown proportionally to reflect the new rate
        if (!_canAttack && _attackCooldown > 0)
        {
            float oldAttackRate = _baseAttackRate * oldMultiplier;
            float newAttackRate = ActualAttackRate;
            
            if (oldAttackRate > 0 && newAttackRate > 0)
            {
                // Calculate what percentage of the cooldown is remaining
                float oldCooldownTotal = 1f / oldAttackRate;
                float remainingPercentage = _attackCooldown / oldCooldownTotal;
                
                // Apply that same percentage to the new cooldown
                float newCooldownTotal = 1f / newAttackRate;
                _attackCooldown = newCooldownTotal * remainingPercentage;
            }
        }
    }

    /// <summary>
    /// Creates a projectile at the specified position with the given direction
    /// </summary>
    public GameObject CreateProjectile(Vector3 position, Vector3 direction, float speed, float damage)
    {
        GameObject projectileObj = Instantiate(_projectilePrefab, position, Quaternion.LookRotation(direction));
        Projectile projectile = projectileObj.GetComponent<Projectile>();

        if (projectile != null)
        {
            projectile.Initialize(direction, speed, damage);
        }

        return projectileObj;
    }
}