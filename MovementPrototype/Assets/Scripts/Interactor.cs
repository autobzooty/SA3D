using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Interactor : MonoBehaviour
{
  private HashSet<Interactive> m_CurrentInteractives = new HashSet<Interactive>();


  void Update()
  {
    if (Input.GetButtonDown("Interact"))
      AttemptInteract();
  }


  void AttemptInteract()
  {
    if (m_CurrentInteractives.Count == 0) return;


  }


  void Interact()
  {

  }


  void OnTriggerEnter(Collider other)
  {
    var interactive = other.GetComponent<Interactive>();
    if (interactive == null) return;

    // If it turns out to be possible that the hash set may
    // already contain this Interactive, then I might consider
    // adding an AttemptAdd function that is called here
    Add(interactive);
  }


  void OnTriggerExit(Collider other)
  {
    var interactive = other.GetComponent<Interactive>();
    if (interactive == null) return;

    Remove(interactive);
  }


  void Add(Interactive interactive)
  {


    m_CurrentInteractives.Add(interactive);
  }


  void Remove(Interactive interactive)
  {


    m_CurrentInteractives.Remove(interactive);
  }
}
