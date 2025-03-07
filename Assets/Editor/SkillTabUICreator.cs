using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using System.IO;
using UnityEngine.Events;

/// <summary>
/// Creates a fully configured Skill Tab UI system with a single click
/// </summary>
public class SkillTabUICreator : Editor
{
    // UI yapılandırma ayarları
    private static readonly Color PanelBackgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.9f);
    private static readonly Color HeaderColor = new Color(0.15f, 0.15f, 0.15f, 1f);
    private static readonly Color ActiveSkillOutlineColor = new Color(0f, 0.8f, 0.2f);
    private static readonly Color ButtonColor = new Color(0.9f, 0.9f, 0.9f);
    private static readonly Color IconBackgroundColor = new Color(0.2f, 0.2f, 0.2f);
    
    private static readonly Vector2 TabPanelSize = new Vector2(400f, 600f);
    private static readonly Vector2 ToggleButtonSize = new Vector2(50f, 50f);
    private static readonly Vector2 CloseButtonSize = new Vector2(40f, 40f);
    private static readonly Vector2 SkillButtonSize = new Vector2(380f, 100f);
    private static readonly Vector2 IconSize = new Vector2(64f, 64f);
    
    private static readonly string PrefabsPath = "Assets/Prefabs/UI";
    private static readonly string ResourcesPath = "Assets/Resources";
    private static readonly string SpritesPath = "Assets/Sprites/UI";
    
    [MenuItem("Tools/UI/Create Skill Tab UI System", false, 100)]
    public static void CreateSkillTabUISystem()
    {
        // Gerekli klasörleri oluştur
        EnsureDirectoriesExist();
        
        // Temel sprite'ları oluştur
        Sprite roundedRectSprite = CreateRoundedRectSprite();
        Sprite circleSprite = CreateCircleSprite();
        
        // Canvas oluşturma
        Canvas canvas = FindOrCreateCanvas();
        
        // Ana paneli oluştur
        GameObject skillTabPanel = CreateSkillPanel(canvas.transform, roundedRectSprite);
        
        // Menü butonunu oluştur
        GameObject toggleButton = CreateToggleButton(canvas.transform, roundedRectSprite, skillTabPanel);
        
        // Örnek beceri butonları oluştur ve SkillButtonUI scriptlerini ekle
        GameObject[] skillButtons = CreateSkillButtons(skillTabPanel, roundedRectSprite);
        
        // SkillTabUI scriptini ekle ve referansları ayarla
        ApplySkillTabUIScript(skillTabPanel, toggleButton, skillButtons);
        
        // Prefabları güncellediğimizden emin olalım
        UpdatePrefabs(skillTabPanel, toggleButton);
        
        // Kaydet ve kullanıcıya bildir
        Debug.Log("Skill Tab UI sistemi başarıyla oluşturuldu!");
        
        // Oluşturulan paneli seç
        Selection.activeGameObject = skillTabPanel;
        EditorGUIUtility.PingObject(skillTabPanel);
    }
    
    private static void EnsureDirectoriesExist()
    {
        if (!Directory.Exists(PrefabsPath))
            Directory.CreateDirectory(PrefabsPath);
            
        if (!Directory.Exists(ResourcesPath))
            Directory.CreateDirectory(ResourcesPath);
            
        if (!Directory.Exists(SpritesPath))
            Directory.CreateDirectory(SpritesPath);
    }
    
    private static Sprite CreateRoundedRectSprite()
    {
        // Önceden oluşturulmuş sprite'ı kontrol et
        string assetPath = SpritesPath + "/RoundedRect.png";
        Sprite existingSprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        if (existingSprite != null)
            return existingSprite;
            
        // Yeni texture oluştur
        Texture2D texture = new Texture2D(128, 128, TextureFormat.RGBA32, false);
        FillRoundedRect(texture, 20);
        
        // Texture'ı kaydet
        byte[] pngData = texture.EncodeToPNG();
        File.WriteAllBytes(assetPath, pngData);
        AssetDatabase.Refresh();
        
        // Texture import ayarlarını güncelle
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePivot = new Vector2(0.5f, 0.5f);
            importer.spritePixelsPerUnit = 100;
            importer.filterMode = FilterMode.Bilinear;
            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();
        }
        
        return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
    }
    
    private static Sprite CreateCircleSprite()
    {
        // Önceden oluşturulmuş sprite'ı kontrol et
        string assetPath = SpritesPath + "/Circle.png";
        Sprite existingSprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        if (existingSprite != null)
            return existingSprite;
            
        // Yeni texture oluştur
        Texture2D texture = new Texture2D(128, 128, TextureFormat.RGBA32, false);
        FillCircle(texture);
        
        // Texture'ı kaydet
        byte[] pngData = texture.EncodeToPNG();
        File.WriteAllBytes(assetPath, pngData);
        AssetDatabase.Refresh();
        
        // Texture import ayarlarını güncelle
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePivot = new Vector2(0.5f, 0.5f);
            importer.spritePixelsPerUnit = 100;
            importer.filterMode = FilterMode.Bilinear;
            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();
        }
        
        return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
    }
    
    private static void FillRoundedRect(Texture2D texture, float radius)
    {
        Color[] pixels = new Color[texture.width * texture.height];
        float w = texture.width;
        float h = texture.height;
        float r = Mathf.Min(radius, Mathf.Min(w, h) / 2f);
        
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                // Her köşeye göre pozisyonu kontrol et
                float dx = 0, dy = 0;
                
                // Sol üst köşe
                if (x < r && y < r)
                {
                    dx = r - x;
                    dy = r - y;
                }
                // Sağ üst köşe
                else if (x > w - r && y < r)
                {
                    dx = x - (w - r);
                    dy = r - y;
                }
                // Sol alt köşe
                else if (x < r && y > h - r)
                {
                    dx = r - x;
                    dy = y - (h - r);
                }
                // Sağ alt köşe
                else if (x > w - r && y > h - r)
                {
                    dx = x - (w - r);
                    dy = y - (h - r);
                }
                
                // Köşelerde, köşeye olan uzaklığa göre alpha değerini belirle
                float distance = Mathf.Sqrt(dx * dx + dy * dy);
                float alpha = distance > r ? 0f : 1f;
                
                // Kenarlardan biraz uzaklaştıysa, yumuşak kenar oluştur
                if (distance > r - 1f && distance < r)
                {
                    alpha = 1f - (distance - (r - 1f));
                }
                
                pixels[y * (int)w + x] = new Color(1, 1, 1, alpha);
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
    }
    
    private static void FillCircle(Texture2D texture)
    {
        Color[] pixels = new Color[texture.width * texture.height];
        float w = texture.width;
        float h = texture.height;
        float r = Mathf.Min(w, h) / 2f;
        Vector2 center = new Vector2(w / 2f, h / 2f);
        
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                float alpha = distance > r ? 0f : 1f;
                
                // Kenardan biraz uzaklaştıysa, yumuşak kenar oluştur
                if (distance > r - 1f && distance < r)
                {
                    alpha = 1f - (distance - (r - 1f));
                }
                
                pixels[y * (int)w + x] = new Color(1, 1, 1, alpha);
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
    }
    
    private static Canvas FindOrCreateCanvas()
    {
        // Mevcut canvas'ı ara
        Canvas canvas = GameObject.FindObjectOfType<Canvas>();
        
        // Canvas bulunamazsa, yeni bir tane oluştur
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("SkillUICanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            // CanvasScaler ekle
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            
            // GraphicRaycaster ekle
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
        return canvas;
    }
    
    // -------------------------------------------------
    // Yeni basitleştirilmiş yaklaşım
    // -------------------------------------------------
    
    private static GameObject CreateSkillPanel(Transform parent, Sprite backgroundSprite)
    {
        // Temel panel
        GameObject panelObj = new GameObject("SkillTabPanel");
        panelObj.transform.SetParent(parent, false);
        
        // RectTransform
        RectTransform rectTransform = panelObj.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(1, 0.5f);
        rectTransform.anchorMax = new Vector2(1, 0.5f);
        rectTransform.pivot = new Vector2(1, 0.5f);
        rectTransform.anchoredPosition = new Vector2(0, 0);
        rectTransform.sizeDelta = TabPanelSize;
        
        // Arka plan
        Image image = panelObj.AddComponent<Image>();
        image.sprite = backgroundSprite;
        image.color = PanelBackgroundColor;
        image.type = Image.Type.Sliced;
        
        // Header
        GameObject headerObj = new GameObject("Header");
        headerObj.transform.SetParent(panelObj.transform, false);
        
        RectTransform headerRect = headerObj.AddComponent<RectTransform>();
        headerRect.anchorMin = new Vector2(0, 1);
        headerRect.anchorMax = new Vector2(1, 1);
        headerRect.pivot = new Vector2(0.5f, 1);
        headerRect.anchoredPosition = new Vector2(0, 0);
        headerRect.sizeDelta = new Vector2(0, 60);
        
        Image headerImage = headerObj.AddComponent<Image>();
        headerImage.sprite = backgroundSprite;
        headerImage.color = HeaderColor;
        headerImage.type = Image.Type.Sliced;
        
        // Header title
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(headerObj.transform, false);
        
        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 0);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.pivot = new Vector2(0.5f, 0.5f);
        titleRect.offsetMin = new Vector2(20, 10);
        titleRect.offsetMax = new Vector2(-60, -10);
        
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "SKILLS";
        titleText.color = Color.white;
        titleText.alignment = TextAlignmentOptions.Left;
        titleText.fontSize = 24;
        titleText.fontStyle = FontStyles.Bold;
        
        // Close button
        GameObject closeButtonObj = new GameObject("CloseButton");
        closeButtonObj.transform.SetParent(headerObj.transform, false);
        
        RectTransform closeButtonRect = closeButtonObj.AddComponent<RectTransform>();
        closeButtonRect.anchorMin = new Vector2(1, 0.5f);
        closeButtonRect.anchorMax = new Vector2(1, 0.5f);
        closeButtonRect.pivot = new Vector2(1, 0.5f);
        closeButtonRect.anchoredPosition = new Vector2(-10, 0);
        closeButtonRect.sizeDelta = CloseButtonSize;
        
        Image closeButtonImage = closeButtonObj.AddComponent<Image>();
        closeButtonImage.sprite = backgroundSprite;
        closeButtonImage.color = new Color(0.8f, 0.2f, 0.2f);
        closeButtonImage.type = Image.Type.Sliced;
        
        Button closeButton = closeButtonObj.AddComponent<Button>();
        
        // X simgesi
        GameObject closeTextObj = new GameObject("Text");
        closeTextObj.transform.SetParent(closeButtonObj.transform, false);
        
        RectTransform closeTextRect = closeTextObj.AddComponent<RectTransform>();
        closeTextRect.anchorMin = Vector2.zero;
        closeTextRect.anchorMax = Vector2.one;
        closeTextRect.offsetMin = Vector2.zero;
        closeTextRect.offsetMax = Vector2.zero;
        
        TextMeshProUGUI closeText = closeTextObj.AddComponent<TextMeshProUGUI>();
        closeText.text = "X";
        closeText.color = Color.white;
        closeText.alignment = TextAlignmentOptions.Center;
        closeText.fontSize = 24;
        closeText.fontStyle = FontStyles.Bold;
        
        // ScrollView
        GameObject scrollViewObj = new GameObject("ScrollView");
        scrollViewObj.transform.SetParent(panelObj.transform, false);
        
        RectTransform scrollViewRect = scrollViewObj.AddComponent<RectTransform>();
        scrollViewRect.anchorMin = new Vector2(0, 0);
        scrollViewRect.anchorMax = new Vector2(1, 1);
        scrollViewRect.offsetMin = new Vector2(10, 10);
        scrollViewRect.offsetMax = new Vector2(-10, -70);
        
        ScrollRect scrollRect = scrollViewObj.AddComponent<ScrollRect>();
        
        // Viewport
        GameObject viewportObj = new GameObject("Viewport");
        viewportObj.transform.SetParent(scrollViewObj.transform, false);
        
        RectTransform viewportRect = viewportObj.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = Vector2.zero;
        
        Image viewportImage = viewportObj.AddComponent<Image>();
        viewportImage.color = new Color(1, 1, 1, 0.01f);
        
        Mask viewportMask = viewportObj.AddComponent<Mask>();
        viewportMask.showMaskGraphic = false;
        
        // Content (Button Container)
        GameObject contentObj = new GameObject("ButtonContainer");
        contentObj.transform.SetParent(viewportObj.transform, false);
        
        RectTransform contentRect = contentObj.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(0, 0);
        
        VerticalLayoutGroup contentLayout = contentObj.AddComponent<VerticalLayoutGroup>();
        contentLayout.padding = new RectOffset(5, 5, 5, 5);
        contentLayout.spacing = 10;
        contentLayout.childAlignment = TextAnchor.UpperCenter;
        contentLayout.childControlWidth = true;
        contentLayout.childControlHeight = false;
        contentLayout.childForceExpandWidth = true;
        contentLayout.childForceExpandHeight = false;
        
        ContentSizeFitter contentFitter = contentObj.AddComponent<ContentSizeFitter>();
        contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        // ScrollRect yapılandırması
        scrollRect.content = contentRect;
        scrollRect.viewport = viewportRect;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.scrollSensitivity = 10;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        
        // Gösterilmeyi kapatmak için
        panelObj.SetActive(false);
        
        // Close butonuna panel kapatma işlevi ekle
        closeButton.onClick.AddListener(() => { panelObj.SetActive(false); });
        
        // Prefab olarak kaydet
        string panelPrefabPath = PrefabsPath + "/Skills/SkillTabPanel.prefab";
        if (!Directory.Exists(PrefabsPath + "/Skills"))
            Directory.CreateDirectory(PrefabsPath + "/Skills");
            
        PrefabUtility.SaveAsPrefabAsset(panelObj, panelPrefabPath);
        
        return panelObj;
    }
    
    private static GameObject CreateToggleButton(Transform parent, Sprite backgroundSprite, GameObject targetPanel)
    {
        // Buton
        GameObject buttonObj = new GameObject("ToggleMenuButton");
        buttonObj.transform.SetParent(parent, false);
        
        RectTransform rectTransform = buttonObj.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(1, 1);
        rectTransform.anchorMax = new Vector2(1, 1);
        rectTransform.pivot = new Vector2(1, 1);
        rectTransform.anchoredPosition = new Vector2(-20, -20);
        rectTransform.sizeDelta = ToggleButtonSize;
        
        Image image = buttonObj.AddComponent<Image>();
        image.sprite = backgroundSprite;
        image.color = new Color(0.2f, 0.6f, 0.9f);
        image.type = Image.Type.Sliced;
        
        Button button = buttonObj.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.2f, 0.6f, 0.9f);
        colors.highlightedColor = new Color(0.3f, 0.7f, 1f);
        colors.pressedColor = new Color(0.1f, 0.5f, 0.8f);
        button.colors = colors;
        
        // İkon
        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(buttonObj.transform, false);
        
        RectTransform iconRect = iconObj.AddComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0.5f, 0.5f);
        iconRect.anchorMax = new Vector2(0.5f, 0.5f);
        iconRect.pivot = new Vector2(0.5f, 0.5f);
        iconRect.anchoredPosition = Vector2.zero;
        iconRect.sizeDelta = new Vector2(24, 24);
        
        TextMeshProUGUI iconText = iconObj.AddComponent<TextMeshProUGUI>();
        iconText.text = "S";
        iconText.color = Color.white;
        iconText.alignment = TextAlignmentOptions.Center;
        iconText.fontSize = 24;
        iconText.fontStyle = FontStyles.Bold;
        
        // Panel aç/kapat işlevselliğini ekle
        button.onClick.AddListener(() => {
            if (targetPanel != null)
                targetPanel.SetActive(!targetPanel.activeSelf);
        });
        
        // Prefab olarak kaydet
        string buttonPrefabPath = PrefabsPath + "/Skills/ToggleMenuButton.prefab";
        if (!Directory.Exists(PrefabsPath + "/Skills"))
            Directory.CreateDirectory(PrefabsPath + "/Skills");
            
        PrefabUtility.SaveAsPrefabAsset(buttonObj, buttonPrefabPath);
        
        return buttonObj;
    }
    
    private static GameObject[] CreateSkillButtons(GameObject panelObj, Sprite backgroundSprite)
    {
        // ButtonContainer'ı bul
        Transform container = panelObj.transform.Find("ScrollView/Viewport/ButtonContainer");
        if (container == null)
        {
            Debug.LogError("ButtonContainer not found in panel hierarchy!");
            return new GameObject[0];
        }
        
        // Beceri butonları için bilgiler
        string[] skillNames = new string[] { "Multiple Arrows", "Bouncing Arrows", "Fire Damage", "Attack Speed" };
        string[] skillDescriptions = new string[] { 
            "Fires multiple arrows simultaneously", 
            "Arrows bounce between enemies", 
            "Burns enemies over time", 
            "Increases attack speed"
        };
        bool[] activeStates = new bool[] { false, true, false, false };
        
        // Enum değerleri - GameEnums.SkillType değerleriyle eşleştiğini varsayıyoruz
        int[] skillTypeValues = new int[] { 0, 1, 2, 3 }; // ArrowMultiplication, BounceDamage, BurnDamage, AttackSpeedIncrease
        
        // Butonları oluştur
        GameObject[] buttons = new GameObject[skillNames.Length];
        
        for (int i = 0; i < skillNames.Length; i++)
        {
            buttons[i] = CreateExampleSkillButton(container, backgroundSprite, skillNames[i], skillDescriptions[i], 
                activeStates[i], skillTypeValues[i]);
        }
        
        return buttons;
    }
    
    private static GameObject CreateExampleSkillButton(Transform parent, Sprite backgroundSprite, string skillName, 
        string description, bool isActive, int skillTypeValue)
    {
        // Button
        GameObject buttonObj = new GameObject("SkillButton_" + skillName.Replace(" ", ""));
        buttonObj.transform.SetParent(parent, false);
        
        RectTransform rectTransform = buttonObj.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(0, SkillButtonSize.y);
        
        Image backgroundImage = buttonObj.AddComponent<Image>();
        backgroundImage.sprite = backgroundSprite;
        backgroundImage.color = ButtonColor;
        backgroundImage.type = Image.Type.Sliced;
        
        Button button = buttonObj.AddComponent<Button>();
        
        // Outline
        GameObject outlineObj = new GameObject("Outline");
        outlineObj.transform.SetParent(buttonObj.transform, false);
        
        RectTransform outlineRect = outlineObj.AddComponent<RectTransform>();
        outlineRect.anchorMin = Vector2.zero;
        outlineRect.anchorMax = Vector2.one;
        outlineRect.offsetMin = new Vector2(-3, -3);
        outlineRect.offsetMax = new Vector2(3, 3);
        
        Image outlineImage = outlineObj.AddComponent<Image>();
        outlineImage.sprite = backgroundSprite;
        outlineImage.color = ActiveSkillOutlineColor;
        outlineImage.type = Image.Type.Sliced;
        outlineImage.enabled = isActive;
        
        // Icon
        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(buttonObj.transform, false);
        
        RectTransform iconRect = iconObj.AddComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0, 0.5f);
        iconRect.anchorMax = new Vector2(0, 0.5f);
        iconRect.pivot = new Vector2(0, 0.5f);
        iconRect.anchoredPosition = new Vector2(15, 0);
        iconRect.sizeDelta = IconSize;
        
        Image iconImage = iconObj.AddComponent<Image>();
        iconImage.sprite = backgroundSprite;
        iconImage.color = IconBackgroundColor;
        
        // Skill Name
        GameObject nameObj = new GameObject("SkillName");
        nameObj.transform.SetParent(buttonObj.transform, false);
        
        RectTransform nameRect = nameObj.AddComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 1);
        nameRect.anchorMax = new Vector2(1, 1);
        nameRect.pivot = new Vector2(0.5f, 1);
        nameRect.offsetMin = new Vector2(90, -30);
        nameRect.offsetMax = new Vector2(-10, -5);
        
        TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
        nameText.text = skillName;
        nameText.color = Color.black;
        nameText.alignment = TextAlignmentOptions.Left;
        nameText.fontSize = 18;
        nameText.fontStyle = FontStyles.Bold;
        
        // Description
        GameObject descObj = new GameObject("Description");
        descObj.transform.SetParent(buttonObj.transform, false);
        
        RectTransform descRect = descObj.AddComponent<RectTransform>();
        descRect.anchorMin = new Vector2(0, 0);
        descRect.anchorMax = new Vector2(1, 0.5f);
        descRect.pivot = new Vector2(0.5f, 0);
        descRect.offsetMin = new Vector2(90, 5);
        descRect.offsetMax = new Vector2(-70, 0);
        
        TextMeshProUGUI descText = descObj.AddComponent<TextMeshProUGUI>();
        descText.text = description;
        descText.color = Color.black;
        descText.alignment = TextAlignmentOptions.Left;
        descText.fontSize = 12;
        descText.enableWordWrapping = true;
        
        // Status
        GameObject statusObj = new GameObject("Status");
        statusObj.transform.SetParent(buttonObj.transform, false);
        
        RectTransform statusRect = statusObj.AddComponent<RectTransform>();
        statusRect.anchorMin = new Vector2(1, 0);
        statusRect.anchorMax = new Vector2(1, 0);
        statusRect.pivot = new Vector2(1, 0);
        statusRect.anchoredPosition = new Vector2(-10, 10);
        statusRect.sizeDelta = new Vector2(60, 24);
        
        TextMeshProUGUI statusText = statusObj.AddComponent<TextMeshProUGUI>();
        statusText.text = isActive ? "ACTIVE" : "INACTIVE";
        statusText.color = isActive ? Color.green : Color.gray;
        statusText.alignment = TextAlignmentOptions.Center;
        statusText.fontSize = 10;
        statusText.fontStyle = FontStyles.Bold;
        
        // SkillButtonUI scriptini ekle - önce normal buton işlevselliğini düzenle
        bool currentlyActive = isActive;
        button.onClick.AddListener(() => {
            currentlyActive = !currentlyActive;
            outlineImage.enabled = currentlyActive;
            statusText.text = currentlyActive ? "ACTIVE" : "INACTIVE";
            statusText.color = currentlyActive ? Color.green : Color.gray;
        });
        
        // SkillButtonUI scriptini ekle
        SkillButtonUI skillButtonUI = buttonObj.AddComponent<SkillButtonUI>();
        
        // Reflection API kullanmadan önce referansları direkt düzenlemek için
        // Script değişkenlerini Invoke ile ayarlayacağız
        EditorApplication.delayCall += () => {
            try {
                // Reflection API ile alanları ayarlama - bu EditMode'da çalışır
                System.Type type = skillButtonUI.GetType();
                
                // Değişkenleri ayarla - _button, _skillIcon, _outlineImage, _skillNameText, _descriptionText, _statusText
                SetFieldValue(skillButtonUI, "_button", button);
                SetFieldValue(skillButtonUI, "_skillIcon", iconImage);
                SetFieldValue(skillButtonUI, "_outlineImage", outlineImage);
                SetFieldValue(skillButtonUI, "_skillNameText", nameText);
                SetFieldValue(skillButtonUI, "_descriptionText", descText);
                SetFieldValue(skillButtonUI, "_statusText", statusText);
                
                // Animation değişkenlerini ayarla
                SetFieldValue(skillButtonUI, "_animateActiveSkills", true);
                SetFieldValue(skillButtonUI, "_pulseSpeed", 1.5f);
                SetFieldValue(skillButtonUI, "_minPulseScale", 0.95f);
                SetFieldValue(skillButtonUI, "_maxPulseScale", 1.05f);
                
                // SkillType'ı ayarla
                SetFieldValue(skillButtonUI, "_skillType", skillTypeValue);
                
                EditorUtility.SetDirty(buttonObj);
            } catch (System.Exception e) {
                Debug.LogError("Error setting SkillButtonUI fields: " + e.Message);
            }
        };
        
        // Layout Element
        LayoutElement layoutElement = buttonObj.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = SkillButtonSize.y;
        layoutElement.flexibleWidth = 1;
        
        return buttonObj;
    }
    
    private static void ApplySkillTabUIScript(GameObject panelObj, GameObject toggleButton, GameObject[] skillButtons)
    {
        // SkillTabUI scriptini ekle
        SkillTabUI skillTabUI = panelObj.AddComponent<SkillTabUI>();
        
        Button closeButton = panelObj.transform.Find("Header/CloseButton").GetComponent<Button>();
        Transform buttonContainer = panelObj.transform.Find("ScrollView/Viewport/ButtonContainer");
        
        // Script referanslarını düzenlemek için
        EditorApplication.delayCall += () => {
            try {
                // Temel referansları ayarla
                SetFieldValue(skillTabUI, "_menuPanel", panelObj);
                SetFieldValue(skillTabUI, "_toggleMenuButton", toggleButton.GetComponent<Button>());
                SetFieldValue(skillTabUI, "_closeMenuButton", closeButton);
                SetFieldValue(skillTabUI, "_skillButtonContainer", buttonContainer);
                
                // Buton prefabını ayarla - ilk butonu prefab olarak kullan
                if (skillButtons != null && skillButtons.Length > 0)
                {
                    SetFieldValue(skillTabUI, "_skillButtonPrefab", skillButtons[0].GetComponent<SkillButtonUI>());
                }
                
                // Tab tuşu yerine, kullanılmayan bir tuş atayalım veya KeyCode.None kullanarak devre dışı bırakalım
                SetFieldValue(skillTabUI, "_toggleMenuKey", KeyCode.None); // Tab tuşunu devre dışı bırak
                SetFieldValue(skillTabUI, "_activeSkillOutlineColor", ActiveSkillOutlineColor);
                SetFieldValue(skillTabUI, "_buttonSpacing", 10f);
                
                EditorUtility.SetDirty(panelObj);
            } catch (System.Exception e) {
                Debug.LogError("Error setting SkillTabUI fields: " + e.Message);
            }
        };
        
        // Butonlara TabUI referansını ver
        foreach (var button in skillButtons)
        {
            SkillButtonUI buttonUI = button.GetComponent<SkillButtonUI>();
            if (buttonUI != null)
            {
                SetFieldValue(buttonUI, "_tabUI", skillTabUI);
                EditorUtility.SetDirty(button);
            }
        }
        
        // ToggleMenuButton'un onClick olayını doğrudan panele bağla
        Button toggleBtn = toggleButton.GetComponent<Button>();
        toggleBtn.onClick.RemoveAllListeners(); // Önceki tüm dinleyicileri temizle
        toggleBtn.onClick.AddListener(() => {
            panelObj.SetActive(!panelObj.activeSelf);
        });
    }
    
    private static void UpdatePrefabs(GameObject skillTabPanel, GameObject toggleButton)
    {
        // Prefablara yapılan değişiklikleri kaydet
        string panelPrefabPath = PrefabsPath + "/Skills/SkillTabPanel.prefab";
        string togglePath = PrefabsPath + "/Skills/ToggleMenuButton.prefab";
        
        if (!Directory.Exists(PrefabsPath + "/Skills"))
            Directory.CreateDirectory(PrefabsPath + "/Skills");
            
        // Prefabları güncelle
        PrefabUtility.SaveAsPrefabAsset(skillTabPanel, panelPrefabPath);
        PrefabUtility.SaveAsPrefabAsset(toggleButton, togglePath);
    }
    
    // Reflection API yardımcı metodu - private alanları ayarlamak için
    private static void SetFieldValue(object target, string fieldName, object value)
    {
        if (target == null)
        {
            Debug.LogError($"Target object is null when trying to set field {fieldName}");
            return;
        }
        
        try
        {
            System.Type type = target.GetType();
            System.Reflection.FieldInfo fieldInfo = type.GetField(fieldName, 
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
                
            if (fieldInfo != null)
            {
                fieldInfo.SetValue(target, value);
                //Debug.Log($"Successfully set {fieldName} = {value}");
            }
            else
            {
                // Field bulunamadıysa mevcut field'ları yazdır
                string fields = "Available fields: ";
                foreach (var field in type.GetFields(System.Reflection.BindingFlags.Instance | 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public))
                {
                    fields += field.Name + ", ";
                }
                Debug.LogWarning($"Field '{fieldName}' not found in {type.Name}. {fields}");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error setting field {fieldName}: {ex.Message} - {ex.StackTrace}");
        }
    }
} 