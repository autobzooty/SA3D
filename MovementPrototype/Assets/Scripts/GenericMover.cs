using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericMover : MonoBehaviour
{
  private Vector3 TargetLookLocation;
  private float Throttle;
  public float MaxHSpeed = 2.0f;
    void Start()
    {
      TargetLookLocation = Vector3.zero;
      Throttle = 0.0f;
    }

  void Update()
  {
    Turn();
    Move();
  }

  void Turn()
  {
    if(TargetLookLocation != Vector3.zero)
    {
      Vector3 lookLocation = TargetLookLocation - transform.position;
      lookLocation.y = 0;
      transform.rotation = Quaternion.LookRotation(lookLocation, Vector3.up);
    }
  }

  void Move()
  {
    transform.position += transform.forward * MaxHSpeed * Throttle * Time.deltaTime;
    SnapToGround();
  }

  void SnapToGround()
  {
    int layerMask = LayerMask.GetMask("Default");
    Ray ray = new Ray(transform.position + transform.up, -transform.up);
    //The "2" here is kind of a magic number
    if(Physics.Raycast(ray, out RaycastHit hit, 2, layerMask, QueryTriggerInteraction.Ignore))
    {
      transform.position = hit.point;
    }
  }

  public void SetTargetLookLocation(Vector3 lookTarget)
  {
    TargetLookLocation = lookTarget;
  }

  public void SetThrottle(float throttle)
  {
    Throttle = Mathf.Clamp(throttle, 0, 1);
  }
}
