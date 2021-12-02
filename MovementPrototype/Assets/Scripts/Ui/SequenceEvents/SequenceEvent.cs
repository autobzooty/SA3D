using UnityEngine;


namespace SequenceEvents
{
  public abstract class SequenceEvent
  {
    virtual public void Execute() { }


    public void Error(object message)
    {
      if (UiWindowMaster.Instance.m_NoisySequenceEventErrors)
        Debug.LogError(message);
    }
  }
}
