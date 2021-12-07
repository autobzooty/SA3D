using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Add new HoldHandle")]
public class HoldHandle : ScriptableObject
{
  private List<object> m_Keys = new List<object>();

  public List<object> KeysCopy { get { return new List<object>(m_Keys); } }
  public int Count { get { return m_Keys.Count; } }
  public bool HasHolds { get { return m_Keys.Count > 0;} }


  public void Add(object key)
  {
    m_Keys.Add(key);
  }


  public void Release(object key)
  {
    if (!m_Keys.Contains(key))
    {
      var m = $"Tried to release hold for {key}, but it was already free.";
      Debug.LogError(m);

      return;
    }

    m_Keys.Remove(key);
  }


  public void ReleaseAll()
  {
    m_Keys.Clear();
  }
}
