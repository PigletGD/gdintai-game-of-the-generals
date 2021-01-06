using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// GAME EVENT LISTENER
/// 
/// ROLE: ALLOWS AN OBJECT TO DO SOMETHING WHEN AN EVENT IS TRIGGERED
/// 
/// THINGS TO NOTE:
/// REQUIRES A GAME EVENT TO LISTEN INTO
/// 
/// CONCEPTS:
/// SCRIPTABLE OBJECTS
/// OBSERVER PATTERN
/// LIST
/// 
/// REFERENCE: https://unity.com/how-to/architect-game-code-scriptable-objects
/// </summary>

public class GameEventListener : MonoBehaviour
{
    public GameEventsSO Event;
    public UnityEvent Response;

    // OBJECT GETS TO LISTEN IN ON EVENT IF ENABLED
    private void OnEnable()
    {
        Event.RegisterListener(this);
    }

    // OBJECT STOPS LISTENING TO EVENT IF DISABLED
    private void OnDisable()
    {
        Event.UnregisterListener(this);
    }

    // EXECUTES AN ASSIGNED RESPONSE FUNCTION
    public void OnEventRaised()
    {
        Response.Invoke();
    }
}