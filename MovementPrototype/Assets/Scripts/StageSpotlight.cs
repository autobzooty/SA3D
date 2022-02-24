using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageSpotlight : StateMachine
{
  public bool TimeToSwitch = false;
  public GameObject Spotlight;
  public class Off : State
  {
    public Off(StateMachine owner, string name) : base(owner, name)
    {
      m_Name = "Off";
    }

    public override void Enter()
    {
      print("Spotlight off");
      //This is maybe sloppy because I'm not null checking StageSpotlight but idk if it's necessary
      m_Owner.GetComponent<StageSpotlight>().Spotlight.GetComponent<Light>().enabled = false;
    }

    public override string CheckConnections()
    {
      string newState = string.Empty;
      if(m_Owner.GetComponent<StageSpotlight>().TimeToSwitch)
      {
        newState = "On";
        m_Owner.GetComponent<StageSpotlight>().TimeToSwitch = false;
      }
      return newState;
    }
  }
  public class On : State
  {
    public On(StateMachine owner, string name) : base(owner, name)
    {
      m_Name = "On";
    }

    public override void Enter()
    {
      print("Spotlight on");
      m_Owner.GetComponent<StageSpotlight>().Spotlight.GetComponent<Light>().enabled = true;
    }

    public override string CheckConnections()
    {
      string newState = string.Empty;
      if(m_Owner.GetComponent<StageSpotlight>().TimeToSwitch)
      {
        newState = "Off";
        m_Owner.GetComponent<StageSpotlight>().TimeToSwitch = false;
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
      ChangeTo("On");
    }
  }

  private void OnTriggerExit(Collider other)
  {
    GameObject obj = other.gameObject;
    if(obj.name == "PlayerCollider")
    {
      ChangeTo("Off");
    }
  }
}
