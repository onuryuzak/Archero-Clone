using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : InjectedMonoBehaviour
{
    [Header("Player Data")]
    [SerializeField] private PlayerData _playerData;
    
    [Header("References")]
    [SerializeField] private Joystick _joystick;
    [SerializeField] private WeaponController _weaponController;
    
    private Vector3 _moveDirection;
    private bool _isMoving = false;
    
    // Y pozisyonu her zaman 0 olacak
    private const float FIXED_Y_POSITION = 0f;
    
    // Property to access player data
    public PlayerData PlayerData => _playerData;
    
    protected override void InjectDependencies()
    {
        // If player data is not assigned in inspector, try to resolve from DI container
        if (_playerData == null)
        {
            _playerData = Resolve<PlayerData>();
        }
        
        // If joystick is not assigned, try to resolve from DI container
        if (_joystick == null)
        {
            _joystick = Resolve<Joystick>();
        }
        
        // Register ourselves as a service that others can use
        Register<PlayerController>(this);
        
        // Register player data if we have it
        if (_playerData != null)
        {
            Register<PlayerData>(_playerData);
        }
        
        // Set dependencies injected flag
        DependenciesInjected = _playerData != null; // Requires at least player data to function
    }
    
    protected override void Awake()
    {
        // Call base implementation to inject dependencies
        base.Awake();
        
        // Check if player data is assigned
        if (_playerData == null)
        {
            Debug.LogError("PlayerData is not assigned! Assign a PlayerData scriptable object in the inspector or make sure it's registered in the DependencyContainer.");
        }
        
        // İlk çalıştığında Y pozisyonunu ayarla
        SetPosition(transform.position);
        
        // If weapon controller is not assigned, try to find it in children
        if (_weaponController == null)
        {
            _weaponController = GetComponentInChildren<WeaponController>();
        }
            
        // Apply player data to weapon controller
        if (_weaponController != null && _playerData != null)
        {
            _weaponController.Initialize(_playerData);
        }
    }
    
    private void Update()
    {
        GetJoystickInput();
        HandleMovement();
        HandleAttack();
    }
    
    // Update'ten sonra çalışır, tüm değişikliklerden sonra pozisyonu düzeltir
    private void LateUpdate()
    {
        // Y eksenini düzelt
        Vector3 currentPos = transform.position;
        if (currentPos.y != FIXED_Y_POSITION)
        {
            SetPosition(currentPos);
        }
    }
    
    // Pozisyon ayarlamayı merkezileştiren helper metodu
    private void SetPosition(Vector3 position)
    {
        position.y = FIXED_Y_POSITION;
        transform.position = position;
    }
    
    private void GetJoystickInput()
    {
        // Joystick bağlı değilse DI container'dan bulmaya çalış
        if (_joystick == null)
        {
            _joystick = Resolve<Joystick>();
            
            // Eğer hala null ise, son çare olarak sahneyi ara
            if (_joystick == null)
            {
                _joystick = FindFirstObjectByType<Joystick>();
                
                // Bulunduysa container'a kaydet
                if (_joystick != null)
                {
                    Register<Joystick>(_joystick);
                }
                else
                {
                    Debug.LogError("Joystick bulunamadı! Lütfen sahneye bir Joystick ekleyin veya DependencyContainer'a kaydedin.");
                    return;
                }
            }
        }
        
        // Joystick'ten değerleri al
        float h = _joystick.Horizontal;
        float v = _joystick.Vertical;
        
        // Değerler çok küçükse (ölü bölge) sıfırla
        if (Mathf.Abs(h) < 0.1f) h = 0;
        if (Mathf.Abs(v) < 0.1f) v = 0;
        
        // Hareket vektörünü oluştur ve normalize et
        _moveDirection = new Vector3(h, 0, v);
        
        // Normalize sadece magnitude > 0 ise çalışır
        if (_moveDirection.magnitude > 0)
        {
            _moveDirection.Normalize();
        }
        
        // Hareket var mı kontrolü
        _isMoving = _moveDirection.magnitude > 0;
    }
    
    private void HandleMovement()
    {
        if (!_isMoving || _playerData == null)
            return;
            
        // Hareket vektörünü oluştur - Y DEĞERİNİ 0 OLARAK AYARLA
        Vector3 movement = new Vector3(_moveDirection.x, 0, _moveDirection.z) * _playerData.MoveSpeed * Time.deltaTime;
        
        // Yeni pozisyonu hesapla
        Vector3 newPosition = transform.position + movement;
        
        // Merkezileştirilmiş pozisyon ayarlama metodunu kullan
        SetPosition(newPosition);
        
        // Hareket yönüne dön
        if (movement != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movement);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _playerData.RotationSpeed * Time.deltaTime);
        }
    }
    
    private void HandleAttack()
    {
        if (!_isMoving && _weaponController != null)
        {
            // Playerin ilerisindeki bir noktaya ateş et
            Vector3 targetPosition = transform.position + transform.forward * 20f;
            _weaponController.TryAttack(targetPosition);
        }
    }
    
    public bool IsPlayerMoving()
    {
        return _isMoving;
    }
}