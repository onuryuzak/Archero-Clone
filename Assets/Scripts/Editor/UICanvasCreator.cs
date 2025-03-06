using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

/// <summary>
/// Editor tool for creating UI canvases with specific aspect ratios and orientations
/// </summary>
public class UICanvasCreator : EditorWindow
{
    private enum CanvasOrientation
    {
        Portrait,
        Landscape
    }
    
    private CanvasOrientation orientation = CanvasOrientation.Portrait;
    private string canvasName = "GameCanvas";
    private bool createEventSystem = true;
    private bool createSafeArea = true;
    
    [MenuItem("Tools/UI/Create Canvas")]
    public static void ShowWindow()
    {
        GetWindow<UICanvasCreator>("Canvas Creator");
    }
    
    private void OnGUI()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Canvas Settings", EditorStyles.boldLabel);
        
        canvasName = EditorGUILayout.TextField("Canvas Name", canvasName);
        orientation = (CanvasOrientation)EditorGUILayout.EnumPopup("Orientation", orientation);
        createEventSystem = EditorGUILayout.Toggle("Create Event System", createEventSystem);
        createSafeArea = EditorGUILayout.Toggle("Create Safe Area", createSafeArea);
        
        EditorGUILayout.Space(10);
        
        if (GUILayout.Button("Create 16:9 Canvas"))
        {
            CreateCanvas();
        }

        if (GUILayout.Button("Create 16:9 Portrait Canvas"))
        {
            orientation = CanvasOrientation.Portrait;
            CreateCanvas();
        }
        
        EditorGUILayout.EndVertical();
    }
    
    private void CreateCanvas()
    {
        // Create Canvas
        GameObject canvasObj = new GameObject(canvasName);
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        // Add Canvas Scaler
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        
        if (orientation == CanvasOrientation.Portrait)
        {
            // 9:16 for portrait (reversed 16:9)
            scaler.referenceResolution = new Vector2(1080, 1920); // Common portrait resolution
        }
        else
        {
            // 16:9 for landscape
            scaler.referenceResolution = new Vector2(1920, 1080); // Common landscape resolution
        }
        
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = orientation == CanvasOrientation.Portrait ? 1 : 0;
        
        // Add Graphic Raycaster
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // Create Event System if needed
        if (createEventSystem && FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
        
        // Create Safe Area if enabled
        if (createSafeArea)
        {
            GameObject safeAreaObj = new GameObject("SafeArea");
            safeAreaObj.transform.SetParent(canvasObj.transform, false);
            
            RectTransform safeAreaRect = safeAreaObj.AddComponent<RectTransform>();
            safeAreaRect.anchorMin = Vector2.zero;
            safeAreaRect.anchorMax = Vector2.one;
            safeAreaRect.sizeDelta = Vector2.zero;
            
            // Add SafeArea component
            safeAreaObj.AddComponent<SafeAreaHandler>();
            
            // Create main content area inside the safe area
            GameObject contentObj = new GameObject("Content");
            contentObj.transform.SetParent(safeAreaObj.transform, false);
            
            RectTransform contentRect = contentObj.AddComponent<RectTransform>();
            contentRect.anchorMin = Vector2.zero;
            contentRect.anchorMax = Vector2.one;
            contentRect.sizeDelta = Vector2.zero;
        }
        
        // Select the canvas in the hierarchy
        Selection.activeGameObject = canvasObj;
        
        Debug.Log($"Created {(orientation == CanvasOrientation.Portrait ? "Portrait" : "Landscape")} 16:9 Canvas: {canvasName}");
    }
} 