using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// GAME EVENTS SO
/// 
/// ROLE: CREATES A MODULAR EVENT THAT TAKES IN NO ARGUMENTS
/// 
/// THINGS TO NOTE:
/// REQUIRES AND WORKS WITH THE GAME EVENT LISTENER SCRIPT
/// 
/// CONCEPTS:
/// SCRIPTABLE OBJECTS
/// OBSERVER PATTERN
/// LIST
/// 
/// REFERENCE: https://unity.com/how-to/architect-game-code-scriptable-objects
/// </summary>

[CreateAssetMenu(fileName = "New Event", menuName = "Game Event / void", order = 0)]
public class GameEventsSO : ScriptableObject
{
	// LIST OF OBJECTS LISTENING TO THE EVENT
	private List<GameEventListener> listeners =
		new List<GameEventListener>();

	// TELLS ALL THE LISTENERS THAT THE EVENT WAS TRIGGERED
	public void Raise()
	{
		// GOES THROUGH EACH OBJECT LISTENER AND EXECUTES THEIR RESPONSE TO EVENT
		for (int i = listeners.Count - 1; i >= 0; i--)
			listeners[i].OnEventRaised();
	}

	// TAKES IN AN OBJECT THAT WANTS TO DO RESPOND TO AN EVENT AND REGISTER IT
	public void RegisterListener(GameEventListener listener)
	{
		listeners.Add(listener);
	}

	// TAKES IN AN OBJECT AND REMOVES IT FROM THE LIST OF REGISTERED LISTENERS
	public void UnregisterListener(GameEventListener listener)
	{
		listeners.Remove(listener);
	}
}
