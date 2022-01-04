using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pixelplacement;

public class TrashCrop : MonoBehaviour
{
  void Start()
  {
    transform.localScale = Vector3.zero;
    float spawnTime = 0.25f;
    Tween.LocalScale(transform, Vector3.one, spawnTime, 0, Tween.EaseInOut, Tween.LoopType.None);
  }
}
