//The purpose of this script is to dispatch an event when the player spins the stick in a circle.
//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(InputListener))]

public class SpinResponder : MonoBehaviour
{
  [System.Serializable]
  public class Events
  {
    public UnityEvent LeftStickSpinTriggered;
  }

  public Events m_Events;
  public float SpinWindow = 3/60;                               //Amount of time the player has to reach each checkpoint of the spin

  private float SpinStopwatch = 0.0f;
  private InputListener IL;
  private Vector2 SpinStartVector;
  private int SpinCheckpoint = 0;
  private bool SpinClockwise = true;
  private bool BegunSpin = false;
  private int NumberOfSpinCheckpoints = 4;
  private float SpinWedgeWidth;

  void Start()
  {
    IL = GetComponent<InputListener>();
    SpinWedgeWidth = 360.0f / NumberOfSpinCheckpoints;
  }

  void Update()
  {
    if(BegunSpin)
    {
      Spin();
    }
    else
    {
      AttemptBeginSpin();
    }
  }

  void Spin()
  {
    SpinStopwatch += Time.deltaTime;
    if(SpinStopwatch >= SpinWindow || IL.GetLeftStickVector().magnitude <= 0.5f)
    {
      FailSpin();
    }
    else
    {
      float absAngle = Mathf.Abs(Vector2.Angle(GetCurrentCheckpointVector(), IL.GetLeftStickVector()));
      if(absAngle >= SpinWedgeWidth && absAngle < SpinWedgeWidth * 2)
      {
        ContinueSpin();
        if(SpinCheckpoint == 0)
        {
          TriggerSpin();
        }
      }
    }
  }

  void AttemptBeginSpin()
  {
    if(IL.GetLeftStickVector() != Vector2.zero && IL.GetLeftStickVector().magnitude >= 0.5f)
    {
      BeginSpin();
    }
  }

  void BeginSpin()
  {
    SpinStartVector = IL.GetLeftStickVector();
    BegunSpin = true;
    SpinCheckpoint = 0;
    SpinStopwatch = 0;
  }

  Vector2 GetCurrentCheckpointVector()
  {
    Quaternion rotation = Quaternion.AngleAxis(SpinWedgeWidth * SpinCheckpoint, Vector3.back);
    return rotation * SpinStartVector;
  }

  void ContinueSpin()
  {
    if(SpinCheckpoint == 0)
    {
      SpinClockwise = Vector3.Cross(IL.GetLeftStickVector(), GetCurrentCheckpointVector()).z < 0;
    }
    if(SpinClockwise)
    {
      SpinCheckpoint++;
    }
    else
    {
      if(SpinCheckpoint == 0)
      {
        SpinCheckpoint = NumberOfSpinCheckpoints;
      }
      SpinCheckpoint--;
    }
    if(SpinCheckpoint > NumberOfSpinCheckpoints || SpinCheckpoint < 0)
    {
      SpinCheckpoint = 0;
    }

    SpinStopwatch = 0.0f;
  }

  void TriggerSpin()
  {
    m_Events.LeftStickSpinTriggered.Invoke();
    CancelSpin();
  }

  void FailSpin()
  {
    CancelSpin();
  }

  void CancelSpin()
  {
    BegunSpin = false;
    SpinCheckpoint = 0;
    SpinStopwatch = 0;
  }
}
