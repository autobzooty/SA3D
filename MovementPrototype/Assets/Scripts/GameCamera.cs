using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCamera : MonoBehaviour
{
  public GameObject Target;
  public float SpeedScalar = 1.0f;
  public float TargetDistance = 5.0f;

  private InputListener IL;
  void Start()
  {
    IL = GetComponent<InputListener>();
    if(!Target)
    {
      Target = GameObject.Find("Player");
    }
  }

  void Update()
  {
    Truck();
    Boom();
    LookAtTarget();
    MoveToTargetDistance();
    SnapToSurface();
  }

  void Truck()
  {
    transform.position += -transform.right * IL.GetRightStickVector().x * SpeedScalar * Time.deltaTime;
  }

  void Boom()
  {
    transform.position += transform.up * -IL.GetRightStickVector().y * SpeedScalar * Time.deltaTime;
  }

  void LookAtTarget()
  {
    Vector3 differenceVector = Target.transform.position - transform.position;
    transform.rotation = Quaternion.LookRotation(differenceVector, Vector3.up);
  }

  void MoveToTargetDistance()
  {
    Ray snapRay = new Ray(Target.transform.position, transform.position - Target.transform.position);
    int layerMask = LayerMask.GetMask("Default");
    if(Physics.Raycast(snapRay, out RaycastHit hitInfo, TargetDistance, layerMask, QueryTriggerInteraction.Ignore))
    {
      transform.position = hitInfo.point;
    }
    else
    {
      Vector3 differenceVector = transform.position - Target.transform.position;
      transform.position = Target.transform.position + differenceVector.normalized * TargetDistance; 
    }
  }

  void SnapToSurface()
  {

  }
}
