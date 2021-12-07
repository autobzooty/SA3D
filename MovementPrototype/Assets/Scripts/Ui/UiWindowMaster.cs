using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class UiWindowMaster : MonoBehaviour
{
  public RectTransform m_Root;
  public DialogueWindow m_DialogueWindowPrefab;
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
  private int m_PlayerHolders = 0;
  private int m_CameraHolders = 0;


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


  private void DoDialogueWindow(List<string> pages)
  {
    if (m_Windows.Count > 0)
      return;

    // Create the window and attach it to the window root
    // The window takes care of its own positioning and sizing
    var window = Instantiate(m_DialogueWindowPrefab, m_Root);
    window.OpenWithPages(pages);
    AddWindow(window);
  }


  void AddWindow(UiWindow window)
  {
    m_Windows.Add(window);

    if (window.m_HoldsPlayer)
      ++m_PlayerHolders;
    if (window.m_HoldsCamera)
      ++m_CameraHolders;

    if (m_PlayerHolders > 0)
      AddPlayerHold();
    if (m_CameraHolders > 0)
      AddCameraHold();
  }


  /// Do not call unless you are a UiWindow!
  public void WindowHasClosed(UiWindow window)
  {
    if (window.m_HoldsPlayer)
      --m_PlayerHolders;
    if (window.m_HoldsCamera)
      --m_CameraHolders;

    if (m_PlayerHolders <= 0)
      ReleasePlayerHold();
    if (m_CameraHolders <= 0)
      ReleaseCameraHold();

    Destroy(window.gameObject);
    // Actual removal from the list occurs in LateUpdate
  }


  public static void ToggleTextAdvance()
  {
    Instance.m_TextAdvanceAvailable = !Instance.m_TextAdvanceAvailable;
  }


  void AddPlayerHold()
  {

  }


  void ReleasePlayerHold()
  {

  }


  void AddCameraHold()
  {

  }


  void ReleaseCameraHold()
  {

  }
}
