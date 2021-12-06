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
  public string m_TestMessage =
    "<color=#fe4>So it Begins:</color><p=0.8>" + Environment.NewLine +
    "  Our hero has <color=#3c8>ventured forth</color>" + Environment.NewLine +
    "  into the <color=#38c>cold</color><p=0.3> (at times)<p=0.3> and" + Environment.NewLine +
    "  <color=#f45>frightening</color><p=0.3> (usually)<p=0.3> world.";

  static public UiWindowMaster Instance { get; private set; }
  static public bool TextAdvanceAvailable
  {
    get { return Instance.m_TextAdvanceAvailable; }
    set { Instance.m_TextAdvanceAvailable = value; }
  }

  private Stack<UiWindow> m_Windows = new Stack<UiWindow>();
  private bool m_TextAdvanceAvailable = true;


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
      DialogueWindow(m_TestMessage);

    ProcessInput();
  }


  void ProcessInput()
  {
    foreach (var uiWindow in m_Windows)
    {
      SendInputToWindow(uiWindow);

      if (uiWindow.m_Blocking)
        break;
    }
  }


  void SendInputToWindow(UiWindow uiWindow)
  {
    if (Input.GetButtonDown("Submit"))
    {

    }
  }


  public static void DialogueWindow(string text = "")
  {
    Instance.DoDialogueWindow(text);
  }


  private void DoDialogueWindow(string text)
  {
    if (m_Windows.Count > 0)
      return;

    // The game should pause here

    // Create the window and attach it to the window root
    // The window takes care of its own positioning and sizing
    var window = Instantiate(m_DialogueWindowPrefab, m_Root);
    window.OpenWithText(text);
    AddWindow(window);
  }


  void AddWindow(UiWindow window)
  {
    m_Windows.Push(window);
  }


  public static void ToggleTextAdvance()
  {
    Instance.m_TextAdvanceAvailable = !Instance.m_TextAdvanceAvailable;
  }
}
