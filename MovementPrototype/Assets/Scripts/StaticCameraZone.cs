//The purpose of this script is to move the game camera to a static position when the player is inside the trigger volume.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticCameraZone : MonoBehaviour
{
  public float TransitionTime = 1.0f;

  private GameObject Player;
  private GameCamera GameCamera;
  private Transform DummyCamera;
  void Start()
  {
    Player = GameObject.Find("Player");
    GameCamera = GameObject.Find("GameCamera").GetComponent<GameCamera>();
    DummyCamera = transform.Find("StaticCamera");
  }

  private void OnTriggerEnter(Collider other)
  {
    if(other.gameObject.GetComponentInParent<PlayerMover>())
    {
      print("Switching camera");
      //Change camera mode
      GameCamera.SwitchToStaticCam(DummyCamera, TransitionTime);
    }
  }

  private void OnTriggerExit(Collider other)
  {
    if(other.gameObject.GetComponentInParent<PlayerMover>())
    {
      GameCamera.SwitchToOrbitCam();
    }
  }

  void Update()
  {
        
  }
}
