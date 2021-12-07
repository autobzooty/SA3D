using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class UiWindow : MonoBehaviour
{
  public bool m_BlocksUiInput = true;
  public bool m_HoldsPlayer = true;
  public bool m_HoldsCamera = true;


  virtual public void Open() { }
  virtual public void Close()
  {
    UiWindowMaster.Instance.WindowHasClosed(this);
  }
  virtual public void HandleSubmit() { }
  virtual public void HandleCancel() { }
}
