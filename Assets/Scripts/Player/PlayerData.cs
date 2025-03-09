using UnityEngine;

/// <summary>
/// Scriptable object that contains all player stats and settings
/// </summary>
[CreateAssetMenu(fileName = "NewPlayerData", menuName = "Game/Player Data")]
public class PlayerData : ScriptableObject
{
    [Header("Player Identity")]
    [SerializeField] private string _playerName = "Hero";
    [SerializeField] private Sprite _playerIcon;
    
    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _rotationSpeed = 10f;
    
    [Header("Combat Settings")]
    [SerializeField] private float _baseAttackRate = 1.0f; // Attacks per second
    [SerializeField] private float _baseDamage = 10f;
    [SerializeField] private float _projectileSpeed = 20f;
    [SerializeField] private float _damageMultiplier = 1.0f;
    
    
    [Header("Special Abilities")]
    [SerializeField] private bool _useMultiShotByDefault = true;
    [SerializeField] private int _defaultProjectileCount = 2;
    [SerializeField] private float _defaultSpreadAngle = 15f;
    
    // Properties - Movement
    public float MoveSpeed => _moveSpeed;
    public float RotationSpeed => _rotationSpeed;
    public float BaseAttackRate => _baseAttackRate;
    public float BaseDamage => _baseDamage;
    public float ProjectileSpeed => _projectileSpeed;
    public float DamageMultiplier => _damageMultiplier;
    
    // Properties - Special Abilities
    public bool UseMultiShotByDefault => _useMultiShotByDefault;
    public int DefaultProjectileCount => _defaultProjectileCount;
    public float DefaultSpreadAngle => _defaultSpreadAngle;
    
    // Properties - Identity
    public string PlayerName => _playerName;
    public Sprite PlayerIcon => _playerIcon;
} 