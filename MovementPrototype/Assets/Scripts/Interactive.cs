using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class Interactive : MonoBehaviour
{
  public string m_InteractionText = "Read";

  [System.Serializable]
  public class Events
  {
    public InteractionEvent InteractionStarted;
    public InteractionEvent InteractionEnded;
  }

  public Events m_Events;


  public void RequestInteraction(InteractionEventData ieData)
  {
    m_Events.InteractionStarted.Invoke(ieData);
  }


  public void EndInteraction(InteractionEventData ieData)
  {
    m_Events.InteractionEnded.Invoke(ieData);

    ieData.m_Interactor.RequestEndInteraction(ieData);
  }
}


[System.Serializable]
public class InteractionEvent : UnityEvent<InteractionEventData> { }

public class InteractionEventData
{
  public Interactive m_Interactive;
  public Interactor m_Interactor;
}
