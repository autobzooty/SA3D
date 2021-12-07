using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Add new Dialogue")]
public class Dialogue : ScriptableObject
{
  [Tooltip("Only for user reference -- not used by the game")]
  public string m_Description;
  [Multiline]
  public List<string> m_Pages = new List<string>();
  public List<string> m_Events = new List<string>();
}
