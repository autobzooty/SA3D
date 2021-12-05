using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class CollisionForwarder : MonoBehaviour
{
  [System.Serializable]
  public class Events
  {
    public ColliderEvent Enter;
    public ColliderEvent Exit;
  }

  public Events m_Events;


  private void OnCollisionEnter(Collision collision)
  {
    Enter(collision.collider);
  }


  private void OnTriggerEnter(Collider collision)
  {
    Enter(collision);
  }


  private void OnCollisionExit(Collision collision)
  {
    Exit(collision.collider);
  }


  private void OnTriggerExit(Collider collision)
  {
    Exit(collision);
  }


  void Enter(Collider collider)
  {
    m_Events.Enter.Invoke(collider);
  }


  void Exit(Collider collider)
  {
    m_Events.Exit.Invoke(collider);
  }
}

[System.Serializable]
public class ColliderEvent : UnityEvent<Collider> { }
