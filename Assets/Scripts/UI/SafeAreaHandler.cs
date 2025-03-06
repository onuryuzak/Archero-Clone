using UnityEngine;

/// <summary>
/// Automatically adjusts the RectTransform to the device's safe area.
/// This handles notches, curved edges, and other screen elements on different devices
/// </summary>
public class SafeAreaHandler : MonoBehaviour
{
    private RectTransform _rectTransform;
    private Rect _safeArea;
    private Vector2 _minAnchor;
    private Vector2 _maxAnchor;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        
        // Initial setup
        ApplySafeArea();
    }

    private void Update()
    {
        // Check if safe area has changed (e.g., orientation change)
        if (_safeArea != Screen.safeArea)
        {
            ApplySafeArea();
        }
    }

    private void ApplySafeArea()
    {
        // Store the current safe area
        _safeArea = Screen.safeArea;
        
        // Get reference to canvas
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("SafeAreaHandler: No Canvas found in parent hierarchy!");
            return;
        }
        
        // Calculate safe area anchors relative to the canvas
        Rect pixelRect = canvas.pixelRect;
        
        // Convert safe area to anchors (normalized position within the canvas)
        _minAnchor.x = _safeArea.x / pixelRect.width;
        _minAnchor.y = _safeArea.y / pixelRect.height;
        _maxAnchor.x = (_safeArea.x + _safeArea.width) / pixelRect.width;
        _maxAnchor.y = (_safeArea.y + _safeArea.height) / pixelRect.height;
        
        // Apply the anchors
        _rectTransform.anchorMin = _minAnchor;
        _rectTransform.anchorMax = _maxAnchor;
        
        // Reset offsets
        _rectTransform.offsetMin = Vector2.zero;
        _rectTransform.offsetMax = Vector2.zero;
        
        Debug.Log($"Applied Safe Area: {_safeArea} to {gameObject.name}");
    }
} 