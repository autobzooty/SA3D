using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarScript : MonoBehaviour
{
  void Start()
  {
    
  }

  private void OnTriggerEnter(Collider other)
  {
    //B-)
    if(other.isTrigger) return;

    if(other.gameObject.GetComponentInParent<PlayerMover>())
    {
      Destroy(gameObject);
    }
  }

  void Update()
  {
    
  }
}
