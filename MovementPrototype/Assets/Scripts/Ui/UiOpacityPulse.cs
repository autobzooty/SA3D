using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(Image))]
public class UiOpacityPulse : MonoBehaviour
{
  public float m_Period = 1f;
  public float m_Max = 0;
  public float m_Exponent = 3f;

  private Image m_Image;
  private float m_Timer = 0;
  private float m_Min;


  private void Awake()
  {
    m_Image = GetComponent<Image>();
    m_Min = m_Image.color.a;
  }


  void Update()
  {
    var f = m_Timer / m_Period;
    var range = m_Min - m_Max;
    var opacity = range * Ease(f) + m_Max;
    var color = m_Image.color;
    color.a = opacity;
    m_Image.color = color;

    m_Timer = Mathf.Repeat(m_Timer + Time.deltaTime, m_Period);
  }


  public void ResetTimer()
  {
    m_Timer = 0;
  }


  float A(float t) => Mathf.Pow(t, m_Exponent);
  float B(float t) => 1 - A(1 - t);
  float Ease(float t)
  {
    if (t < 0.25f)
      return A(4 * t) / 2;
    if (t < 0.5f)
      return (1 + B(4 * t - 1)) / 2;
    if (t < 0.75f)
      return (1 + B(3 - 4 * t)) / 2;

    return A(4 - 4 * t) / 2;
  }
}
