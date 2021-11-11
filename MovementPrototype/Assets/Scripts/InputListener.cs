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
    return LeftStick;
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
