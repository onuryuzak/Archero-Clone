using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Custom editor for creating and configuring world space health bars
/// </summary>
#if UNITY_EDITOR
public class HealthBarEditor : EditorWindow
{
    // Canvas settings
    private bool _createNewCanvas = true;
    private GameObject _existingCanvas;
    private RenderMode _renderMode = RenderMode.WorldSpace;
    private float _canvasScale = 0.01f;
    
    // Target settings
    private GameObject _enemyTarget;
    private Vector3 _offset = new Vector3(0, 1.5f, 0);
    private bool _faceCamera = true;
    
    // Bar settings
    private string _healthBarName = "EnemyHealthBar";
    private Vector2 _healthBarSize = new Vector2(100, 15);
    private bool _includeBackground = true;
    private bool _includeBorder = true;
    private bool _includeText = true;
    private Color _fillColor = Color.green;
    private Color _backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
    private Color _borderColor = Color.black;
    private Color _textColor = Color.white;
    private int _borderSize = 2;
    private bool _useSmoothing = true;
    private float _smoothSpeed = 5f;
    
    // Text settings
    private bool _useTMP = true;
    private TMP_FontAsset _tmpFontAsset;
    private Font _textFont;
    private bool _showAsPercentage = false;
    
    // Visibility settings
    private bool _hideWhenFull = false;
    private bool _hideAfterDelay = false;
    private float _hideDelay = 3f;
    
    // Advanced options
    private bool _useColorTransition = true;
    private bool _pulseWhenLow = false;
    private Color _mediumHealthColor = Color.yellow;
    private Color _lowHealthColor = Color.red;
    private float _mediumThreshold = 0.6f;
    private float _lowThreshold = 0.3f;
    
    // UI Organization
    private Vector2 _scrollPosition;
    private bool _canvasOptions = true;
    private bool _targetOptions = true;
    private bool _barOptions = true;
    private bool _colorOptions = false;
    private bool _textOptions = false;
    private bool _visibilityOptions = false;
    private bool _advancedOptions = false;
    
    [MenuItem("GameObject/UI/World Space Health Bar", false, 10)]
    public static void ShowWindow()
    {
        HealthBarEditor window = GetWindow<HealthBarEditor>();
        window.titleContent = new GUIContent("Health Bar Creator");
        window.minSize = new Vector2(350, 450);
        window.Show();
    }
    
    private void OnEnable()
    {
        // Try to find TMP font asset
        _tmpFontAsset = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        
        // If not found in resources, try to find in project
        if (_tmpFontAsset == null)
        {
            string[] guids = AssetDatabase.FindAssets("t:TMP_FontAsset");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                _tmpFontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            }
        }
        
        // Try to find default font
        _textFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
        
        // Try to find main camera
        if (Camera.main != null)
        {
            // Default offset to be in front of camera
            _offset = new Vector3(0, 1.5f, 0);
        }
    }
    
    private void OnGUI()
    {
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("World Space Health Bar Creator", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        EditorGUILayout.HelpBox("Create a health bar that floats above enemies in the game world.", MessageType.Info);
        EditorGUILayout.Space(10);
        
        // Canvas Settings
        _canvasOptions = EditorGUILayout.Foldout(_canvasOptions, "Canvas Settings", true);
        if (_canvasOptions)
        {
            EditorGUI.indentLevel++;
            _createNewCanvas = EditorGUILayout.Toggle("Create New Canvas", _createNewCanvas);
            
            if (!_createNewCanvas)
            {
                _existingCanvas = EditorGUILayout.ObjectField("Existing Canvas", _existingCanvas, typeof(GameObject), true) as GameObject;
            }
            else
            {
                _renderMode = (RenderMode)EditorGUILayout.EnumPopup("Render Mode", _renderMode);
                if (_renderMode == RenderMode.WorldSpace)
                {
                    _canvasScale = EditorGUILayout.FloatField("Canvas Scale", _canvasScale);
                }
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(5);
        }
        
        // Target Settings
        _targetOptions = EditorGUILayout.Foldout(_targetOptions, "Target Settings", true);
        if (_targetOptions)
        {
            EditorGUI.indentLevel++;
            _enemyTarget = EditorGUILayout.ObjectField("Enemy Target", _enemyTarget, typeof(GameObject), true) as GameObject;
            _offset = EditorGUILayout.Vector3Field("Offset From Target", _offset);
            _faceCamera = EditorGUILayout.Toggle("Face Camera", _faceCamera);
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(5);
        }
        
        // Bar Settings
        _barOptions = EditorGUILayout.Foldout(_barOptions, "Bar Settings", true);
        if (_barOptions)
        {
            EditorGUI.indentLevel++;
            _healthBarName = EditorGUILayout.TextField("Health Bar Name", _healthBarName);
            _healthBarSize = EditorGUILayout.Vector2Field("Size (Width, Height)", _healthBarSize);
            _includeBackground = EditorGUILayout.Toggle("Include Background", _includeBackground);
            _includeBorder = EditorGUILayout.Toggle("Include Border", _includeBorder);
            _includeText = EditorGUILayout.Toggle("Include Text", _includeText);
            _useSmoothing = EditorGUILayout.Toggle("Use Smooth Fill", _useSmoothing);
            if (_useSmoothing)
                _smoothSpeed = EditorGUILayout.Slider("Smooth Speed", _smoothSpeed, 1f, 10f);
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(5);
        }
        
        // Color settings
        _colorOptions = EditorGUILayout.Foldout(_colorOptions, "Color Settings", true);
        if (_colorOptions)
        {
            EditorGUI.indentLevel++;
            _fillColor = EditorGUILayout.ColorField("Fill Color", _fillColor);
            if (_includeBackground)
                _backgroundColor = EditorGUILayout.ColorField("Background Color", _backgroundColor);
            if (_includeBorder)
            {
                _borderColor = EditorGUILayout.ColorField("Border Color", _borderColor);
                _borderSize = EditorGUILayout.IntSlider("Border Size", _borderSize, 1, 5);
            }
            if (_includeText)
                _textColor = EditorGUILayout.ColorField("Text Color", _textColor);
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(5);
        }
        
        // Text options
        if (_includeText)
        {
            _textOptions = EditorGUILayout.Foldout(_textOptions, "Text Settings", true);
            if (_textOptions)
            {
                EditorGUI.indentLevel++;
                _useTMP = EditorGUILayout.Toggle("Use TextMeshPro", _useTMP);
                if (_useTMP)
                    _tmpFontAsset = EditorGUILayout.ObjectField("TMP Font Asset", _tmpFontAsset, typeof(TMP_FontAsset), false) as TMP_FontAsset;
                else
                    _textFont = EditorGUILayout.ObjectField("Font", _textFont, typeof(Font), false) as Font;
                
                _showAsPercentage = EditorGUILayout.Toggle("Show as Percentage", _showAsPercentage);
                EditorGUI.indentLevel--;
                EditorGUILayout.Space(5);
            }
        }
        
        // Visibility options
        _visibilityOptions = EditorGUILayout.Foldout(_visibilityOptions, "Visibility Settings", true);
        if (_visibilityOptions)
        {
            EditorGUI.indentLevel++;
            _hideWhenFull = EditorGUILayout.Toggle("Hide When Full Health", _hideWhenFull);
            _hideAfterDelay = EditorGUILayout.Toggle("Hide After Delay", _hideAfterDelay);
            
            if (_hideAfterDelay)
                _hideDelay = EditorGUILayout.FloatField("Hide Delay (seconds)", _hideDelay);
            
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(5);
        }
        
        // Advanced options
        _advancedOptions = EditorGUILayout.Foldout(_advancedOptions, "Advanced Settings", true);
        if (_advancedOptions)
        {
            EditorGUI.indentLevel++;
            _useColorTransition = EditorGUILayout.Toggle("Color Transition", _useColorTransition);
            if (_useColorTransition)
            {
                _mediumHealthColor = EditorGUILayout.ColorField("Medium Health Color", _mediumHealthColor);
                _lowHealthColor = EditorGUILayout.ColorField("Low Health Color", _lowHealthColor);
                _mediumThreshold = EditorGUILayout.Slider("Medium Threshold", _mediumThreshold, 0.3f, 0.8f);
                _lowThreshold = EditorGUILayout.Slider("Low Threshold", _lowThreshold, 0.1f, 0.5f);
            }
            
            _pulseWhenLow = EditorGUILayout.Toggle("Pulse When Low", _pulseWhenLow);
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(5);
        }
        
        EditorGUILayout.Space(10);
        
        if (GUILayout.Button("Create Health Bar", GUILayout.Height(30)))
        {
            CreateHealthBar();
        }
        
        EditorGUILayout.Space(10);
        EditorGUILayout.EndScrollView();
    }
    
    private void CreateHealthBar()
    {
        // Get or create the canvas
        GameObject canvasObj;
        Canvas canvas;
        
        if (_createNewCanvas)
        {
            // Create a new canvas
            canvasObj = new GameObject("WorldSpaceCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = _renderMode;
            
            if (_renderMode == RenderMode.WorldSpace)
            {
                // Set canvas size and scale for world space
                RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();
                canvasRect.sizeDelta = new Vector2(5, 5); // Default size in world units
                canvasRect.localScale = new Vector3(_canvasScale, _canvasScale, _canvasScale);
                
                // Position the canvas near the target if available
                if (_enemyTarget != null)
                {
                    canvasObj.transform.position = _enemyTarget.transform.position + _offset;
                }
            }
            
            // Add required components
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
            
            // Add canvas group for alpha control
            CanvasGroup canvasGroup = canvasObj.AddComponent<CanvasGroup>();
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            
            Debug.Log("Created new Canvas GameObject with render mode: " + _renderMode);
        }
        else
        {
            if (_existingCanvas == null)
            {
                EditorUtility.DisplayDialog("Error", "Please select an existing Canvas or create a new one.", "OK");
                return;
            }
            
            canvas = _existingCanvas.GetComponent<Canvas>();
            if (canvas == null)
            {
                EditorUtility.DisplayDialog("Invalid Canvas", "The selected GameObject is not a Canvas.", "OK");
                return;
            }
            
            canvasObj = _existingCanvas;
        }
        
        // Create the health bar parent
        GameObject healthBarObj = new GameObject(_healthBarName);
        RectTransform healthBarRect = healthBarObj.AddComponent<RectTransform>();
        healthBarObj.transform.SetParent(canvasObj.transform, false);
        healthBarRect.sizeDelta = _healthBarSize;
        healthBarRect.anchoredPosition = Vector2.zero;
        
        // Create background if enabled
        GameObject backgroundObj = null;
        if (_includeBackground)
        {
            backgroundObj = CreateUIImage("Background", healthBarObj.transform, _backgroundColor);
            RectTransform backgroundRect = backgroundObj.GetComponent<RectTransform>();
            backgroundRect.anchorMin = Vector2.zero;
            backgroundRect.anchorMax = Vector2.one;
            backgroundRect.sizeDelta = Vector2.zero;
        }
        
        // Create border if enabled
        GameObject borderObj = null;
        if (_includeBorder)
        {
            borderObj = CreateUIImage("Border", healthBarObj.transform, _borderColor);
            RectTransform borderRect = borderObj.GetComponent<RectTransform>();
            borderRect.anchorMin = Vector2.zero;
            borderRect.anchorMax = Vector2.one;
            borderRect.sizeDelta = Vector2.zero;
            
            // Create a mask for the border (using a different approach)
            GameObject maskObj = CreateUIImage("BorderMask", borderObj.transform, Color.white);
            RectTransform maskRect = maskObj.GetComponent<RectTransform>();
            maskRect.anchorMin = Vector2.zero;
            maskRect.anchorMax = Vector2.one;
            maskRect.sizeDelta = new Vector2(-_borderSize * 2, -_borderSize * 2);
            maskRect.anchoredPosition = Vector2.zero;
            
            // Add a mask component
            Mask mask = maskObj.AddComponent<Mask>();
            mask.showMaskGraphic = false;
            
            // Move any existing children to the mask
            if (backgroundObj != null)
                backgroundObj.transform.SetParent(maskObj.transform, true);
        }
        
        // Get the correct parent for the fill (either the border mask or the health bar)
        Transform fillParent = healthBarObj.transform;
        if (_includeBorder)
            fillParent = healthBarObj.transform.Find("Border/BorderMask");
        else if (_includeBackground)
            fillParent = backgroundObj.transform;
        
        // Create the fill bar
        GameObject fillObj = CreateUIImage("Fill", fillParent, _fillColor);
        RectTransform fillRect = fillObj.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;
        
        // Set the fill method
        Image fillImage = fillObj.GetComponent<Image>();
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
        fillImage.fillAmount = 1f;
        
        // Create text if enabled
        GameObject textObj = null;
        if (_includeText)
        {
            if (_useTMP && _tmpFontAsset != null)
            {
                textObj = new GameObject("Text");
                textObj.transform.SetParent(healthBarObj.transform, false);
                
                RectTransform textRect = textObj.AddComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.sizeDelta = Vector2.zero;
                textRect.anchoredPosition = Vector2.zero;
                
                TextMeshProUGUI textComponent = textObj.AddComponent<TextMeshProUGUI>();
                textComponent.font = _tmpFontAsset;
                textComponent.color = _textColor;
                textComponent.alignment = TextAlignmentOptions.Center;
                textComponent.text = _showAsPercentage ? "100%" : "100/100";
                textComponent.fontSize = _healthBarSize.y * 0.6f;
            }
            else
            {
                textObj = new GameObject("Text");
                textObj.transform.SetParent(healthBarObj.transform, false);
                
                RectTransform textRect = textObj.AddComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.sizeDelta = Vector2.zero;
                textRect.anchoredPosition = Vector2.zero;
                
                Text textComponent = textObj.AddComponent<Text>();
                textComponent.font = _textFont;
                textComponent.color = _textColor;
                textComponent.alignment = TextAnchor.MiddleCenter;
                textComponent.text = _showAsPercentage ? "100%" : "100/100";
                textComponent.fontSize = Mathf.RoundToInt(_healthBarSize.y * 0.6f);
            }
        }
        
        // Add the HealthBar component
        HealthBar healthBarComponent = healthBarObj.AddComponent<HealthBar>();
        
        // Configure the component via SerializedObject for proper Undo support
        SerializedObject serializedHealthBar = new SerializedObject(healthBarComponent);
        
        // Set references
        if (_enemyTarget != null)
            serializedHealthBar.FindProperty("_target").objectReferenceValue = _enemyTarget.transform;
            
        serializedHealthBar.FindProperty("_offset").vector3Value = _offset;
        serializedHealthBar.FindProperty("_faceCamera").boolValue = _faceCamera;
        serializedHealthBar.FindProperty("_width").floatValue = _healthBarSize.x * _canvasScale;
        serializedHealthBar.FindProperty("_height").floatValue = _healthBarSize.y * _canvasScale;
        
        serializedHealthBar.FindProperty("_fillImage").objectReferenceValue = fillImage;
        if (_includeBackground)
            serializedHealthBar.FindProperty("_backgroundImage").objectReferenceValue = backgroundObj.GetComponent<Image>();
        if (_includeBorder)
            serializedHealthBar.FindProperty("_borderImage").objectReferenceValue = borderObj.GetComponent<Image>();
        
        if (_includeText)
        {
            if (_useTMP)
                serializedHealthBar.FindProperty("_healthText").objectReferenceValue = textObj.GetComponent<TextMeshProUGUI>();
            else
                serializedHealthBar.FindProperty("_healthText").objectReferenceValue = textObj.GetComponent<Text>();
                
            serializedHealthBar.FindProperty("_showHealthText").boolValue = true;
            serializedHealthBar.FindProperty("_showAsPercentage").boolValue = _showAsPercentage;
        }
        else
        {
            serializedHealthBar.FindProperty("_showHealthText").boolValue = false;
        }
        
        // Set settings
        serializedHealthBar.FindProperty("_useSmoothFill").boolValue = _useSmoothing;
        serializedHealthBar.FindProperty("_smoothFillSpeed").floatValue = _smoothSpeed;
        
        // Visibility settings
        serializedHealthBar.FindProperty("_hideWhenFull").boolValue = _hideWhenFull;
        serializedHealthBar.FindProperty("_hideAfterDelay").boolValue = _hideAfterDelay;
        serializedHealthBar.FindProperty("_hideDelay").floatValue = _hideDelay;
        
        // Animation settings
        serializedHealthBar.FindProperty("_useColorTransition").boolValue = _useColorTransition;
        serializedHealthBar.FindProperty("_highHealthColor").colorValue = _fillColor;
        serializedHealthBar.FindProperty("_mediumHealthColor").colorValue = _mediumHealthColor;
        serializedHealthBar.FindProperty("_lowHealthColor").colorValue = _lowHealthColor;
        serializedHealthBar.FindProperty("_lowHealthThreshold").floatValue = _lowThreshold;
        serializedHealthBar.FindProperty("_mediumHealthThreshold").floatValue = _mediumThreshold;
        serializedHealthBar.FindProperty("_pulseWhenLow").boolValue = _pulseWhenLow;
        
        // Apply all the serialized property changes
        serializedHealthBar.ApplyModifiedProperties();
        
        // Select the created object
        Selection.activeGameObject = healthBarObj;
        
        Debug.Log("World Space Health Bar created: " + healthBarObj.name);
    }
    
    private GameObject CreateUIImage(string name, Transform parent, Color color)
    {
        GameObject imageObj = new GameObject(name);
        imageObj.transform.SetParent(parent, false);
        
        RectTransform rectTransform = imageObj.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
        
        Image image = imageObj.AddComponent<Image>();
        image.color = color;
        
        return imageObj;
    }
}
#endif