using UnityEngine;


namespace SequenceEvents
{
  public class SetWorldPosRotProvider : SequenceEventProvider
  {
    public SetWorldPosRot m_Event;
    
    
    public override SequenceEvent GetEvent()
    {
      
      
      return m_Event;
    }
  }
  
  
  [System.Serializable]
  public class SetWorldPosRot : SequenceEvent
  {
    public Transform m_Transform;
    public Vector3? m_TargetWorldPosition;
    public Quaternion? m_TargetWorldRotation;


    public SetWorldPosRot(Transform transform,
      Vector3? targetWorldPosition = null,
      Quaternion? targetWorldRotation = null)
    {
      m_Transform = transform;
      m_TargetWorldPosition = targetWorldPosition;
      m_TargetWorldRotation = targetWorldRotation;
    }


    public override void Execute()
    {
      if (m_Transform == null)
      {
        Error("Null Transform");
        return;
      }

      if (m_TargetWorldPosition == null &&
          m_TargetWorldRotation == null)
      {
        Error("All null target values");
        return;
      }

      if (m_TargetWorldPosition != null) m_Transform.position = m_TargetWorldPosition.Value;
      if (m_TargetWorldRotation != null) m_Transform.rotation = m_TargetWorldRotation.Value;
    }
  }
}
