using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pixelplacement;

public class GameCamera : MonoBehaviour
{
  public enum CameraModes
  {
    Orbit,
    Static,
  }
  public GameObject Target;
  public float SpeedScalar = 1.0f;
  public float TargetDistance = 5.0f;

  private InputListener IL;

  private CameraModes CurrentCameraMode;
  void Start()
  {
    CurrentCameraMode = CameraModes.Orbit;
    IL = GetComponent<InputListener>();
    if(!Target)
    {
      Target = GameObject.Find("Player");
    }
  }

  void Update()
  {
    if(CurrentCameraMode == CameraModes.Orbit)
    {
      Truck();
      Boom();
      LookAtTarget();
      MoveToTargetDistance();
      SnapToSurface();
    }
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

  public void SwitchToStaticCam(Transform staticCamTransform, float switchTime)
  {
    CurrentCameraMode = CameraModes.Static;
    Tween.Position(transform, staticCamTransform.position, switchTime, 0, Tween.EaseInOut, Tween.LoopType.None);
    Tween.Rotation(transform, staticCamTransform.rotation, switchTime, 0, Tween.EaseInOut, Tween.LoopType.None);
  }

  public void SwitchToOrbitCam()
  {
    CurrentCameraMode = CameraModes.Orbit;
  }
}
