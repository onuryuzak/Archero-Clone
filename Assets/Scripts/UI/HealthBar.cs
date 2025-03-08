using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Controls the behavior of a health bar UI element that floats above entities in the game world
/// </summary>
public class HealthBar : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform _target;
    [SerializeField] private Vector3 _offset = new Vector3(0, 1.5f, 0); // Position above target
    [SerializeField] private bool _faceCamera = true;
    
    [Header("Bar Settings")]
    [SerializeField] private Image _fillImage;
    [SerializeField] private Image _backgroundImage;
    [SerializeField] private Image _borderImage;
    [SerializeField] private float _smoothFillSpeed = 5f;
    [SerializeField] private bool _useSmoothFill = true;
    
    [Header("Text Settings")]
    [SerializeField] private TextMeshProUGUI _healthText;
    [SerializeField] private bool _showHealthText = true;
    [SerializeField] private bool _showAsPercentage = false;
    [SerializeField] private string _textFormat = "{0}/{1}";
    [SerializeField] private string _percentFormat = "{0}%";
    
    [Header("Animation Settings")]
    [SerializeField] private bool _useColorTransition = true;
    [SerializeField] private Color _highHealthColor = Color.green;
    [SerializeField] private Color _mediumHealthColor = Color.yellow;
    [SerializeField] private Color _lowHealthColor = Color.red;
    [SerializeField] private float _lowHealthThreshold = 0.3f;
    [SerializeField] private float _mediumHealthThreshold = 0.6f;
    [SerializeField] private bool _pulseWhenLow = false;
    [SerializeField] private float _pulseSpeed = 2f;
    
    [Header("Visibility Settings")]
    [SerializeField] private bool _hideWhenFull = false;
    [SerializeField] private bool _hideAfterDelay = false;
    [SerializeField] private float _hideDelay = 3f;
    
    // Runtime variables
    private float _currentFillAmount = 1f;
    private float _targetFillAmount = 1f;
    private float _maxHealth = 100f;
    private float _currentHealth = 100f;
    private Coroutine _pulseCoroutine;
    private Coroutine _hideCoroutine;
    private Camera _mainCamera;
    private Canvas _canvas;
    private CanvasGroup _canvasGroup;
    
    private void Awake()
    {
        // Get the canvas component
        _canvas = GetComponentInParent<Canvas>();
        _canvasGroup = GetComponentInParent<CanvasGroup>();
        
        if (_canvasGroup == null && _hideAfterDelay)
        {
            _canvasGroup = _canvas.gameObject.AddComponent<CanvasGroup>();
        }
        
        // Default initialization if needed
        if (_fillImage == null)
            _fillImage = transform.Find("Fill")?.GetComponent<Image>();
        
        if (_healthText == null)
            _healthText = GetComponentInChildren<TextMeshProUGUI>();
        
        // Find main camera
        _mainCamera = Camera.main;
    }
    
    private void Start()
    {
        // Initialize with current values
        UpdateVisuals();
        
        // If set to hide when full and health is full, hide the health bar
        if (_hideWhenFull && _currentHealth >= _maxHealth)
        {
            SetVisible(false);
        }
    }
    
    private void LateUpdate()
    {
        // Follow target if assigned
        if (_target != null)
        {
            transform.position = _target.position + _offset;
            
            // Make the health bar face the camera
            if (_faceCamera && _mainCamera != null)
            {
                transform.rotation = _mainCamera.transform.rotation;
            }
        }
        
        // Handle smooth fill transition if enabled
        if (_useSmoothFill && _currentFillAmount != _targetFillAmount)
        {
            _currentFillAmount = Mathf.Lerp(_currentFillAmount, _targetFillAmount, Time.deltaTime * _smoothFillSpeed);
            
            // If we're very close to the target, snap to it
            if (Mathf.Abs(_currentFillAmount - _targetFillAmount) < 0.005f)
                _currentFillAmount = _targetFillAmount;
                
            _fillImage.fillAmount = _currentFillAmount;
        }
    }
    
    /// <summary>
    /// Initialize the health bar with the maximum health value
    /// </summary>
    public void Initialize(float maxHealth, Transform target = null)
    {
        _maxHealth = maxHealth;
        _currentHealth = maxHealth;
        _targetFillAmount = 1f;
        _currentFillAmount = 1f;
        
        if (target != null)
            _target = target;
        
        if (_fillImage != null)
            _fillImage.fillAmount = 1f;
            
        UpdateVisuals();
        
        // Make visible
        SetVisible(true);
    }
    
    /// <summary>
    /// Update the health bar with a new health value
    /// </summary>
    public void UpdateHealth(float currentHealth)
    {
        // Make visible when health changes
        SetVisible(true);
        
        // Cancel any hide coroutine
        if (_hideCoroutine != null)
        {
            StopCoroutine(_hideCoroutine);
            _hideCoroutine = null;
        }
        
        // Clamp the health value
        _currentHealth = Mathf.Clamp(currentHealth, 0, _maxHealth);
        
        // Calculate new fill amount (normalize to 0-1 range)
        _targetFillAmount = _maxHealth > 0 ? _currentHealth / _maxHealth : 0;
        
        // If not using smooth fill, update immediately
        if (!_useSmoothFill && _fillImage != null)
            _fillImage.fillAmount = _targetFillAmount;
        
        // Update visuals (text and color)
        UpdateVisuals();
        
        // Handle visibility based on settings
        if (_hideWhenFull && _currentHealth >= _maxHealth)
        {
            SetVisible(false);
        }
        else if (_hideAfterDelay)
        {
            // Start the hide delay
            if (_hideCoroutine != null)
                StopCoroutine(_hideCoroutine);
                
            _hideCoroutine = StartCoroutine(HideAfterDelay());
        }
    }
    
    /// <summary>
    /// Update all visual elements of the health bar
    /// </summary>
    private void UpdateVisuals()
    {
        // Update fill image color based on health percentage
        if (_useColorTransition && _fillImage != null)
        {
            _fillImage.color = GetHealthColor(_targetFillAmount);
        }
        
        // Update health text
        UpdateHealthText();
        
        // Handle pulse effect if enabled
        HandlePulseEffect();
    }
    
    /// <summary>
    /// Update the health text based on current settings
    /// </summary>
    private void UpdateHealthText()
    {
        if (_healthText != null && _showHealthText)
        {
            if (_showAsPercentage)
            {
                float percentage = Mathf.Round(_targetFillAmount * 100);
                _healthText.text = string.Format(_percentFormat, percentage);
            }
            else
            {
                _healthText.text = string.Format(_textFormat, Mathf.Ceil(_currentHealth), Mathf.Ceil(_maxHealth));
            }
            
            _healthText.gameObject.SetActive(true);
        }
        else if (_healthText != null)
        {
            _healthText.gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// Get the appropriate color based on the health percentage
    /// </summary>
    private Color GetHealthColor(float healthPercentage)
    {
        if (healthPercentage <= _lowHealthThreshold)
            return _lowHealthColor;
        else if (healthPercentage <= _mediumHealthThreshold)
            return _mediumHealthColor;
        else
            return _highHealthColor;
    }
    
    /// <summary>
    /// Handle the pulse effect when health is low
    /// </summary>
    private void HandlePulseEffect()
    {
        if (_pulseWhenLow && _targetFillAmount <= _lowHealthThreshold)
        {
            // Start pulsing if we're not already
            if (_pulseCoroutine == null)
                _pulseCoroutine = StartCoroutine(PulseEffect());
        }
        else
        {
            // Stop pulsing if we were
            if (_pulseCoroutine != null)
            {
                StopCoroutine(_pulseCoroutine);
                _pulseCoroutine = null;
                
                // Reset alpha
                if (_fillImage != null)
                {
                    Color color = _fillImage.color;
                    color.a = 1f;
                    _fillImage.color = color;
                }
            }
        }
    }
    
    /// <summary>
    /// Coroutine for pulsing the fill image when health is low
    /// </summary>
    private IEnumerator PulseEffect()
    {
        while (true)
        {
            // Pulse the alpha of the fill image
            if (_fillImage != null)
            {
                float alpha = 0.5f + 0.5f * Mathf.Sin(Time.time * _pulseSpeed);
                Color color = _fillImage.color;
                color.a = Mathf.Clamp(alpha, 0.5f, 1f);
                _fillImage.color = color;
            }
            
            yield return null;
        }
    }
    
    /// <summary>
    /// Set the visibility of the health bar
    /// </summary>
    public void SetVisible(bool visible)
    {
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = visible ? 1f : 0f;
            _canvasGroup.blocksRaycasts = visible;
        }
        else
        {
            gameObject.SetActive(visible);
        }
    }
    
    /// <summary>
    /// Coroutine to hide the health bar after a delay
    /// </summary>
    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(_hideDelay);
        
        // Only hide if we're not at low health
        if (_targetFillAmount > _lowHealthThreshold)
        {
            SetVisible(false);
        }
        
        _hideCoroutine = null;
    }
}