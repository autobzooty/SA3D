using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GenericMover))]
public class TrashFarmerAI : MonoBehaviour
{
  enum AIStates
  {
    Idle,
    LookAtPlayer,
    MoveToTrash,
    MoveToDumpster
  }

  public Vector3 HoldingPosition = new Vector3(0, 0, 0.75f);
  public float GrabDistance = 0.75f;

  private AIStates CurrentAIState;
  private GameObject CurrentTrashTarget;
  private GameObject Player;
  private GameObject Dumpster;
  private GenericMover Mover;
  private GameObject[] TrashSpawners;
  private bool HandsFull = false;

  void Start()
  {
    CurrentAIState = AIStates.Idle;
    Player = GameObject.Find("Player");
    Dumpster = GameObject.Find("Dumpster");
    Mover = gameObject.GetComponent<GenericMover>();
    TrashSpawners = GameObject.FindGameObjectsWithTag("TrashCropSpawner");
  }

  void Update()
  {
    UpdateAIState();
    if(CurrentAIState == AIStates.Idle)
    {
      Mover.SetThrottle(0);
    }
    else if (CurrentAIState == AIStates.LookAtPlayer)
    {
      Mover.SetTargetLookLocation(Player.transform.position);
      Mover.SetThrottle(0);
    }
    else if(CurrentAIState == AIStates.MoveToTrash)
    {
      Mover.SetTargetLookLocation(CurrentTrashTarget.GetComponent<TrashCropSpawner>().GetTrashReference().transform.position);
      Mover.SetThrottle(1);
      if(GetDistanceToTrash() <= GrabDistance)
      {
        HandsFull = true;
      }
    }
    else if(CurrentAIState == AIStates.MoveToDumpster)
    {
      Mover.SetTargetLookLocation(Dumpster.transform.position);
      Mover.SetThrottle(0.5f);

      
      CurrentTrashTarget.GetComponent<TrashCropSpawner>().GetTrashReference().transform.position = transform.TransformPoint(HoldingPosition);

      if(GetDistanceToDumpster() <= GrabDistance)
      {
        HandsFull = false;
        CurrentTrashTarget.GetComponent<TrashCropSpawner>().DestroyTrash();
      }
    }
  }

  void UpdateAIState()
  {
    if(GetDistanceToPlayer() < 3.0f)
    {
      CurrentAIState = AIStates.LookAtPlayer;
    }
    else
    {
      if(HandsFull)
      {
        CurrentAIState = AIStates.MoveToDumpster;
      }
      else
      {
        //Check if any trash spawners have spawned trash
        bool trashReady = false;
        for(int i = 0; i < TrashSpawners.Length; ++i)
        {
          if(TrashSpawners[i].GetComponent<TrashCropSpawner>().GetTrashReference())
          {
            CurrentTrashTarget = TrashSpawners[i];
            trashReady = true;
            CurrentAIState = AIStates.MoveToTrash;
          }
        }
        if(!trashReady)
        {
          CurrentAIState = AIStates.Idle;
        }
      }
    }
  }

  float GetDistanceToPlayer()
  {
    return (Player.transform.position - transform.position).magnitude;
  }
  
  float GetDistanceToTrash()
  {
    return (CurrentTrashTarget.GetComponent<TrashCropSpawner>().GetTrashReference().transform.position - transform.position).magnitude;
  }

  float GetDistanceToDumpster()
  {
    return (Dumpster.transform.position - transform.position).magnitude;
  }
}
