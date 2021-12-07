using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DialogueRequester : MonoBehaviour
{
  public Dialogue m_Dialogue;
  [Multiline]
  public List<string> m_Pages = new List<string>
  {
    "<color=#fe4>So it Begins:</color><p=0.8>" + Environment.NewLine +
    "  Our hero has <color=#3c8>ventured forth</color>" + Environment.NewLine +
    "  into the <color=#38c>cold</color><p=0.3> (at times)<p=0.3> and" + Environment.NewLine +
    "  <color=#f45>frightening</color><p=0.3> (usually)<p=0.3> world.",
  };


  public void OnInteractionStarted(InteractionEventData ieData)
  {
    if (m_Dialogue == null)
      UiWindowMaster.DialogueWindow(m_Pages);
    else
      UiWindowMaster.DialogueWindow(m_Dialogue);
  }
}
