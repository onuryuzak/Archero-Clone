using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles projectile movement, collision, and damage
/// </summary>
public class Projectile : MonoBehaviour
{
    // Physics settings
    [Header("Physics Settings")]
    [SerializeField] private bool _useGravity = false;
    [SerializeField] private float _gravityScale = 1f;
    [SerializeField] private float _maxLifetime = 5f; // Maximum time before auto-destruction
    [SerializeField] private LayerMask _collisionLayers; // What layers this projectile collides with
    
    [Header("Visual Effects")]
    [SerializeField] private GameObject _hitEffectPrefab;
    
    [Header("Advanced Physics")]
    [SerializeField] private bool _useAdvancedPhysics = true;
    [SerializeField] private float _initialAngle = 15f; // Initial angle in degrees
    [SerializeField] private float _dragCoefficient = 0.01f; // Air resistance factor
    
    // State
    private Vector3 _direction;
    private float _speed;  // Bu değişkeni PlayerData'dan gelecek değeri saklamak için kullanacağız
    private float _damage;
    private float _lifetime = 0f;
    private bool _isPaused = false; // Added to support pausing movement
    
    // Cache
    private Transform _transform;
    
    private Vector3 _initialVelocity; // Initial velocity vector
    private Vector3 _currentVelocity; // Current velocity
    private Vector3 _acceleration;    // Current acceleration
    
    // Properties
    public float Speed => _speed;
    public float Damage => _damage;
    
    private void Awake()
    {
        _transform = transform;
    }
    
    /// <summary>
    /// Initializes the projectile with direction, speed, and damage
    /// </summary>
    public void Initialize(Vector3 direction, float speed, float damage)
    {
        _direction = direction.normalized;
        _speed = speed;
        _damage = damage;
        
        // Set forward direction to match movement direction
        _transform.forward = _direction;
        
        // Calculate initial velocity for advanced physics
        if (_useAdvancedPhysics)
        {
            // Apply initial angle if using gravity
            if (_useGravity && _initialAngle != 0)
            {
                // Rotate the direction vector up by the initial angle
                Quaternion rotation = Quaternion.AngleAxis(_initialAngle, Vector3.Cross(_direction, Vector3.up).normalized);
                _direction = rotation * _direction;
                _transform.forward = _direction;
            }
            
            _initialVelocity = _direction * _speed;
            _currentVelocity = _initialVelocity;
        }
    }
    
    /// <summary>
    /// Updates the projectile position and rotation based on physics
    /// </summary>
    private void Update()
    {
        if (_isPaused) return; // Skip movement if paused
        
        // Count lifetime and destroy if exceeded maximum
        _lifetime += Time.deltaTime;
        if (_lifetime > _maxLifetime)
        {
            Destroy(gameObject);
            return;
        }
        
        Vector3 oldPosition = _transform.position;
        
        if (_useAdvancedPhysics)
        {
            // Calculate acceleration (gravity + drag)
            _acceleration = Vector3.zero;
            
            // Apply gravity
            if (_useGravity)
            {
                _acceleration += Physics.gravity * _gravityScale;
            }
            
            // Apply drag (air resistance) - proportional to velocity squared
            if (_dragCoefficient > 0 && _currentVelocity.magnitude > 0)
            {
                Vector3 dragForce = -_currentVelocity.normalized * _dragCoefficient * _currentVelocity.sqrMagnitude;
                _acceleration += dragForce;
            }
            
            // Update velocity using verlet integration (more stable than Euler)
            _currentVelocity += _acceleration * Time.deltaTime;
            
            // Update position
            _transform.position += _currentVelocity * Time.deltaTime;
            
            // Orient projectile along velocity direction
            if (_currentVelocity.sqrMagnitude > 0.001f)
            {
                _transform.forward = _currentVelocity.normalized;
            }
        }
        else
        {
            // Use simple physics (legacy behavior)
            // Apply gravity if enabled
            if (_useGravity)
            {
                _direction += Physics.gravity * _gravityScale * Time.deltaTime;
            }
            
            // Move in the current direction
            Vector3 movement = _direction * _speed * Time.deltaTime;
            _transform.position += movement;
            
            // Orient in the direction of movement if using gravity
            if (_useGravity && _direction != Vector3.zero)
            {
                _transform.forward = _direction;
            }
        }
        
        // Calculate movement vector for collision detection
        Vector3 movementVector = _transform.position - oldPosition;
        
        // Check for collisions
        if (movementVector.magnitude > 0 && Physics.Raycast(oldPosition, movementVector.normalized, out RaycastHit hit, movementVector.magnitude, _collisionLayers))
        {
            HandleCollision(hit);
        }
    }
    
    /// <summary>
    /// Pauses the projectile's movement
    /// </summary>
    public void PauseMovement()
    {
        _isPaused = true;
    }
    
    /// <summary>
    /// Resumes the projectile's movement
    /// </summary>
    public void ResumeMovement()
    {
        _isPaused = false;
    }
    
    /// <summary>
    /// Handles collision events
    /// </summary>
    private void HandleCollision(RaycastHit hit)
    {
        // Get the object we hit
        GameObject hitObject = hit.collider.gameObject;
        
        // Check if the hit object is damageable
        IDamageable damageable = hitObject.GetComponent<IDamageable>();
        if (damageable != null)
        {
            // Apply damage
            damageable.TakeDamage(_damage);
            
            // Check if we need to apply burn damage
            Enemy enemy = hitObject.GetComponent<Enemy>();
            if (enemy != null)
            {
                // Find if there's an active skill system in the scene
                PlayerSkillSystem skillSystem = FindObjectOfType<PlayerSkillSystem>();
                if (skillSystem != null && skillSystem.IsSkillActive(GameEnums.SkillType.BurnDamage))
                {
                    // Get burn skill data
                    BurnDamageSkill burnSkill = skillSystem.GetSkill<BurnDamageSkill>();
                    if (burnSkill != null)
                    {
                        // Add or get BurnEffect component
                        BurnEffect burnEffect = enemy.GetComponent<BurnEffect>();
                        if (burnEffect == null)
                        {
                            burnEffect = enemy.gameObject.AddComponent<BurnEffect>();
                        }
                        
                        // Configure max stacks
                        burnEffect.SetMaxStacks(burnSkill.GetMaxStacks());
                        
                        // Apply burn stack with appropriate duration and damage
                        burnEffect.AddBurnStack(
                            burnSkill.GetBurnDuration(),
                            burnSkill.GetBurnDamagePerSecond()
                        );
                        
                        Debug.Log($"Applied burn effect to {enemy.name}: {burnSkill.GetBurnDamagePerSecond()} dps for {burnSkill.GetBurnDuration()} seconds");
                    }
                }
            }
        }
        
        // Instantiate hit effect if available
        if (_hitEffectPrefab != null)
        {
            Instantiate(_hitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
        }
        
        // Invoke the hit event
        OnHit?.Invoke(hitObject, hit.point);
        
        // Eğer BouncingProjectile componenti yoksa, mermiyi yok et
        BouncingProjectile bouncer = GetComponent<BouncingProjectile>();
        if (bouncer == null)
        {
            // Mermiyi yok et - Bounce özelliği olmayan projeler ilk çarpışmada yok edilir
            Destroy(gameObject, 0.05f); // Slight delay to ensure hit effects are shown
        }
        // BouncingProjectile componenti varsa, HandleHit metodu sekme işlemini yönetecek
    }
    
    /// <summary>
    /// Handle collision with triggers
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        // Check if this layer is in our collision layers
        if (((1 << other.gameObject.layer) & _collisionLayers) != 0)
        {
            // Skip collision if paused (added to support the bounce feature)
            if (_isPaused) return;
            
            // Trigger için doğrudan GameObject ve konum kullanarak çarpışma işlemi
            HandleTriggerCollision(other.gameObject, transform.position);
        }
    }
    
    /// <summary>
    /// Trigger çarpışmalarını yönetir
    /// </summary>
    private void HandleTriggerCollision(GameObject hitObject, Vector3 hitPosition)
    {
        // Hasar verilebilir nesne mi kontrol et
        IDamageable damageable = hitObject.GetComponent<IDamageable>();
        if (damageable != null)
        {
            // Hasar uygula
            damageable.TakeDamage(_damage);
            
            // Görsel efektler eklenebilir
            
            // OnHit olayını tetikle
            OnHit?.Invoke(hitObject, hitPosition);
            
            // Eğer BouncingProjectile componenti yoksa, mermiyi yok et
            BouncingProjectile bouncer = GetComponent<BouncingProjectile>();
            if (bouncer == null)
            {
                // Mermiyi yok et - Bounce özelliği olmayan projeler ilk çarpışmada yok edilir
                Destroy(gameObject); // Slight delay to ensure hit effects are shown
            }
            // BouncingProjectile componenti varsa, HandleHit metodu sekme işlemini yönetecek
        }
    }
    
    // Events
    public event Action<GameObject, Vector3> OnHit;

    /// <summary>
    /// Gravityi açıp kapatmak için public method
    /// </summary>
    /// <param name="useGravity">Gravity aktif olsun mu?</param>
    public void SetGravity(bool useGravity)
    {
        _useGravity = useGravity;
        Debug.Log($"[Projectile] Gravity set to: {useGravity}");
    }
} 