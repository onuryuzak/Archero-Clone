using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Burn status effect applied to enemies
/// </summary>
public class BurnEffect : MonoBehaviour
{
    [SerializeField] private GameObject _burnVfxPrefab;
    
    private Enemy _targetEnemy;
    private List<BurnStack> _activeStacks = new List<BurnStack>();
    private int _maxStacks = 3;
    
    private GameObject _activeVfx;
    
    private void Awake()
    {
        _targetEnemy = GetComponent<Enemy>();
    }
    
    private void Update()
    {
        // Process all active burn effects
        for (int i = _activeStacks.Count - 1; i >= 0; i--)
        {
            BurnStack stack = _activeStacks[i];
            
            // Decrease remaining duration
            stack.remainingDuration -= Time.deltaTime;
            
            // Check if it's time to apply damage
            stack.timeSinceLastTick += Time.deltaTime;
            if (stack.timeSinceLastTick >= 1f) // Apply damage every second
            {
                ApplyBurnDamage(stack.damagePerSecond);
                stack.timeSinceLastTick = 0f;
            }
            
            // Remove stack if duration has ended
            if (stack.remainingDuration <= 0f)
            {
                _activeStacks.RemoveAt(i);
            }
        }
        
        // Remove VFX if no active stacks remain
        if (_activeStacks.Count == 0 && _activeVfx != null)
        {
            Destroy(_activeVfx);
            _activeVfx = null;
        }
    }
    
    /// <summary>
    /// Adds a new burn effect stack
    /// </summary>
    public void AddBurnStack(float duration, float damagePerSecond)
    {
        // Check stack limit
        if (_activeStacks.Count >= _maxStacks)
        {
            // Find the stack with shortest remaining duration
            int shortestIndex = 0;
            float shortestTime = float.MaxValue;
            
            for (int i = 0; i < _activeStacks.Count; i++)
            {
                if (_activeStacks[i].remainingDuration < shortestTime)
                {
                    shortestTime = _activeStacks[i].remainingDuration;
                    shortestIndex = i;
                }
            }
            
            // Replace the shortest duration stack with the new one
            _activeStacks[shortestIndex] = new BurnStack
            {
                remainingDuration = duration,
                damagePerSecond = damagePerSecond,
                timeSinceLastTick = 0f
            };
            
            Debug.Log($"[BurnEffect] Updated existing burn stack on {_targetEnemy.name}: {damagePerSecond} dps for {duration} seconds");
        }
        else
        {
            // Add new stack
            _activeStacks.Add(new BurnStack
            {
                remainingDuration = duration,
                damagePerSecond = damagePerSecond,
                timeSinceLastTick = 0f
            });
            
            Debug.Log($"[BurnEffect] Added new burn stack to {_targetEnemy.name}: {damagePerSecond} dps for {duration} seconds. Total stacks: {_activeStacks.Count}/{_maxStacks}");
        }
        
        // Create VFX if it doesn't exist
        if (_activeVfx == null && _burnVfxPrefab != null)
        {
            _activeVfx = Instantiate(_burnVfxPrefab, transform);
        }
        // If no VFX prefab is assigned, create a simple effect
        else if (_activeVfx == null)
        {
            CreateSimpleBurnEffect();
        }
    }
    
    /// <summary>
    /// Creates a simple burn effect when no VFX prefab is available
    /// </summary>
    private void CreateSimpleBurnEffect()
    {
        // Create a simple particle system for the burn effect
        _activeVfx = new GameObject("BurnEffect");
        _activeVfx.transform.SetParent(transform);
        _activeVfx.transform.localPosition = Vector3.up * 0.5f; // Position slightly above
        
        // Add a particle system
        ParticleSystem ps = _activeVfx.AddComponent<ParticleSystem>();
        
        // Configure the particle system for a fire effect
        var main = ps.main;
        main.startLifetime = 0.6f;
        main.startSpeed = 1f;
        main.startSize = 0.3f;
        main.startColor = new Color(1f, 0.5f, 0f, 0.8f); // Orange-ish color
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        
        var emission = ps.emission;
        emission.rateOverTime = 15f;
        
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.5f;
        
        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(new Color(1f, 0.5f, 0f), 0.0f),
                new GradientColorKey(new Color(1f, 0f, 0f), 1.0f) 
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(0.8f, 0.0f),
                new GradientAlphaKey(0f, 1.0f) 
            }
        );
        colorOverLifetime.color = gradient;
        
        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve curve = new AnimationCurve();
        curve.AddKey(0.0f, 0.5f);
        curve.AddKey(1.0f, 0.0f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, curve);
        
        // Add a light component for extra effect
        Light light = _activeVfx.AddComponent<Light>();
        light.color = new Color(1f, 0.6f, 0f);
        light.intensity = 1.5f;
        light.range = 3f;
        light.type = LightType.Point;
        
        // Make the light flicker
        StartCoroutine(FlickerLight(light));
    }
    
    /// <summary>
    /// Coroutine to make the light flicker
    /// </summary>
    private IEnumerator FlickerLight(Light light)
    {
        float baseIntensity = light.intensity;
        
        while (light != null && _activeStacks.Count > 0)
        {
            // Random flicker
            float noise = Random.Range(0.8f, 1.2f);
            light.intensity = baseIntensity * noise;
            
            yield return new WaitForSeconds(0.05f);
        }
    }
    
    /// <summary>
    /// Applies burn damage
    /// </summary>
    private void ApplyBurnDamage(float damage)
    {
        if (_targetEnemy != null)
        {
            _targetEnemy.TakeDamage(damage);
        }
    }
    
    /// <summary>
    /// Sets the maximum number of stacks
    /// </summary>
    public void SetMaxStacks(int max)
    {
        _maxStacks = Mathf.Max(1, max);
    }
    
    /// <summary>
    /// Burn stack data
    /// </summary>
    private class BurnStack
    {
        public float remainingDuration;
        public float damagePerSecond;
        public float timeSinceLastTick;
    }
    
    private void OnDestroy()
    {
        // Clean up any effects when destroyed
        if (_activeVfx != null)
        {
            Destroy(_activeVfx);
        }
    }
} 