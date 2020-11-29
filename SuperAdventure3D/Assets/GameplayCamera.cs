using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameplayCamera : MonoBehaviour
{
  private Gamepad Gamepad1;

  public float Speed = 20.0f;
  public float MaxCameraDistance = 10.0f;
  public GameObject FocusTarget;

  void Start()
  {
    this.Gamepad1 = Gamepad.current;
  }

  void Update()
  {
    if(this.FocusTarget)
    {
      Move();
      Look();
    }
  }

  void Move()
  {
    transform.position += transform.TransformDirection(new Vector3(GetRightStick().x * Time.deltaTime * this.Speed, 0, 0));
    Vector3 differenceVector = transform.position - this.FocusTarget.transform.position;
    if(differenceVector.magnitude > this.MaxCameraDistance)
    {
      Vector3 newPosition = this.FocusTarget.transform.position + differenceVector.normalized * this.MaxCameraDistance;
      newPosition.y = transform.position.y;
      transform.position = newPosition;
    }
  }

  void Look()
  {
    if(this.FocusTarget != null)
    {
      transform.LookAt(this.FocusTarget.transform, Vector3.up);
    }
  }

  Vector2 GetRightStick()
  {
    if(this.Gamepad1 != null)
    {
      return this.Gamepad1.rightStick.ReadValue();
    }
    else
    {
      print("Gamepad not found.");
      return Vector2.zero;
    }
  }
}
