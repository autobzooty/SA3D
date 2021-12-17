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
    if(other.gameObject.GetComponentInParent<PlayerMover>())
    {
      print(other.name);
      if(other.name == "PlayerCollider")
      {
        Destroy(gameObject);
      }
    }
  }

  void Update()
  {
    
  }
}
