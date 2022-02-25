using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageSpotlight : StateMachine
{
  public bool TimeToSwitch = false;
  public GameObject Spotlight;
  public GameObject SpotlightMesh;
  [HideInInspector]
  public GameObject Player;
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
      if((m_Owner as StageSpotlight).TimeToSwitch)
      {
        return "On";
      }
      return string.Empty;
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

    public override void Update()
    {
      Vector3 playerLookVector = (m_Owner as StageSpotlight).Player.transform.position - (m_Owner as StageSpotlight).SpotlightMesh.transform.position;
      Quaternion newRotation = Quaternion.LookRotation(playerLookVector, Vector3.up);
      (m_Owner as StageSpotlight).SpotlightMesh.transform.rotation = newRotation;
    }

    public override void Exit()
    {
      (m_Owner as StageSpotlight).TimeToSwitch = false;
    }

    public override string CheckConnections()
    {
      if((m_Owner as StageSpotlight).TimeToSwitch)
      {
        return "Off";
      }
      return string.Empty;
    }
  }

  public override void Start()
  {
    Player = GameObject.Find("Player");
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
