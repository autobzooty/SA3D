using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathPlane : MonoBehaviour
{
  void Start()
  {
    GetComponent<MeshRenderer>().enabled = false;
  }
}