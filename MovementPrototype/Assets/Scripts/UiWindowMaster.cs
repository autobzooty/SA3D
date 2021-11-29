using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class UiWindowMaster : MonoBehaviour
{
  public RectTransform m_Root;
  public DialogueWindow m_DialogueWindowPrefab;
  public float m_DefaultTypingDelay = 2.0f / 60.0f;
  [Multiline]
  public string m_TestMessage =
    "<color=#fe4>So it Begins:</color><d=0.8>" + Environment.NewLine + "<r>" +
    "  Our hero has <color=#3c8>ventured forth</color>" + Environment.NewLine +
    "  into the <color=#38c>cold</color><d=0.3> <r>(at times)<d=0.3> <r>and" + Environment.NewLine +
    "  <color=#f45>frightening</color><d=0.3> <r>(usually)<d=0.3> <r>world.";

  static public UiWindowMaster Instance { get; private set; }

  [HideInInspector]
  public List<string> m_HandledTags = new List<string> { "D", "F", "R" };

  private Stack<UiWindow> m_Windows = new Stack<UiWindow>();


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
  }


  public static void DialogueWindow(string text = "")
  {
    Instance.DoDialogueWindow(text);
  }


  private void DoDialogueWindow(string text)
  {
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
}
