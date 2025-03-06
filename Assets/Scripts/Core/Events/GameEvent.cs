using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject-based event system implementing the Observer Pattern
/// </summary>
[CreateAssetMenu(menuName = "Events/Game Event")]
public class GameEvent : ScriptableObject
{
    // List of listeners that will respond to this event
    private List<GameEventListener> listeners = new List<GameEventListener>();
    
    public void Raise()
    {
        // Iterate through listeners from the end to the beginning in case listeners get removed during execution
        for (int i = listeners.Count - 1; i >= 0; i--)
        {
            listeners[i].OnEventRaised();
        }
    }
    
    public void RegisterListener(GameEventListener listener)
    {
        if (!listeners.Contains(listener))
        {
            listeners.Add(listener);
        }
    }
    
    public void UnregisterListener(GameEventListener listener)
    {
        if (listeners.Contains(listener))
        {
            listeners.Remove(listener);
        }
    }
} 