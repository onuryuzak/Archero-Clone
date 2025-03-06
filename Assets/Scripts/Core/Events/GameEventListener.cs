using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Component that listens for a GameEvent and responds with UnityEvents
/// Part of the Observer Pattern implementation
/// </summary>
public class GameEventListener : MonoBehaviour
{
    [Tooltip("Event to register with")]
    [SerializeField] private GameEvent gameEvent;
    
    [Tooltip("Response to invoke when Event is raised")]
    [SerializeField] private UnityEvent response;
    
    private void OnEnable()
    {
        if (gameEvent != null)
        {
            gameEvent.RegisterListener(this);
        }
    }
    
    private void OnDisable()
    {
        if (gameEvent != null)
        {
            gameEvent.UnregisterListener(this);
        }
    }
    
    public void OnEventRaised()
    {
        response?.Invoke();
    }
} 