using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine : MonoBehaviour
{
  public class State
  {
    public string m_Name = "Base";

    public State(StateMachine owner)
    {
      owner.m_States.Add(m_Name, this);
    }

    public virtual void Enter() { }
    public virtual void Update() { }
    public virtual string CheckConnections() { return string.Empty; }
    public virtual void Exit() { }
  }


  public State m_CurrentState;
  public State m_StartingState;

  public Dictionary<string, State> m_States =
    new Dictionary<string, State>();


  private void ChangeTo(State newState)
  {
    m_CurrentState.Exit();

    m_CurrentState = newState;

    newState.Enter();
  }


  private void Update()
  {
    m_CurrentState.Update();

    var nextName = m_CurrentState.CheckConnections();
    if (m_States.ContainsKey(nextName))
      ChangeTo(m_States[nextName]);
  }
}
