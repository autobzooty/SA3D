using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine : MonoBehaviour
{
  public class State
  {
    public string m_Name = "Base";
    public StateMachine m_Owner;

    public State(StateMachine owner, string name)
    {
      m_Name = name;
      m_Owner = owner;
      owner.m_States.Add(m_Name, this);
    }

    public virtual void Enter() { }
    public virtual void Update() { }
    public virtual string CheckConnections() { return string.Empty; }
    public virtual void Exit() { }
  }


  public State m_CurrentState;
  public string m_StartingState;

  public Dictionary<string, State> m_States =
    new Dictionary<string, State>();

  public void ChangeTo(string newState)
  {
    m_CurrentState.Exit();

    m_CurrentState = m_States[newState];

    m_CurrentState.Enter();
  }

  public virtual void Start()
  {
    print(m_StartingState);
    m_States[m_StartingState].Enter();
  }

  private void Update()
  {
    m_CurrentState.Update();

    var nextName = m_CurrentState.CheckConnections();
    if (m_States.ContainsKey(nextName))
      ChangeTo(nextName);
  }
}
