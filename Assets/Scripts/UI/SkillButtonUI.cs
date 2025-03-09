using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

/// <summary>
/// Represents a single skill button in the tab menu
/// </summary>
public class SkillButtonUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Button _button;
    [SerializeField] private Image _skillIcon;
    [SerializeField] private Image _outlineImage;
    [SerializeField] private TextMeshProUGUI _skillNameText;
    [SerializeField] private TextMeshProUGUI _descriptionText;
    [SerializeField] private TextMeshProUGUI _statusText;
    
    [Header("Animation")]
    [SerializeField] private float _pulseSpeed = 1.5f;
    [SerializeField] private float _minPulseScale = 0.95f;
    [SerializeField] private float _maxPulseScale = 1.05f;

    // Rage skill için UI öğeleri
    [Header("Rage Mode UI")]
    [SerializeField] private GameObject _rageTimerContainer;
    
    // References
    private SkillData _skill;
    private GameEnums.SkillType _skillType;
    private SkillTabUI _tabUI;
    
    // Butonun seçili durumu için değişken ekle
    private bool _isSelected = false;
    
    // Rage skill referansı
    private RageModeSkill _rageSkill;
    
    /// <summary>
    /// Initializes the button with skill data
    /// </summary>
    /// <param name="skill">The skill this button represents</param>
    /// <param name="tabUI">The parent tab UI</param>
    public void Initialize(SkillData skill, SkillTabUI tabUI)
    {
        _skill = skill;
        _skillType = skill.SkillType;
        _tabUI = tabUI;
        
        // Get button component if not assigned
        if (_button == null)
            _button = GetComponent<Button>();
            
        // Set up button click handler
        if (_button != null)
        {
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(OnButtonClicked);
        }
        
        // Set up UI elements
        if (_skillNameText != null)
            _skillNameText.text = skill.SkillName;
            
        if (_descriptionText != null)
            _descriptionText.text = skill.Description;
            
        if (_skillIcon != null && skill.Icon != null)
            _skillIcon.sprite = skill.Icon;
        
        // Rage skill ise, özel UI öğelerini ayarla
        if (skill is RageModeSkill rageSkill)
        {
            _rageSkill = rageSkill;
            GameEvents.OnRageModeStateChanged += OnRageModeStateChanged;
            
            // Rage modu UI'ını göster/gizle
            if (_rageTimerContainer != null)
                _rageTimerContainer.SetActive(false); // Artık zamanlayıcı kullanmadığımız için gizliyoruz
        }
        else
        {
            // Rage skill değilse UI'ı gizle
            if (_rageTimerContainer != null)
                _rageTimerContainer.SetActive(false);
        }
            
        // Initial visual state
        UpdateVisualState();
    }
    
    /// <summary>
    /// Called when the button is clicked
    /// </summary>
    private void OnButtonClicked()
    {
        if (_tabUI != null)
        {
            // Tıklandığında yeteneği aktifleştir/deaktifleştir
            _tabUI.ToggleSkill(_skillType);
        }
    }
    
    private void OnDestroy()
    {
        // Rage skill olay aboneliklerini temizle
        if (_rageSkill != null)
        {
            GameEvents.OnRageModeStateChanged -= OnRageModeStateChanged;
        }
    }
    
    /// <summary>
    /// Rage modunun durumu değiştiğinde çağrılır
    /// </summary>
    private void OnRageModeStateChanged(bool isActive)
    {
        // Rage modu durumu değiştiğinde görsel durumu güncelle
        UpdateVisualState();
    }
    
    /// <summary>
    /// Butonun görsel durumunu günceller
    /// </summary>
    public void UpdateVisualState()
    {
        // Skill durumunu kontrol et
        bool isActive = _skill != null && _skill.IsActive;
        
        // OutlineImage durumunu güncelle
        if (_outlineImage != null)
        {
            _outlineImage.gameObject.SetActive(isActive || _isSelected);
            
            if (isActive && _tabUI != null)
            {
                _outlineImage.color = _tabUI.GetActiveOutlineColor();
            }
            else if (_isSelected)
            {
                _outlineImage.color = Color.white;
            }
        }
        
        // Status text durumunu güncelle
        if (_statusText != null)
        {
            if (isActive)
            {
                _statusText.text = "ACTIVE";
                _statusText.color = Color.green;
            }
            else
            {
                _statusText.text = "SELECT";
                _statusText.color = Color.white;
            }
        }
    }

    /// <summary>
    /// Butonun seçili durumunu ayarlar
    /// </summary>
    /// <param name="selected">Yeni seçim durumu</param>
    public void SetSelected(bool selected)
    {
        _isSelected = selected;
        
        // Seçili olduğunda görsel değişiklikler yap
        if (_button != null)
        {
            ColorBlock colors = _button.colors;
            
            if (_isSelected)
            {
                // Seçili durum rengi - daha parlak yeşil
                colors.normalColor = new Color(0.7f, 1f, 0.7f);
                
                // Buton etrafına DOTween ile parlama efekti ekle
                if (_outlineImage != null)
                {
                    // Mevcut DOTween animasyonunu durdur
                    DOTween.Kill(_outlineImage);
                    
                    // Outline görünürlüğünü aktif et
                    _outlineImage.enabled = true;
                    _outlineImage.color = Color.green; // Yeşil outline
                    
                    // Pulse efekti
                    Sequence pulseSequence = DOTween.Sequence();
                    pulseSequence.Append(_outlineImage.DOFade(0.9f, 0.3f));
                    pulseSequence.Append(_outlineImage.DOFade(0.5f, 0.3f));
                    pulseSequence.SetLoops(-1);
                    
                    // Buton ölçeğini biraz büyüt
                    transform.DOScale(1.05f, 0.2f).SetEase(Ease.OutQuad);
                }
                
                // Status text'i güncelle
                if (_statusText != null)
                {
                    _statusText.text = "ACTIVE";
                    _statusText.color = Color.green;
                }
            }
            else
            {
                // Normal durum - tüm butonlar pasif görünümde
                colors.normalColor = Color.white;
                
                // Outline'ı gizle
                if (_outlineImage != null)
                {
                    DOTween.Kill(_outlineImage);
                    _outlineImage.enabled = false;
                }
                
                // Normal boyuta geri dön
                transform.DOScale(1f, 0.2f).SetEase(Ease.OutQuad);
                
                // Status text'i güncelle
                if (_statusText != null)
                {
                    _statusText.text = "INACTIVE";
                    _statusText.color = Color.gray;
                }
            }
            
            _button.colors = colors;
        }
    }

} 