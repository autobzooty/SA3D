using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropShadow : MonoBehaviour
{
  public GameObject DropShadowPrefab;
  private GameObject MyDropShadow;
  void Start()
  {
    MyDropShadow = GameObject.Instantiate(DropShadowPrefab);
  }

  void LateUpdate()
  {
    Ray ray = new Ray(transform.position + Vector3.up, Vector3.down);
    float maxDistance = 20.0f;
    int layerMask = LayerMask.GetMask("Default");

    Physics.Raycast(ray, out RaycastHit hit, maxDistance, layerMask, QueryTriggerInteraction.Ignore);
    MyDropShadow.transform.position = hit.point + (Vector3.up * 0.01f);
  }

  private void OnDestroy()
  {
    Destroy(MyDropShadow);
  }
}
