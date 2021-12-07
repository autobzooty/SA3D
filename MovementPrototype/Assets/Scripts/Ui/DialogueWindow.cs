using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Pixelplacement;


public class DialogueWindow : UiWindow
{
  public TextMeshProUGUI m_TextMesh;
  public Image m_MoreIconImage;
  public Image m_EndIconImage;

  private RectTransform m_RectTransform;
  private UiOpacityPulse m_MoreIconPulse;
  private UiOpacityPulse m_EndIconPulse;
  private Vector2 m_ClosedSize;
  private Vector2 m_OpenSize;
  private List<string> m_Pages;
  private int m_PageIndex;
  private string m_CurrentPage;
  private string m_WorkingString;
  private int m_TypingIndex = 0;
  private bool m_Typing = false;
  private float m_TypingDelay;
  private float m_Pause = 0;
  private float m_TypingTimer = float.MaxValue;
  private const float m_OpenDuration = 0.25f;
  private const float m_OpenDelay = 0f;
  private int m_GroupSize = 1;
  private int m_RemainingLines;

  private bool m_PagesAreLeft { get { return m_PageIndex < m_Pages.Count - 1; } }


  private void Awake()
  {
    m_RectTransform = GetComponent<RectTransform>();
    m_MoreIconPulse = m_MoreIconImage.GetComponent<UiOpacityPulse>();
    m_EndIconPulse = m_EndIconImage.GetComponent<UiOpacityPulse>();
    m_OpenSize = m_ClosedSize = m_RectTransform.sizeDelta;
    m_ClosedSize.x = 0;
    m_RectTransform.sizeDelta = m_ClosedSize;
    m_TextMesh.text = "";
  }


  private void Update()
  {
    if (m_Typing)
    {
      var dt = Time.deltaTime;
      Type(dt);
    }
  }


  public override void HandleSubmit()
  {
    if (m_Typing) return;

    if (m_PagesAreLeft)
      NextPage();
    else
      Close();
  }


  public void OpenWithString(string text)
  {
    OpenWithPages(new List<string> { text });
  }


  public void OpenWithPages(List<string> pages)
  {
    m_Pages = pages;
    m_PageIndex = 0;
    Open();
  }


  public void OpenWithDialogue(Dialogue dialogue)
  {
    if (dialogue == null)
    {
      OpenWithString("");
      return;
    }

    OpenWithPages(dialogue.m_Pages);
    // TODO:
    //   do something with the dialogue's m_Events list
  }


  override public void Open()
  {
    base.Open();

    HideIcons();

    Tween.Size(m_RectTransform, m_OpenSize, m_OpenDuration, m_OpenDelay,
      Tween.EaseInOut, completeCallback: OnWindowFinishedOpening);
  }


  override public void Close()
  {
    HideIcons();
    m_TextMesh.text = "";

    Tween.Size(m_RectTransform, m_ClosedSize, m_OpenDuration, 0,
      Tween.EaseInOut, completeCallback: OnWindowFinishedClosing);
  }


  private void OnWindowFinishedOpening()
  {
    SetUpCurrentPage();
  }


  private void OnWindowFinishedClosing()
  {
    base.Close();
  }


  public void SetUpCurrentPage()
  {
    m_WorkingString = "";
    m_TextMesh.text = "";
    m_CurrentPage = m_Pages[m_PageIndex];
    CountLines(m_CurrentPage);

    BeginTyping();
  }


  public void NextPage()
  {
    // TODO:
    //   come up with a cool way to clear the text

    ++m_PageIndex;
    SetUpCurrentPage();
  }


  private void BeginTyping()
  {
    m_Typing = true;
    m_TypingIndex = 0;
    m_TypingTimer = 0;
    m_TypingDelay = UiWindowMaster.Instance.m_DefaultTypingDelay;

    HideIcons();
  }


  private void Type(float dt)
  {
    var delay =
        Input.GetButton("Text Advance") && UiWindowMaster.TextAdvanceAvailable ?
        UiWindowMaster.Instance.m_FastTypingDelay : m_TypingDelay + m_Pause;

    // Make sure that the delay isn't too small so that this
    // algorithm doesn't suffer some problems I have foreseen
    delay = Mathf.Max(dt, delay);

    // The typing timer is initialized to float.MaxValue so that no
    // delay occurs before the first character typed in every window
    if (m_TypingTimer >= delay)
    {
      m_TypingTimer -= delay;
      m_Pause = 0;

      // TODO:
      //   Implement group typing, presumably by calling TypeNext in
      //   a loop up to m_GroupSize times to type multiple characters
      TypeNext();
    }

    m_TypingTimer += dt;
  }


  private void EndTyping()
  {
    m_Typing = false;

    if (m_PagesAreLeft)
      ShowMoreIcon();
    else
      ShowEndIcon();
  }


  private void TypeNext()
  {
    AddCharToText();

    ++m_TypingIndex;

    if (m_TypingIndex >= m_CurrentPage.Length)
      EndTyping();
  }


  private void AddCharToText()
  {
    // This can only ever happen if a null or empty
    // string or list is sent to the UiWindowMaster
    if (string.IsNullOrEmpty(m_CurrentPage))
      return;

    // First, get the char pointed at by the current
    // typing index
    var currentChar = m_CurrentPage[m_TypingIndex];

    // If the current char is a backslash, then
    // we need to skip it and add the next one,
    // regardless of what it is
    if (currentChar == '\\')
    {
      ++m_TypingIndex;

      if (m_TypingIndex >= m_CurrentPage.Length)
      {
        AddText("\\");

        return;
      }

      currentChar = m_CurrentPage[m_TypingIndex];
      AddText(currentChar.ToString());

      return;
    }

    int oldIndex;

    do
    {
      oldIndex = m_TypingIndex;
      m_TypingIndex = HandleTags(m_TypingIndex);
    } while (oldIndex != m_TypingIndex && m_TypingIndex < m_CurrentPage.Length);

    if (m_TypingIndex >= m_CurrentPage.Length)
      return;

    currentChar = m_CurrentPage[m_TypingIndex];
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

    if (m_WorkingString != m_CurrentPage)
      output += " ";

    m_TextMesh.text = output;
  }


  int HandleTags(int currentIndex)
  {
    var currentChar = m_CurrentPage[currentIndex];

    if (currentChar != '<')
      return currentIndex;

    var startIndex = currentIndex + 1;
    var endIndex = m_CurrentPage.IndexOf('>', startIndex);

    if (endIndex < 0)
      return currentIndex;

    var tagLength = endIndex - startIndex;
    var tagStr = m_CurrentPage.Substring(startIndex, tagLength);
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
    case "Q": // Queue up a sequence event
      break;
    case "R": // Reset
      ResetTags();
      break;
    case "S": // Text style
      // TODO:
      //   implement text styles!
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


  void ShowMoreIcon()
  {
    m_MoreIconImage.enabled = true;
    m_MoreIconPulse.enabled = true;
    m_MoreIconPulse.ResetTimer();
  }


  void HideMoreIcon()
  {
    m_MoreIconImage.enabled = false;
    m_MoreIconPulse.enabled = false;
  }


  void ShowEndIcon()
  {
    m_EndIconImage.enabled = true;
    m_EndIconPulse.enabled = true;
    m_EndIconPulse.ResetTimer();
  }


  void HideEndIcon()
  {
    m_EndIconImage.enabled = false;
    m_EndIconPulse.enabled = false;
  }


  void HideIcons()
  {
    HideMoreIcon();
    HideEndIcon();
  }
}

// TODO:
//   Add Final Fantasy Tactics-style page scrolling
