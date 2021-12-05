using UnityEngine;
using UnityEngine.Events;


[RequireComponent(typeof(Collider))]
public class CollisionForwarder : MonoBehaviour
{
  [System.Serializable]
  public class Events
  {
    public CollisionEvent CollisionEnter;
    public CollisionEvent CollisionExit;
    public ColliderEvent TriggerEnter;
    public ColliderEvent TriggerExit;
  }

  public Events m_Events;


  private void OnCollisionEnter(Collision collision)
  {
    m_Events.CollisionEnter.Invoke(collision);
  }


  private void OnCollisionExit(Collision collision)
  {
    m_Events.CollisionExit.Invoke(collision);
  }


  private void OnTriggerEnter(Collider collider)
  {
    m_Events.TriggerEnter.Invoke(collider);
  }


  private void OnTriggerExit(Collider collider)
  {
    m_Events.TriggerExit.Invoke(collider);
  }
}


[System.Serializable]
public class ColliderEvent : UnityEvent<Collider> { }
[System.Serializable]
public class CollisionEvent : UnityEvent<Collision> { }
