using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Runtime UI panel for testing the skill system during gameplay
/// </summary>
public class SkillTestPanel : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerSkillSystem _skillSystem;
    [SerializeField] private GameObject _skillButtonPrefab;
    [SerializeField] private Transform _skillButtonContainer;
    [SerializeField] private Button _activateAllButton;
    [SerializeField] private Button _deactivateAllButton;
    [SerializeField] private Button _togglePanelButton;
    [SerializeField] private GameObject _panelContent;
    
    // Public properties for editor access
    public GameObject SkillButtonPrefab { get => _skillButtonPrefab; set => _skillButtonPrefab = value; }
    public Transform SkillButtonContainer { get => _skillButtonContainer; set => _skillButtonContainer = value; }
    public Button ActivateAllButton { get => _activateAllButton; set => _activateAllButton = value; }
    public Button DeactivateAllButton { get => _deactivateAllButton; set => _deactivateAllButton = value; }
    public Button TogglePanelButton { get => _togglePanelButton; set => _togglePanelButton = value; }
    public GameObject PanelContent { get => _panelContent; set => _panelContent = value; }
    public PlayerSkillSystem SkillSystem { get => _skillSystem; set => _skillSystem = value; }
    
    [Header("Settings")]
    [SerializeField] private bool _showOnStart = true;
    
    private Dictionary<GameEnums.SkillType, SkillButtonData> _skillButtons = new Dictionary<GameEnums.SkillType, SkillButtonData>();
    
    private void Awake()
    {
        if (_skillSystem == null)
        {
            _skillSystem = FindObjectOfType<PlayerSkillSystem>();
        }
        
        // Set up button listeners
        if (_activateAllButton != null)
        {
            _activateAllButton.onClick.AddListener(ActivateAllSkills);
        }
        
        if (_deactivateAllButton != null)
        {
            _deactivateAllButton.onClick.AddListener(DeactivateAllSkills);
        }
        
        if (_togglePanelButton != null)
        {
            _togglePanelButton.onClick.AddListener(TogglePanel);
        }
    }
    
    private void Start()
    {
        // Initialize the panel
        if (_panelContent != null)
        {
            _panelContent.SetActive(_showOnStart);
        }
        
        // Create skill buttons
        CreateSkillButtons();
    }
    
    private void Update()
    {
        // Update skill button states
        UpdateSkillButtonStates();
    }
    
    /// <summary>
    /// Creates UI buttons for each skill
    /// </summary>
    private void CreateSkillButtons()
    {
        if (_skillSystem == null || _skillButtonPrefab == null || _skillButtonContainer == null)
            return;
        
        // Clear existing buttons
        foreach (Transform child in _skillButtonContainer)
        {
            Destroy(child.gameObject);
        }
        
        _skillButtons.Clear();
        
        // Get all available skills
        foreach (GameEnums.SkillType skillType in System.Enum.GetValues(typeof(GameEnums.SkillType)))
        {
            SkillData skill = _skillSystem.GetSkill(skillType);
            if (skill != null)
            {
                // Create button
                GameObject buttonObj = Instantiate(_skillButtonPrefab, _skillButtonContainer);
                SkillButtonData buttonData = new SkillButtonData();
                
                // Get UI components
                buttonData.Button = buttonObj.GetComponent<Button>();
                buttonData.NameText = buttonObj.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
                buttonData.StatusText = buttonObj.transform.Find("StatusText")?.GetComponent<TextMeshProUGUI>();
                buttonData.IconImage = buttonObj.transform.Find("Icon")?.GetComponent<Image>();
                
                // Set initial values
                if (buttonData.NameText != null)
                {
                    buttonData.NameText.text = skill.SkillName;
                }
                
                if (buttonData.IconImage != null && skill.Icon != null)
                {
                    buttonData.IconImage.sprite = skill.Icon;
                }
                
                // Set button action
                GameEnums.SkillType currentSkillType = skillType; // Capture for lambda
                if (buttonData.Button != null)
                {
                    buttonData.Button.onClick.AddListener(() => ToggleSkill(currentSkillType));
                }
                
                // Store button data
                _skillButtons[skillType] = buttonData;
            }
        }
    }
    
    /// <summary>
    /// Updates the UI state of all skill buttons
    /// </summary>
    private void UpdateSkillButtonStates()
    {
        if (_skillSystem == null)
            return;
        
        foreach (var kvp in _skillButtons)
        {
            GameEnums.SkillType skillType = kvp.Key;
            SkillButtonData buttonData = kvp.Value;
            
            SkillData skill = _skillSystem.GetSkill(skillType);
            if (skill != null)
            {
                // Update status text
                if (buttonData.StatusText != null)
                {
                    string statusText = skill.IsActive ? "ACTIVE" : "INACTIVE";
                    
                    // Add extra info for specific skills
                    switch (skillType)
                    {
                        case GameEnums.SkillType.ArrowMultiplication:
                            if (skill is ArrowMultiplicationSkill arrowSkill && skill.IsActive)
                            {
                                statusText += $" ({arrowSkill.GetArrowCount()} arrows)";
                            }
                            break;
                            
                        case GameEnums.SkillType.BounceDamage:
                            if (skill is BounceDamageSkill bounceSkill && skill.IsActive)
                            {
                                statusText += $" ({bounceSkill.GetBounceCount()} bounces)";
                            }
                            break;
                            
                        case GameEnums.SkillType.RageMode:
                            if (skill is RageModeSkill rageSkill && skill.IsActive)
                            {
                                statusText += rageSkill.IsRageModeActive ? " (RAGE ON)" : " (charging)";
                            }
                            break;
                    }
                    
                    buttonData.StatusText.text = statusText;
                    buttonData.StatusText.color = skill.IsActive ? Color.green : Color.red;
                }
                
                // Update button color
                if (buttonData.Button != null)
                {
                    ColorBlock colors = buttonData.Button.colors;
                    colors.normalColor = skill.IsActive ? new Color(0.8f, 1f, 0.8f) : new Color(1f, 0.8f, 0.8f);
                    buttonData.Button.colors = colors;
                }
            }
        }
    }
    
    /// <summary>
    /// Toggles a skill on/off
    /// </summary>
    public void ToggleSkill(GameEnums.SkillType skillType)
    {
        if (_skillSystem != null)
        {
            _skillSystem.ToggleSkill(skillType);
        }
    }
    
    /// <summary>
    /// Activates all skills
    /// </summary>
    public void ActivateAllSkills()
    {
        if (_skillSystem == null)
            return;
        
        foreach (GameEnums.SkillType skillType in System.Enum.GetValues(typeof(GameEnums.SkillType)))
        {
            SkillData skill = _skillSystem.GetSkill(skillType);
            if (skill != null && !skill.IsActive)
            {
                _skillSystem.ToggleSkill(skillType);
            }
        }
    }
    
    /// <summary>
    /// Deactivates all skills
    /// </summary>
    public void DeactivateAllSkills()
    {
        if (_skillSystem == null)
            return;
        
        foreach (GameEnums.SkillType skillType in System.Enum.GetValues(typeof(GameEnums.SkillType)))
        {
            SkillData skill = _skillSystem.GetSkill(skillType);
            if (skill != null && skill.IsActive)
            {
                _skillSystem.ToggleSkill(skillType);
            }
        }
    }
    
    /// <summary>
    /// Toggles the panel visibility
    /// </summary>
    public void TogglePanel()
    {
        if (_panelContent != null)
        {
            _panelContent.SetActive(!_panelContent.activeSelf);
        }
    }
    
    /// <summary>
    /// Helper class to store UI components for each skill button
    /// </summary>
    private class SkillButtonData
    {
        public Button Button;
        public TextMeshProUGUI NameText;
        public TextMeshProUGUI StatusText;
        public Image IconImage;
    }
} 