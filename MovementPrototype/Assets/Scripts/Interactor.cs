using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Interactor : MonoBehaviour
{
  private Transform m_Transform;
  private List<Interactive> m_Interactives = new List<Interactive>();

  private string m_CurrentInteractionText = string.Empty;

  private Interactive PrimaryInteractive { get { return m_Interactives[0]; } }


  private void Awake()
  {
    m_Transform = transform;
  }


  void Update()
  {
    SortInteractives();

    if (Input.GetButtonDown("Interact"))
      AttemptInteract();
  }


  public void OnInteractionTriggerEnter(Collider collider)
  {
    var interactive = collider.GetComponent<Interactive>();
    if (interactive == null) return;

    // If it turns out to be possible that the hash set may
    // already contain this Interactive, then I might consider
    // adding an AttemptAdd function that is called here
    Add(interactive);
  }


  public void OnInteractionTriggerExit(Collider collider)
  {
    var interactive = collider.GetComponent<Interactive>();
    if (interactive == null) return;

    Remove(interactive);
  }


  void SortInteractives()
  {
    if (m_Interactives.Count > 0)
    {
      // For now, only sort by Transform-Transform distance,
      // but later, it will be better use ray casting or
      // something to determine line of sight, and to take
      // the interactor's facing direction into account

      m_Interactives.Sort((a, b) =>
      {
        var txA = a.transform;
        var posA = txA.position;
        var sqDistA = (posA - m_Transform.position).sqrMagnitude;
        var txB = b.transform;
        var posB = txB.position;
        var sqDistB = (posB - m_Transform.position).sqrMagnitude;

        if (sqDistA == sqDistB) return 0;
        return sqDistA < sqDistB ? -1 : 1;
      });

      UpdateText();
    }
  }


  void UpdateText()
  {
    var primaryText = PrimaryInteractive.m_InteractionText;

    if (m_CurrentInteractionText != primaryText)
    {
      // Update the text in the UI element
      m_CurrentInteractionText = primaryText;
      Debug.Log(m_CurrentInteractionText);
    }
  }


  void AttemptInteract()
  {
    if (m_Interactives.Count == 0) return;

    
  }


  void Interact()
  {

  }


  void Add(Interactive interactive)
  {
    if (m_Interactives.Count == 0)
      CreateUiElement(interactive.m_InteractionText);

    m_Interactives.Add(interactive);
  }


  void Remove(Interactive interactive)
  {
    m_Interactives.Remove(interactive);

    if (m_Interactives.Count == 0)
      RemoveUiElement();
  }


  void CreateUiElement(string text)
  {
    // Create the UI element that tells the player what
    // interaction will occur when they press the
    // Interact button

  }


  void RemoveUiElement()
  {
    // Remove the interaction text UI element
    m_CurrentInteractionText = "";
    Debug.Log(m_CurrentInteractionText);
  }
}
