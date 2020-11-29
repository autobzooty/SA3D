using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameplayCamera : MonoBehaviour
{
  private Gamepad Gamepad1;

  public float Speed = 10.0f;
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
    transform.Rotate(this.FocusTarget.transform.up, this.Speed, Space.Self);
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
