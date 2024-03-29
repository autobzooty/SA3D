using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class UiWindowMaster : MonoBehaviour
{
  public RectTransform m_Root;
  public DialogueWindow m_DialogueWindowPrefab;
  public HoldHandle m_PlayerInputHoldHandle;
  public HoldHandle m_PlayerUpdateHoldHandle;
  public HoldHandle m_CameraInputHoldHandle;
  public float m_DefaultTypingDelay = 2.0f / 60.0f;
  public float m_FastTypingDelay = 1.0f / 60.0f;
  public bool m_NoisySequenceEventErrors = true;
  [Multiline]
  public List<string> m_TestPages = new List<string>
  {
    "<color=#fe4>So it Begins:</color><p=0.8>" + Environment.NewLine +
    "  Our hero has <color=#3c8>ventured forth</color>" + Environment.NewLine +
    "  into the <color=#38c>cold</color><p=0.3> (at times)<p=0.3> and" + Environment.NewLine +
    "  <color=#f45>frightening</color><p=0.3> (usually)<p=0.3> world.",
  };

  static public UiWindowMaster Instance { get; private set; }
  static public bool TextAdvanceAvailable
  {
    get { return Instance.m_TextAdvanceAvailable; }
    set { Instance.m_TextAdvanceAvailable = value; }
  }

  private List<UiWindow> m_Windows = new List<UiWindow>();
  private bool m_TextAdvanceAvailable = true;
  private int m_PlayerInputHolders = 0;
  private int m_PlayerUpdateHolders = 0;
  private int m_CameraInputHolders = 0;


  void Start()
  {
    if (Instance != null)
      Debug.LogError("The UiWindowMaster " + Instance +
        " already exists, but now here comes " + this + "...?");
    else
      Instance = this;
  }


  private void Update()
  {
    if (Input.GetKeyDown(KeyCode.Space) && m_Windows.Count <= 0)
      DialogueWindow(m_TestPages);

    ProcessInput();
  }


  private void LateUpdate()
  {
    CleanUp();
  }


  void ProcessInput()
  {
    foreach (var uiWindow in m_Windows)
    {
      SendInputToWindow(uiWindow);

      if (uiWindow.m_BlocksUiInput)
        break;
    }
  }


  void SendInputToWindow(UiWindow uiWindow)
  {
    if (Input.GetButtonDown("Submit"))
    {
      uiWindow.HandleSubmit();
    }
  }


  void CleanUp()
  {
    var activeWindows = new List<UiWindow>();

    foreach (var window in m_Windows)
    {
      if (window != null)
        activeWindows.Add(window);
    }

    m_Windows = activeWindows;
  }


  public static void DialogueWindow(List<string> pages)
  {
    if (pages == null)
      pages = new List<string> { "" };

    Instance.DoDialogueWindow(pages);
  }


  public static void DialogueWindow(Dialogue dialogue)
  {
    Instance.DoDialogueWindow(dialogue);
  }


  private void DoDialogueWindow(List<string> pages)
  {
    // Create the window and attach it to the window root
    // The window takes care of its own positioning and sizing
    var window = Instantiate(m_DialogueWindowPrefab, m_Root);
    window.OpenWithPages(pages);
    AddWindow(window);
  }


  private void DoDialogueWindow(Dialogue dialogue)
  {
    // Create the window and attach it to the window root
    // The window takes care of its own positioning and sizing
    var window = Instantiate(m_DialogueWindowPrefab, m_Root);
    window.OpenWithDialogue(dialogue);
    AddWindow(window);
  }


  void AddWindow(UiWindow window)
  {
    m_Windows.Add(window);

    if (window.m_HoldsPlayerInput)
    {
      if (m_PlayerInputHolders <= 0)
        AddPlayerInputHold();

      ++m_PlayerInputHolders;
    }

    if (window.m_HoldsPlayerUpdate)
    {
      if (m_PlayerUpdateHolders <= 0)
        AddPlayerUpdateHold();

      ++m_PlayerUpdateHolders;
    }

    if (window.m_HoldsCameraInput)
    {
      if (m_CameraInputHolders <= 0)
        AddCameraInputHold();

      ++m_CameraInputHolders;
    }
  }


  /// Do not call unless you are a UiWindow!
  public void WindowHasClosed(UiWindow window)
  {
    if (window.m_HoldsPlayerInput)
    {
      --m_PlayerInputHolders;

      if (m_PlayerInputHolders <= 0)
        ReleasePlayerInputHold();
    }

    if (window.m_HoldsPlayerUpdate)
    {
      --m_PlayerUpdateHolders;

      if (m_PlayerUpdateHolders <= 0)
        ReleasePlayerUpdateHold();
    }

    if (window.m_HoldsCameraInput)
    {
      --m_CameraInputHolders;

      if (m_CameraInputHolders <= 0)
        ReleaseCameraInputHold();
    }

    Destroy(window.gameObject);
    // Actual removal from the list occurs in LateUpdate
  }


  public static void ToggleTextAdvance()
  {
    Instance.m_TextAdvanceAvailable = !Instance.m_TextAdvanceAvailable;
  }


  void AddPlayerInputHold()
  {
    m_PlayerInputHoldHandle.Add(this);
  }


  void ReleasePlayerInputHold()
  {
    m_PlayerInputHoldHandle.Release(this);
  }


  void AddPlayerUpdateHold()
  {
    m_PlayerUpdateHoldHandle.Add(this);
  }


  void ReleasePlayerUpdateHold()
  {
    m_PlayerUpdateHoldHandle.Release(this);
  }


  void AddCameraInputHold()
  {
    m_CameraInputHoldHandle.Add(this);
  }


  void ReleaseCameraInputHold()
  {
    m_CameraInputHoldHandle.Release(this);
  }
}
