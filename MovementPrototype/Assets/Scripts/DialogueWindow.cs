using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class DialogueWindow : MonoBehaviour
{
  public RectTransform mRectTransform;
  public TextMeshProUGUI mTextMesh;

  private float mTargetW;


  void Start()
  {
    mTargetW = mRectTransform.rect.width;
  }


  void OpenWithText(string text)
  {
    mTextMesh.SetText(text);
  }
}
