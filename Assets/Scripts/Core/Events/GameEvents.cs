using UnityEngine;
using System;

/// <summary>
/// Central static repository for all game events using C# Actions
/// This static class allows global access to all game events from anywhere in the code
/// </summary>
public static class GameEvents
{

    // Player events
    public static Action<SkillData> OnSkillActivated; // With skill data parameter
    public static Action<SkillData> OnSkillDeactivated; // With skill data parameter

    // Enemy events
    public static Action<Enemy> OnEnemySpawned; // With enemy parameter
    public static Action<Enemy> OnEnemyDefeated; // With enemy parameter
    
    public static Action<bool> OnRageModeStateChanged;

    /// <summary>
    /// Reset all events - useful when changing scenes or restarting game
    /// </summary>
    public static void ResetAllEvents()
    {
        
        // Player events
        OnSkillActivated = null;
        OnSkillDeactivated = null;
        
        // Enemy events
        OnEnemySpawned = null;
        OnEnemyDefeated = null;
        
        Debug.Log("[GameEvents] All events have been reset");
    }
} 