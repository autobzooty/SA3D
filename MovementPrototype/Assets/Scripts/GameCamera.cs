using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pixelplacement;

public class GameCamera : MonoBehaviour
{
  public float MinDistanceToGround = 2.0f;
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
  private bool TrackX = false;
  private bool TrackY = false;
  private bool TrackZ = false;
  private GameObject DummyCamera;

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
    if(CurrentCameraMode == CameraModes.Static)
    {
      UpdateStaticCam();
    }
  }

  void Truck()
  {
    transform.position += -transform.right * IL.GetRightStickVector().x * SpeedScalar * Time.deltaTime;
  }

  void Boom()
  {
    transform.position += transform.up * -IL.GetRightStickVector().y * SpeedScalar * Time.deltaTime;
    Ray downRay = new Ray(transform.position, Vector3.down);
    int layerMask = LayerMask.GetMask("Default");
    if(Physics.Raycast(downRay, out RaycastHit hitInfo, MinDistanceToGround, layerMask, QueryTriggerInteraction.Ignore))
    {
      transform.position = hitInfo.point + Vector3.up * MinDistanceToGround;
    }
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
      //Only snap to walls, not floors or ceilings
      Vector3 normalForwardVector = hitInfo.normal;
      normalForwardVector.y = 0;
      if(Vector3.Angle(hitInfo.normal, normalForwardVector) > 75.0f)
      {
        transform.position = hitInfo.point;
      }
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

  public void SwitchToStaticCam(Transform staticCamTransform, float switchTime, bool trackX, bool trackY, bool trackZ)
  {
    if(CurrentCameraMode == CameraModes.Static) return;
    CurrentCameraMode = CameraModes.Static;
    DummyCamera = staticCamTransform.gameObject;
    TrackX = trackX;
    TrackY = trackY;
    TrackZ = trackZ;
    Tween.Position(transform, staticCamTransform.position, switchTime, 0, Tween.EaseInOut, Tween.LoopType.None);
    Tween.Rotation(transform, staticCamTransform.rotation, switchTime, 0, Tween.EaseInOut, Tween.LoopType.None);
  }

  void UpdateStaticCam()
  {
    //Vector3 targetLocalPosition = transform.InverseTransformPoint(Target.transform.position);
    Vector3 targetLocalPosition = Target.transform.position;
    Vector3 newPosition = transform.position;
    if(TrackX)
    {
      newPosition.x = targetLocalPosition.x;
    }
    if(TrackY)
    {
      newPosition.y = targetLocalPosition.y;
    }
    if(TrackZ)
    {
      newPosition.z = targetLocalPosition.z;
    }
    transform.localPosition = newPosition;
    DummyCamera.transform.position = transform.position;
  }

  public void SwitchToOrbitCam()
  {
    CurrentCameraMode = CameraModes.Orbit;
  }
}
