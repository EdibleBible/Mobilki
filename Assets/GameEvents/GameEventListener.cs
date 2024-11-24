using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[System.Serializable]
public class CustomGameEvent : UnityEvent<Component, object> { }
public class GameEventListener : MonoBehaviour
{
    public GameEvent gameEvent;

    public CustomGameEvent response;

    private void OnEnable()
    {
        gameEvent.RegisterListener(this);
    }
    private void OnDisable()
    {
        gameEvent.UnregisterListener(this);
    }
    public void OnEventRaised(Component sender = null, object data = null)
    {

        try
        {
            if (response == null)
                return;
            response.Invoke(sender, data);
        }
        catch (Exception e)
        {
            if (response == null)
                return;

            Debug.LogError($"Error in {this.name} GameEventListener: {e.Message} Game Event Name: {gameEvent.name}");
        }
    }

}
