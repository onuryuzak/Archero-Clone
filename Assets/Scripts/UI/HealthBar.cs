using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Image _healthFillImage;
    [SerializeField] private Canvas _canvas;
    
    private Camera mainCamera;
    
    private void Awake()
    {
        mainCamera = Camera.main;
        
        if (_canvas != null)
        {
            _canvas.worldCamera = mainCamera;
        }
    }
    
    private void LateUpdate()
    {
        // Make the health bar face the camera (billboard)
        if (mainCamera != null)
        {
            transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                mainCamera.transform.rotation * Vector3.up);
        }
    }
    
    public void Initialize(float maxHealth)
    {
        if (_healthFillImage != null)
        {
            _healthFillImage.fillAmount = 1.0f;
        }
    }
    
    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if (_healthFillImage != null)
        {
            _healthFillImage.fillAmount = Mathf.Clamp01(currentHealth / maxHealth);
        }
    }
} 