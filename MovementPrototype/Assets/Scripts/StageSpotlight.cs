using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageSpotlight : StateMachine
{
  public bool TimeToSwitch = false;
  public GameObject Spotlight;
  public class Off : State
  {
    public Off(StageSpotlight owner, string name) : base(owner, name) { }

    public override void Enter()
    {
      print("Spotlight off");
      (m_Owner as StageSpotlight).Spotlight.GetComponent<Light>().enabled = false;
    }

    public override void Exit()
    {
      (m_Owner as StageSpotlight).TimeToSwitch = false;
    }

    public override string CheckConnections()
    {
      string newState = string.Empty;
      if((m_Owner as StageSpotlight).TimeToSwitch)
      {
        newState = "On";
      }
      return newState;
    }
  }
  public class On : State
  {
    public On(StageSpotlight owner, string name) : base(owner, name) { }

    public override void Enter()
    {
      print("Spotlight on");
      (m_Owner as StageSpotlight).Spotlight.GetComponent<Light>().enabled = true;
    }

    public override void Exit()
    {
      (m_Owner as StageSpotlight).TimeToSwitch = false;
    }

    public override string CheckConnections()
    {
      string newState = string.Empty;
      if((m_Owner as StageSpotlight).TimeToSwitch)
      {
        newState = "Off";
      }
      return newState;
    }
  }

  public override void Start()
  {
    m_CurrentState = new Off(this, "Off");
    new On(this, "On");
    base.Start();
  }

  private void OnTriggerEnter(Collider other)
  {
    GameObject obj = other.gameObject;
    if(obj.name == "PlayerCollider")
    {
      TimeToSwitch = true;
    }
  }

  private void OnTriggerExit(Collider other)
  {
    GameObject obj = other.gameObject;
    if(obj.name == "PlayerCollider")
    {
      TimeToSwitch = true;
    }
  }
}
