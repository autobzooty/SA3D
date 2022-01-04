using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrashCropSpawner : MonoBehaviour
{
  public GameObject TrashPrefab;
  public float MinSpawnTime = 10.0f;
  public float MaxSpawnTime = 25.0f;

  private float SpawnStopwatch = 0.0f;
  private float CurrentSpawnTime = 10.0f;
  private GameObject MyTrash;

  void Start()
  {
    ResetSpawnTime();
  }

  void Update()
  {
    if(MyTrash != null) return;

    SpawnStopwatch += Time.deltaTime;
    if(SpawnStopwatch >= CurrentSpawnTime)
    {
      SpawnTrash();
    }
  }

  void ResetSpawnTime()
  {
    CurrentSpawnTime = Random.Range(MinSpawnTime, MaxSpawnTime);
    SpawnStopwatch = 0;
  }

  void SpawnTrash()
  {
    MyTrash = GameObject.Instantiate(TrashPrefab);
    MyTrash.transform.position = transform.position;
  }

  public GameObject GetTrashReference()
  {
    return MyTrash;
  }

  public void DestroyTrash()
  {
    GameObject.Destroy(MyTrash);
    ResetSpawnTime();
  }
}
