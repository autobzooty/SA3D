using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputListener : MonoBehaviour
{
  public HoldHandle m_PlayerInputHoldHandle;
  public HoldHandle m_CameraInputHoldHandle;
  private Vector2 LeftStick;
  private Vector2 RightStick;
  private bool BottomButton;
  private bool LeftButton;
  
  void Start()
  {
        
  }

  void Update()
  {
    LeftStick = new Vector2(Input.GetAxisRaw("LHorizontal"), Input.GetAxisRaw("LVertical"));
    RightStick = new Vector2(Input.GetAxisRaw("RHorizontal"), Input.GetAxisRaw("RVertical"));
    BottomButton = Input.GetButton("BottomButton");
    LeftButton = Input.GetButton("LeftButton");
  }

  public Vector2 GetLeftStickVector()
  {
    if (m_PlayerInputHoldHandle != null && m_PlayerInputHoldHandle.HasHolds)
      return Vector2.zero;

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
    if (m_CameraInputHoldHandle != null && m_CameraInputHoldHandle.HasHolds)
      return Vector2.zero;

    if(RightStick.magnitude <= 0.1)
    {
      RightStick = Vector2.zero;
    }
    return RightStick;
  }

  public bool GetBottomButton()
  {
    if (m_PlayerInputHoldHandle != null && m_PlayerInputHoldHandle.HasHolds)
      return false;

    return BottomButton;
  }

  public bool GetBottomButtonDown()
  {
    if (m_PlayerInputHoldHandle != null && m_PlayerInputHoldHandle.HasHolds)
      return false;

    return Input.GetButtonDown("BottomButton");
  }

  public bool GetBottomButtonUp()
  {
    if (m_PlayerInputHoldHandle != null && m_PlayerInputHoldHandle.HasHolds)
      return false;

    return Input.GetButtonUp("BottomButton");
  }

  public bool GetLeftButton()
  {
    if (m_PlayerInputHoldHandle != null && m_PlayerInputHoldHandle.HasHolds)
      return false;

    return LeftButton;
  }

  public bool GetLeftButtonDown()
  {
    if (m_PlayerInputHoldHandle != null && m_PlayerInputHoldHandle.HasHolds)
      return false;

    return Input.GetButtonDown("LeftButton");
  }

  public bool GetLeftButtonUp()
  {
    if (m_PlayerInputHoldHandle != null && m_PlayerInputHoldHandle.HasHolds)
      return false;

    return Input.GetButtonUp("LeftButton");
  }
}