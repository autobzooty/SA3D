using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spinner : MonoBehaviour
{
  public Transform SpinnerTarget;
  public float SpinSpeed = 20.0f;
  void Start()
  {
    if(SpinnerTarget == null)
    {
      SpinnerTarget = transform;
    }
  }

  void Update()
  {
    transform.Rotate(Vector3.up, SpinSpeed * Time.deltaTime);
  }
}
