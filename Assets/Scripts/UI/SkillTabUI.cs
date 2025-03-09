using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// Skill tab menu that can be opened and closed during gameplay
/// Allows toggling skills on and off
/// </summary>
public class SkillTabUI : MonoBehaviour
{
    [Header("UI References")] [SerializeField]
    private GameObject _menuPanel;

    [SerializeField] private Transform _skillButtonContainer;
    [SerializeField] private SkillButtonUI _skillButtonPrefab;
    [SerializeField] private Button _toggleMenuButton;
    [SerializeField] private Button _closeMenuButton;

    [Header("Visual Settings")] [SerializeField]
    private Color _activeSkillOutlineColor = new Color(0.2f, 0.8f, 0.2f);

    // References
    private Dictionary<GameEnums.SkillType, SkillButtonUI> _skillButtons =
        new Dictionary<GameEnums.SkillType, SkillButtonUI>();

    private PlayerSkillSystem _skillSystem;

    // Seçili buton için değişken ekle
    private SkillButtonUI _selectedButton;
    private GameEnums.SkillType? _selectedSkillType;

    private void Awake()
    {
        // Register button callbacks
        if (_toggleMenuButton != null)
            _toggleMenuButton.onClick.AddListener(ToggleMenu);

        if (_closeMenuButton != null)
            _closeMenuButton.onClick.AddListener(CloseMenu);

        // Make sure menu is initially closed
        if (_menuPanel != null)
            _menuPanel.SetActive(false);
    }

    private void Start()
    {
        // Find the player skill system
        _skillSystem = FindFirstObjectByType<PlayerSkillSystem>();

        if (_skillSystem == null)
        {
            Debug.LogError("SkillTabUI: PlayerSkillSystem not found!");
            return;
        }

        // Create skill buttons
        CreateSkillButtons();

        // Başlangıçta buton durumlarını beceri durumlarıyla senkronize et
        SynchronizeButtonsWithSkillStates();
    }

    private void OnDestroy()
    {
        // Clean up button callbacks
        if (_toggleMenuButton != null)
            _toggleMenuButton.onClick.RemoveListener(ToggleMenu);

        if (_closeMenuButton != null)
            _closeMenuButton.onClick.RemoveListener(CloseMenu);
    }

    /// <summary>
    /// Creates buttons for all available skills
    /// </summary>
    private void CreateSkillButtons()
    {
        if (_skillSystem == null || _skillButtonContainer == null || _skillButtonPrefab == null)
            return;

        // Clear any existing buttons
        foreach (Transform child in _skillButtonContainer)
        {
            Destroy(child.gameObject);
        }

        _skillButtons.Clear();

        // Create buttons for each skill type
        foreach (GameEnums.SkillType skillType in System.Enum.GetValues(typeof(GameEnums.SkillType)))
        {
            SkillData skill = _skillSystem.GetSkill(skillType);
            if (skill != null)
            {
                // Create button instance
                SkillButtonUI buttonUI = Instantiate(_skillButtonPrefab, _skillButtonContainer);

                // Initialize with skill data
                buttonUI.Initialize(skill, this);

                // Store reference
                _skillButtons[skillType] = buttonUI;
            }
        }
    }

    /// <summary>
    /// Toggles a skill on/off with special handling for Rage Mode:
    /// - Rage Mode can be active alongside one other skill
    /// - Only one non-Rage skill can be active at a time
    /// - Toggling off Rage Mode only deactivates Rage Mode itself
    /// </summary>
    /// <param name="skillType">The skill type to toggle</param>
    public void ToggleSkill(GameEnums.SkillType skillType)
    {
        if (_skillSystem == null) return;

        bool isRageSkill = (skillType == GameEnums.SkillType.RageMode);
        bool isRageActive = _skillSystem.IsSkillActive(GameEnums.SkillType.RageMode);
        bool isSkillActive = _skillSystem.IsSkillActive(skillType);

        // Case 1: Toggling off an active skill
        if (isSkillActive)
        {
            _skillSystem.ToggleSkill(skillType);

            // If turning off Rage Mode, only clear its selection
            if (isRageSkill)
            {
                if (_selectedSkillType.HasValue && _selectedSkillType.Value == GameEnums.SkillType.RageMode)
                {
                    ClearSelection();
                }
            }
            else
            {
                // Just clear the selection for the deactivated skill
                if (_selectedSkillType.HasValue && _selectedSkillType.Value == skillType)
                {
                    ClearSelection();
                }
            }

            UpdateAllButtonStates();
            return;
        }

        // Case 2: Activating a new skill

        // If activating a non-Rage skill
        if (!isRageSkill)
        {
            // Deactivate any other non-Rage skills
            foreach (GameEnums.SkillType type in System.Enum.GetValues(typeof(GameEnums.SkillType)))
            {
                if (type != GameEnums.SkillType.RageMode &&
                    type != skillType &&
                    _skillSystem.IsSkillActive(type))
                {
                    _skillSystem.ToggleSkill(type);
                }
            }
        }

        // Activate the selected skill
        _skillSystem.ToggleSkill(skillType);
        SelectButton(skillType);
        UpdateAllButtonStates();

        Debug.Log(
            $"[SkillTabUI] Toggled {skillType}. Rage Active: {isRageActive}, Selected Skill: {_selectedSkillType}");
    }

    /// <summary>
    /// Deactivates all skills except the specified skill type
    /// </summary>
    /// <param name="exceptSkillType">The skill type to preserve (null to deactivate all)</param>
    private void DeactivateAllSkillsExcept(GameEnums.SkillType? exceptSkillType)
    {
        if (_skillSystem == null) return;

        foreach (GameEnums.SkillType type in System.Enum.GetValues(typeof(GameEnums.SkillType)))
        {
            // Skip the specified skill type
            if (exceptSkillType.HasValue && type == exceptSkillType.Value)
                continue;

            SkillData skill = _skillSystem.GetSkill(type);
            if (skill != null && skill.IsActive)
            {
                _skillSystem.ToggleSkill(type);
                UpdateSkillButtonState(type);
            }
        }
    }

    /// <summary>
    /// Synchronizes button states with active skills, allowing multiple selections for Rage Mode
    /// </summary>
    private void SynchronizeButtonsWithSkillStates()
    {
        if (_skillSystem == null) return;

        // Clear all selections first
        ClearSelection();

        bool hasSelectedNonRageSkill = false;

        // First, check and select Rage Mode if it's active
        if (_skillSystem.IsSkillActive(GameEnums.SkillType.RageMode))
        {
            SelectButton(GameEnums.SkillType.RageMode);
        }

        // Then check other skills
        foreach (GameEnums.SkillType skillType in System.Enum.GetValues(typeof(GameEnums.SkillType)))
        {
            if (skillType == GameEnums.SkillType.RageMode)
                continue;

            if (_skillSystem.IsSkillActive(skillType) && _skillButtons.TryGetValue(skillType, out SkillButtonUI button))
            {
                // If this is a non-Rage skill and it's active, select it
                SelectButton(skillType);
                hasSelectedNonRageSkill = true;
                break; // Only one non-Rage skill can be active
            }
        }

        // Update all button visual states
        UpdateAllButtonStates();

        Debug.Log(
            $"[SkillTabUI] Button states synchronized. Rage Mode: {_skillSystem.IsSkillActive(GameEnums.SkillType.RageMode)}, Other Skill Selected: {hasSelectedNonRageSkill}");
    }

    /// <summary>
    /// Selects a button without clearing previous selections if it's a Rage Mode combination
    /// </summary>
    public void SelectButton(GameEnums.SkillType skillType)
    {
        bool isRageSkill = (skillType == GameEnums.SkillType.RageMode);
        if (!isRageSkill && _selectedSkillType.HasValue && _selectedSkillType.Value != GameEnums.SkillType.RageMode)
        {
            if (_skillButtons.TryGetValue(_selectedSkillType.Value, out SkillButtonUI previousButton))
            {
                previousButton.SetSelected(false);
            }
        }

        // Select the new button
        if (_skillButtons.TryGetValue(skillType, out SkillButtonUI button))
        {
            // If it's Rage Mode, don't clear other selections
            if (isRageSkill)
            {
                button.SetSelected(true);
                // Keep track of Rage Mode selection separately
                _selectedButton = button;
                _selectedSkillType = skillType;
            }
            else
            {
                // For non-Rage skills, update the selection normally
                _selectedButton = button;
                _selectedSkillType = skillType;
                button.SetSelected(true);
            }
        }
    }

    /// <summary>
    /// Tüm butonların seçili olmama durumunu ayarlar
    /// </summary>
    public void ClearSelection()
    {
        if (_selectedSkillType.HasValue &&
            _skillButtons.TryGetValue(_selectedSkillType.Value, out SkillButtonUI button))
        {
            button.SetSelected(false);
        }

        _selectedButton = null;
        _selectedSkillType = null;
    }

    /// <summary>
    /// Updates a skill button's visual state
    /// </summary>
    /// <param name="skillType">The type of skill to update</param>
    private void UpdateSkillButtonState(GameEnums.SkillType skillType)
    {
        if (_skillButtons.TryGetValue(skillType, out SkillButtonUI button))
        {
            button.UpdateVisualState();
        }
    }

    /// <summary>
    /// Tüm butonların durumunu aktif beceri durumlarıyla eşleştirir
    /// </summary>
    private void UpdateAllButtonStates()
    {
        foreach (var button in _skillButtons.Values)
        {
            button.UpdateVisualState();
        }
    }

    /// <summary>
    /// Toggles the menu open/closed
    /// </summary>
    public void ToggleMenu()
    {
        if (_menuPanel != null)
        {
            bool isActive = _menuPanel.activeSelf;
            _menuPanel.SetActive(!isActive);

            // Menü açılıyorsa buton durumlarını senkronize et
            if (!isActive)
            {
                SynchronizeButtonsWithSkillStates();
            }
        }
    }

    /// <summary>
    /// Closes the menu
    /// </summary>
    public void CloseMenu()
    {
        if (_menuPanel != null)
        {
            _menuPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Returns the color to use for active skill outlines
    /// </summary>
    public Color GetActiveOutlineColor()
    {
        return _activeSkillOutlineColor;
    }
}