using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Pixelplacement;


public class DialogueWindow : UiWindow
{
  public TextMeshProUGUI m_TextMesh;
  [HideInInspector]
  public RectTransform m_RectTransform;

  private Vector2 m_TargetSize;
  private string m_Text;
  private string m_WorkingString = "";
  private int m_TypingIndex = 0;
  private bool m_Typing = false;
  private float m_TypingDelay;
  private float m_Pause = 0;
  private float m_TypingTimer = float.MaxValue;
  private float m_OpenDuration = 1f / 4f;
  private int m_GroupSize = 1;
  private int m_RemainingLines;


  private void Awake()
  {
    m_RectTransform = GetComponent<RectTransform>();
    var size = m_RectTransform.sizeDelta;
    m_TargetSize = size;
    size.x = 0;
    m_RectTransform.sizeDelta = size;
    m_TextMesh.text = "";
  }


  private void Update()
  {
    if (m_Typing)
    {
      var dt = Time.deltaTime;
      var delay =
        Input.GetButton("Text Advance") && UiWindowMaster.TextAdvanceAvailable ?
        UiWindowMaster.Instance.m_FastTypingDelay : m_TypingDelay + m_Pause;

      // Make sure that the delay isn't too small so that this
      // algorithm doesn't suffer some problems I have foreseen
      delay = Mathf.Max(dt, delay);

      if (m_TypingTimer >= delay)
      {
        m_TypingTimer -= delay;
        m_Pause = 0;
        Type();
      }

      m_TypingTimer += dt;
    }
  }


  public void OpenWithText(string text)
  {
    m_Text = text;
    CountLines(text);
    Tween.Size(m_RectTransform, m_TargetSize, m_OpenDuration, 0.1f, Tween.EaseInOut, completeCallback: OnWindowFinishedOpening);
  }


  public void OpenWithDialogue()
  {

  }


  private void OnWindowFinishedOpening()
  {
    BeginTyping();
  }


  private void BeginTyping()
  {
    m_Typing = true;
    m_TypingIndex = 0;
    m_TypingTimer = 0;
    m_TypingDelay = UiWindowMaster.Instance.m_DefaultTypingDelay;
  }


  private void Type()
  {
    AddCharToText();

    ++m_TypingIndex;

    if (m_TypingIndex >= m_Text.Length)
      EndTyping();
  }


  private void EndTyping()
  {
    m_Typing = false;
  }


  private void AddCharToText()
  {
    // First, get the char pointed at by the current
    // typing index
    var currentChar = m_Text[m_TypingIndex];

    // If the current char is a backslash, then
    // we need to skip it and add the next one,
    // regardless of what it is
    if (currentChar == '\\')
    {
      ++m_TypingIndex;

      if (m_TypingIndex >= m_Text.Length)
      {
        AddText("\\");

        return;
      }

      currentChar = m_Text[m_TypingIndex];
      AddText(currentChar.ToString());

      return;
    }

    int oldIndex;

    do
    {
      oldIndex = m_TypingIndex;
      m_TypingIndex = HandleTags(m_TypingIndex);
    } while (oldIndex != m_TypingIndex);

    if (m_TypingIndex >= m_Text.Length)
      return;

    currentChar = m_Text[m_TypingIndex];
    AddText(currentChar.ToString());
    if (currentChar == '\r')
    {
      ++m_TypingIndex;
      AddText("\n");
    }
  }


  void AddText(string text)
  {
    if (text == "\n")
      --m_RemainingLines;

    m_WorkingString += text;
    var output = m_WorkingString;
    for (var i = 0; i < m_RemainingLines; ++i)
      output += Environment.NewLine;

    if (m_WorkingString != m_Text)
      output += " ";

    m_TextMesh.text = output;
  }


  int HandleTags(int currentIndex)
  {
    var currentChar = m_Text[currentIndex];

    if (currentChar != '<')
      return currentIndex;

    var startIndex = currentIndex + 1;
    var endIndex = m_Text.IndexOf('>', startIndex);

    if (endIndex < 0)
      return currentIndex;

    var tagLength = endIndex - startIndex;
    var tagStr = m_Text.Substring(startIndex, tagLength);
    var tagSplit = tagStr.Split('=');
    var tagName = tagSplit[0].ToUpper();
    string tagValueStr;

    switch (tagName)
    {
    case "A": // Toggle the availability of text advance
      UiWindowMaster.ToggleTextAdvance();
      break;
    case "D": // Delay (in s)
      tagValueStr = tagSplit[1];

      if (!float.TryParse(tagValueStr, out var delay))
        return currentIndex;

      SetDelay(delay);
      break;
    case "E": // Execute sequence event
      break;
    case "F": // Delay (in frames @ 60 fps)
      tagValueStr = tagSplit[1];

      if (!int.TryParse(tagValueStr, out var frames))
        return currentIndex;

      SetDelayFrames(frames);
      break;
    case "G": // Group text into clusters
      tagValueStr = tagSplit[1];

      if (!int.TryParse(tagValueStr, out var groupSize))
        return currentIndex;

      SetGroupSize(groupSize);
      break;
    case "P": // Pause (in s)
      tagValueStr = tagSplit[1];

      if (!float.TryParse(tagValueStr, out var pause))
        return currentIndex;

      SetPause(pause);
      break;
    case "R": // Reset
      ResetTags();
      break;
    default:
      var fullTagStr = $"<{tagStr}>";
      AddText(fullTagStr);
      break;
    }

    return currentIndex + tagLength + 2;
  }


  void SetDelay(float delay)
  {
    m_TypingDelay = delay;
  }


  void SetDelayFrames(int frames)
  {
    SetDelay(frames / 60.0f);
  }


  void SetPause(float pause)
  {
    m_Pause = pause;
  }


  void SetGroupSize(int groupSize)
  {
    m_GroupSize = groupSize;
  }


  void ResetTags()
  {
    SetDelay(UiWindowMaster.Instance.m_DefaultTypingDelay);
    SetPause(0);
    SetGroupSize(0);
  }


  void CountLines(string page)
  {
    m_RemainingLines = page.Split('\n').Length - 1;
  }
}