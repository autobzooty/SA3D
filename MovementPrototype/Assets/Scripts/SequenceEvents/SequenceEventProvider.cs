using UnityEngine;


namespace SequenceEvents
{
  public class SequenceEventProvider : MonoBehaviour
  {
    public virtual SequenceEvent GetEvent() { return null; }
  }


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
