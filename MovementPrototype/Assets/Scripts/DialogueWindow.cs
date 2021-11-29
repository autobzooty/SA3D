using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Pixelplacement;


public class DialogueWindow : MonoBehaviour
{
  public TextMeshProUGUI m_TextMesh;
  [HideInInspector]
  public RectTransform m_RectTransform;

  private Vector2 m_TargetSize;
  private string m_Text;
  private int m_TypingIndex = 0;
  private bool m_Typing = false;
  private float m_TypingDelay;
  private float m_TypingTimer = float.MaxValue;


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
      if (m_TypingTimer >= m_TypingDelay)
      {
        m_TypingTimer -= m_TypingDelay;
        Type();
      }

      m_TypingTimer += Time.deltaTime;
    }
  }


  public void OpenWithText(string text)
  {
    m_Text = text;

    Tween.Size(m_RectTransform, m_TargetSize, 0.25f, 0, Tween.EaseInOut, completeCallback: OnWindowFinishedOpening);
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
    m_TextMesh.text += text;
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

    if (!UiWindowMaster.Instance.m_HandledTags.Contains(tagName))
    {
      var fullTagStr = $"<{tagStr}>";

      AddText(fullTagStr);
    }
    else if (tagName == "D")
    {
      var tagValueStr = tagSplit[1];

      if (!float.TryParse(tagValueStr, out var delay))
        return currentIndex;

      SetDelay(delay);
    }
    else if (tagName == "F")
    {
      var tagValueStr = tagSplit[1];

      if (!int.TryParse(tagValueStr, out var frames))
        return currentIndex;

      SetDelayFrames(frames);
    }
    else if (tagName == "R")
    {
      ResetDelay();
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


  void ResetDelay()
  {
    SetDelay(UiWindowMaster.Instance.m_DefaultTypingDelay);
  }
}