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
    LeftStick = new Vector2(Input.GetAxis("LHorizontal"), Input.GetAxis("LVertical"));
    RightStick = new Vector2(Input.GetAxis("RHorizontal"), Input.GetAxis("RVertical"));
    BottomButton = Input.GetButton("BottomButton");
  }

  public Vector2 GetLeftStickVector()
  {
    float rightHeld = Input.GetKey(KeyCode.D) ? 1f : 0f;
    float leftHeld = Input.GetKey(KeyCode.A) ? -1f : 0f;
    float upHeld = Input.GetKey(KeyCode.W) ? 1f : 0f;
    float downHeld = Input.GetKey(KeyCode.S) ? -1f : 0f;

    Vector2 keyboardVector = new Vector2(rightHeld + leftHeld, upHeld + downHeld);

    Vector2 leftStickVector = new Vector2(Input.GetAxisRaw("LHorizontal"), Input.GetAxisRaw("LVertical"));
    if(leftStickVector.magnitude <= 0.1)
    {
      leftStickVector = Vector2.zero;
    }

    return leftStickVector;
  }

  public Vector2 GetRightStickVector()
  {
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
