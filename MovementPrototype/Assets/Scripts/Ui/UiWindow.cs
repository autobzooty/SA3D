using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class UiWindow : MonoBehaviour
{
  public bool m_Blocking = true;


  virtual public void HandleSubmit() { }
  virtual public void HandleCancel() { }
}
