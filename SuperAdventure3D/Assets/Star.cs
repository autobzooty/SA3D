using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pixelplacement;
using UnityEngine.SceneManagement;

public class Star : MonoBehaviour
{
  // Start is called before the first frame update
  void Start()
  {
    Tween.LocalPosition(this.transform, this.transform.position + new Vector3(0, -0.25f, 0), 0.75f, 0, Tween.EaseInOut, Tween.LoopType.PingPong);
  }

  // Update is called once per frame
  void Update()
  {
        
  }

  private void OnTriggerEnter(Collider other)
  {
    GameObject obj = other.gameObject;
    if(obj.GetComponent<PlayerMover>())
    {
      int scene = SceneManager.GetActiveScene().buildIndex;
      SceneManager.LoadScene(scene, LoadSceneMode.Single);
    }
  }
}
