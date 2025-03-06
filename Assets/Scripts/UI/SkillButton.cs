using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

/// <summary>
/// UI component for a skill button that displays skill information and handles interactions
/// </summary>
public class SkillButton : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button _button;
    [SerializeField] private Image _iconImage;
    [SerializeField] private Image _backgroundImage;
    [SerializeField] private TextMeshProUGUI _skillNameText;
    [SerializeField] private TextMeshProUGUI _skillDescriptionText;
    
    [Header("Color Settings")]
    [SerializeField] private Color _activeColor = Color.white;
    [SerializeField] private Color _inactiveColor = new Color(0.5f, 0.5f, 0.5f);
    [SerializeField] private Color _rageActiveColor = new Color(1f, 0.3f, 0.3f);
    
    [Header("Skill Reference")]
    [SerializeField] private GameEnums.SkillType _skillType;
    
    private PlayerSkillSystem _skillSystem;
    private SkillData _skillData;
    
    private void Awake()
    {
        // Get component references if not set
        if (_button == null)
            _button = GetComponent<Button>();
            
        // Find skill system in scene
        _skillSystem = FindObjectOfType<PlayerSkillSystem>();
        
        // Add click listener
        if (_button != null)
            _button.onClick.AddListener(OnButtonClicked);
    }
    
    private void Start()
    {
        // Get skill data from skill system
        if (_skillSystem != null)
        {
            _skillData = _skillSystem.GetSkill(_skillType);
            
            if (_skillData != null)
            {
                // Set initial UI
                _skillNameText.text = _skillData.SkillName;
                _skillDescriptionText.text = _skillData.Description;
                
                if (_skillData.Icon != null)
                    _iconImage.sprite = _skillData.Icon;
                
                // Subscribe to skill events
                _skillData.OnSkillActivated += OnSkillStateChanged;
                _skillData.OnSkillDeactivated += OnSkillStateChanged;
            }
        }
        
        UpdateUI();
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        if (_button != null)
            _button.onClick.RemoveListener(OnButtonClicked);
            
        if (_skillData != null)
        {
            _skillData.OnSkillActivated -= OnSkillStateChanged;
            _skillData.OnSkillDeactivated -= OnSkillStateChanged;
        }
    }
    
    /// <summary>
    /// Called when the button is clicked
    /// </summary>
    private void OnButtonClicked()
    {
        if (_skillSystem != null)
        {
            // Toggle the skill
            _skillSystem.ToggleSkill(_skillType);
            UpdateUI();
        }
    }
    
    /// <summary>
    /// Called when skill state changes
    /// </summary>
    private void OnSkillStateChanged(SkillData skill)
    {
        UpdateUI();
    }
    
    /// <summary>
    /// Updates button UI based on skill state
    /// </summary>
    private void UpdateUI()
    {
        if (_skillData == null) return;
        
        // Check if skill is active
        if (_skillData.IsActive)
        {
            // Check if it's the rage mode skill and if it's in active state
            if (_skillType == GameEnums.SkillType.RageMode && 
                _skillData is RageModeSkill rageSkill && 
                rageSkill.IsRageModeActive)
            {
                _backgroundImage.color = _rageActiveColor;
            }
            else
            {
                _backgroundImage.color = _activeColor;
            }
        }
        else
        {
            _backgroundImage.color = _inactiveColor;
        }
    }
} 