using UnityEngine;
using Pixelplacement;


namespace SequenceEvents
{
  public class TweenWorldPosRotProvider : SequenceEventProvider
  {
    public TweenWorldPosRot m_Event;
    
    
    public override SequenceEvent GetEvent()
    {
      
      
      return m_Event;
    }
  }
  
  
  [System.Serializable]
  public class TweenWorldPosRot : SequenceEvent
  {
    public Transform m_Transform;
    public Vector3? m_TargetWorldPosition;
    public Quaternion? m_TargetWorldRotation;
    public float m_Duration;
    public AnimationCurve m_Curve;


    public TweenWorldPosRot(Transform transform,
      Vector3? targetWorldPosition = null,
      Quaternion? targetWorldRotation = null,
      float duration = 0.25f,
      AnimationCurve curve = null)
    {
      m_Transform = transform;
      m_TargetWorldPosition = targetWorldPosition;
      m_TargetWorldRotation = targetWorldRotation;
      m_Duration = duration;
      m_Curve = curve ?? Tween.EaseInOut;
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

      if (m_TargetWorldPosition != null)
        Tween.Position(m_Transform, m_TargetWorldPosition.Value, m_Duration, 0, m_Curve);
      if (m_TargetWorldPosition != null)
        Tween.Rotation(m_Transform, m_TargetWorldRotation.Value, m_Duration, 0, m_Curve);
    }
  }
}
