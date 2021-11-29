using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputListener : MonoBehaviour
{
  private Vector2 LeftStick;
  private Vector2 RightStick;
  private bool BottomButton;
  
  void Start()
  {
        
  }

  void Update()
  {
    LeftStick = new Vector2(Input.GetAxisRaw("LHorizontal"), Input.GetAxisRaw("LVertical"));
    RightStick = new Vector2(Input.GetAxisRaw("RHorizontal"), Input.GetAxisRaw("RVertical"));
    BottomButton = Input.GetButton("BottomButton");
  }

  public Vector2 GetLeftStickVector()
  {
    float rightHeld = Input.GetKey(KeyCode.D) ? 1f : 0f;
    float leftHeld = Input.GetKey(KeyCode.A) ? -1f : 0f;
    float upHeld = Input.GetKey(KeyCode.W) ? 1f : 0f;
    float downHeld = Input.GetKey(KeyCode.S) ? -1f : 0f;

    Vector2 keyboardVector = new Vector2(rightHeld + leftHeld, upHeld + downHeld);

    if(LeftStick.magnitude <= 0.1)
    {
      LeftStick = Vector2.zero;
    }

    return LeftStick;
  }

  public Vector2 GetRightStickVector()
  {
    if(RightStick.magnitude <= 0.1)
    {
      RightStick = Vector2.zero;
    }
    return RightStick;
  }

  public bool GetBottomButton()
  {
    return BottomButton;
  }

  public bool GetBottomButtonDown()
  {
    return Input.GetButtonDown("BottomButton");
  }

  public bool GetBottomButtonUp()
  {
    return Input.GetButtonUp("BottomButton");
  }
}
