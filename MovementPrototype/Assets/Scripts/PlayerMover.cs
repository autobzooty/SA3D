using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(InputListener))]

public class PlayerMover : MonoBehaviour
{
  //Public Variables
  public float TurnSpeed = 15.0f;
  public float Acceleration = 5.0f;
  public float MaxRunSpeed = 4.0f;
  public float Gravity = 10.0f;
  public float StandableGroundAngle = 50.0f;
  public float GroundAngle = 89.0f;
  public float CeilingAngle = 50.0f;
  public Collider MovementCollider;
  public float StepHeight = 0.2f;
  public float InitialJumpStrength = 5.0f;
  public float AdditiveJumpStrength = 1.0f;
  public float JumpLaunchTime = 5f/60f;
  public Transform VelocityFacer;
  public float MinimumJumpLaunchTime = 5/60;
  public float JumpStrengthSpeedScalar = 1.8f;
  public float DiveForwardStrength = 2.0f;
  public float DiveDownwardStrength = 1.0f;

  //Components
  private InputListener IL;

  //Private variables
  private GameObject GameCamera;
  private Vector3 HSpeed = new Vector3();
  private float VSpeed = 0.0f;
  private bool OnGround = true;
  private bool Sliding = true;
  private bool Diving = false;
  private bool PreviousOnGround = true;
  private float Deceleration;
  private ContactPoint[] Contacts = new ContactPoint[0];
  private float StandableGroundDotValue;
  private float GroundDotValue;
  private float CeilingDotValue;
  private Vector3 MovementDirection;
  private float JumpLaunchStopwatch = 0.0f;
  private bool JumpLaunching = false;

  void Start()
  {
    IL = GetComponent<InputListener>();
    Deceleration = Acceleration * 3;
    StandableGroundDotValue = Mathf.Cos(Mathf.Deg2Rad * StandableGroundAngle);
    GroundDotValue = Mathf.Cos(Mathf.Deg2Rad * GroundAngle);
    CeilingDotValue = Mathf.Cos(Mathf.Deg2Rad * CeilingAngle);
    FindGameCamera();
  }

  void Update()
  {
    Turn();
    HSpeedUpdate();
    VSpeedUpdate();
    Move();
  }

  void FindGameCamera()
  {
    GameCamera = GameObject.Find("GameCamera");
    if(!GameCamera)
    {
      print("Warning! No game camera was found! Shit will probably break!");
    }
  }

  void Turn()
  {
    if(OnGround)
    {
      if(Sliding)
      {
        int layerMask = LayerMask.GetMask("Default");
        Ray ray = new Ray(transform.position + transform.up, -transform.up);
        if(Physics.Raycast(ray, out RaycastHit hit, 2, layerMask))
        {
          Vector3 targetLookDirection = hit.normal;
          targetLookDirection.y = 0;
          targetLookDirection.Normalize();

          Vector3 newDirection;
          if(Vector3.Dot(transform.forward, targetLookDirection) >= 0)
          {
            newDirection = Vector3.RotateTowards(transform.forward, targetLookDirection, TurnSpeed * 0.05f * Time.deltaTime, 0.0f);
          }
          else
          {
            newDirection = Vector3.RotateTowards(transform.forward, -targetLookDirection, TurnSpeed * 0.05f * Time.deltaTime, 0.0f);
          }
          transform.rotation = Quaternion.LookRotation(newDirection);
        }
      }
      else
      {
        //Put the left stick input into camera space
        Vector3 targetLookDirection = GameCamera.transform.TransformDirection(new Vector3(IL.GetLeftStickVector().x, 0, IL.GetLeftStickVector().y));
        //We never want the character to look up or down, so 0 this value out
        targetLookDirection.y = 0;
        //Normalize the vector for good measure since we modified the Y value, though this probably doesn't matter
        targetLookDirection.Normalize();
        //Rotate towards the target rotation at a set speed
        Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetLookDirection, TurnSpeed * Time.deltaTime, 0.0f);
        transform.rotation = Quaternion.LookRotation(newDirection);
      }
    }
  }

  void HSpeedUpdate()
  {
    if(OnGround)
    {
      float leftStickMagnitude = IL.GetLeftStickVector().magnitude;
      leftStickMagnitude = Mathf.Clamp(leftStickMagnitude, 0, 1);
      if(Sliding)
      {
        int layerMask = LayerMask.GetMask("Default");
        Ray ray = new Ray(transform.position + transform.up, -transform.up);
        if(Physics.Raycast(ray, out RaycastHit hit, 2, layerMask))
        {
          Vector3 targetLookDirection = hit.normal;
          targetLookDirection.y = 0;
          targetLookDirection.Normalize();

          Vector3 localTargetLookDirection = transform.InverseTransformDirection(targetLookDirection);
          HSpeed += Acceleration * localTargetLookDirection * Time.deltaTime;
        }
      }
      else if(Diving)
      {
        if(HSpeed.magnitude < Deceleration * Time.deltaTime)
        {
          HSpeed = Vector3.zero;
        }
        else
        {
          HSpeed += -HSpeed.normalized * Deceleration * 0.25f * Time.deltaTime;
        }
      }
      else
      {
        HSpeed.y = 0;
        HSpeed.x = 0;

        if(leftStickMagnitude > 0)
        {
          //MaxRunSpeed changes based on the magnitude of the left stick
          //If we're below this threshold, we accelerate, otherwise we decelerate
          if(HSpeed.magnitude < MaxRunSpeed * leftStickMagnitude)
          {
            HSpeed.z += Acceleration * leftStickMagnitude * Time.deltaTime;
          }
          else
          {
            if(HSpeed.magnitude < Deceleration * Time.deltaTime)
            {
              HSpeed = Vector3.zero;
            }
            else
            {
              HSpeed += -HSpeed.normalized * Deceleration * Time.deltaTime;
            }
          }
        }
        else
        {
          if(HSpeed.magnitude < Deceleration * Time.deltaTime)
          {
            HSpeed = Vector3.zero;
          }
          else
          {
            HSpeed += -HSpeed.normalized * Deceleration * Time.deltaTime;
          }
        }
      }
    }
    else
    {
      Vector3 targetMoveDirection = GameCamera.transform.TransformDirection(new Vector3(IL.GetLeftStickVector().x, 0, IL.GetLeftStickVector().y));
      targetMoveDirection.y = 0;
      targetMoveDirection.Normalize();

      Debug.DrawRay(transform.position, targetMoveDirection, Color.red);

      targetMoveDirection = transform.InverseTransformDirection(targetMoveDirection);
      targetMoveDirection.y = 0;
      targetMoveDirection.Normalize();


      if((HSpeed + targetMoveDirection * 3 * Time.deltaTime).magnitude < MaxRunSpeed)
      {
        HSpeed += targetMoveDirection * 3 * Time.deltaTime;
      }
    }
    
  }

  void VSpeedUpdate()
  {
    UpdateOnGround();
    AttemptDive();
    UpdateGraphicals();

    if(JumpLaunching)
    {
      if(JumpLaunchStopwatch == 0.0f)
      {
        float t = HSpeed.magnitude / MaxRunSpeed;
        VSpeed += Mathf.LerpUnclamped(InitialJumpStrength, InitialJumpStrength * JumpStrengthSpeedScalar, t);
      }

      VSpeed += AdditiveJumpStrength * Time.deltaTime;
      JumpLaunchStopwatch += Time.deltaTime;

      if(JumpLaunchStopwatch >= MinimumJumpLaunchTime)
      {
        if(!IL.GetBottomButton() || JumpLaunchStopwatch >= JumpLaunchTime)
        {
          JumpLaunching = false;
          JumpLaunchStopwatch = 0.0f;
        }
      }
    }

    if(OnGround && !JumpLaunching)
    {
      if(VSpeed != 0)
      {
        //If we are in this block, it means we landed on this frame.
        //TO-DO: Kill HSpeed if left stick is neutral on landing
        if(IL.GetLeftStickVector().magnitude == 0)
        {
          HSpeed = Vector3.zero;
        }
      }
      VSpeed = 0;
    }
    else
    {
      VSpeed -= Gravity * Time.deltaTime;
    }
    if(VSpeed > 0)
    {
      if(CeilingCheck())
      {
        VSpeed = 0;
      }
    }
  }

  bool CeilingCheck()
  {
    Ray[] ceilingCheckRays = new Ray[5];
    ceilingCheckRays[0] = new Ray(transform.position + transform.up * StepHeight, transform.up);
    ceilingCheckRays[1] = new Ray(transform.position + transform.up * StepHeight + transform.right * 0.14f, transform.up);
    ceilingCheckRays[2] = new Ray(transform.position + transform.up * StepHeight + -transform.right * 0.14f, transform.up);
    ceilingCheckRays[3] = new Ray(transform.position + transform.up * StepHeight + transform.forward * 0.14f, transform.up);
    ceilingCheckRays[4] = new Ray(transform.position + transform.up * StepHeight + -transform.forward * 0.14f, transform.up);

    foreach(Ray ray in ceilingCheckRays)
    {
      //Debug.DrawRay(ray.origin, ray.direction, Color.blue);

      float castLength = 1 - StepHeight + VSpeed * Time.deltaTime;

      if(Physics.Raycast(ray, out RaycastHit hit, castLength))
      {
        if(Vector3.Dot(Vector3.down, hit.normal) >= CeilingDotValue)
        {
          return true;
        }
      }
    }
    return false;
  }

  //Also updates Sliding
  void UpdateOnGround()
  {
    //Default OnGround to false. If we find a ground normal, we'll flip it to true
    OnGround = false;

    Ray[] groundCheckRays = new Ray[5];
    groundCheckRays[0] = new Ray(transform.position + transform.up, -transform.up);
    groundCheckRays[1] = new Ray(transform.position + transform.up + transform.right * 0.14f, -transform.up);
    groundCheckRays[2] = new Ray(transform.position + transform.up + -transform.right * 0.14f, -transform.up);
    groundCheckRays[3] = new Ray(transform.position + transform.up + transform.forward * 0.14f, -transform.up);
    groundCheckRays[4] = new Ray(transform.position + transform.up + -transform.forward * 0.14f, -transform.up);

    foreach(Ray ray in groundCheckRays)
    {
      float castLength = 0;
      if(VSpeed == 0)
      {
        castLength = StepHeight + 1;
      }
      else
      {
        if(VSpeed < 0)
        {
          castLength = -VSpeed * Time.deltaTime + 1;
        }
      }
      int layerMask = LayerMask.GetMask("Default");
      if(Physics.Raycast(ray, out RaycastHit hit, castLength, layerMask))
      {
        if(Vector3.Dot(Vector3.up, hit.normal) >= GroundDotValue)
        {
          OnGround = true;
          SnapToGround();

          if(Vector3.Dot(Vector3.up, hit.normal) >= StandableGroundDotValue)
          {
            Sliding = false;
          }
          else
          {
            Sliding = true;
          }
        }
      }
      Debug.DrawLine(ray.origin, ray.origin + ray.direction, Color.blue);
    }
    AttemptJump();
    if(JumpLaunching)
    {
      OnGround = false;
    }
  }

  void AttemptJump()
  {
    if(IL.GetBottomButtonDown() && OnGround && !Diving)
    {
      JumpLaunching = true;
    }
  }

  void AttemptDive()
  {
    if(IL.GetLeftButtonDown() && !Diving)
    {
      Diving = true;
      HSpeed.z += DiveForwardStrength;
      if(OnGround)
      {
        VSpeed += InitialJumpStrength;
        OnGround = false;
        Diving = true;
      }
      else
      {
        VSpeed -= DiveDownwardStrength;
      }
    }
    else if((IL.GetLeftButtonDown() || IL.GetBottomButtonDown()) && Diving && OnGround)
    {
      VSpeed += InitialJumpStrength;
      OnGround = false;
      Diving = false;
    }
  }

  void UpdateGraphicals()
  {
    Transform graphicals = transform.Find("Graphicals").transform;
    if(Diving)
    {
      graphicals.localPosition = new Vector3(0, 0, -0.4f);
      graphicals.localEulerAngles = new Vector3(90, 0, 0);
    }
    else
    {
      graphicals.localPosition = Vector3.zero;
      graphicals.localEulerAngles = Vector3.zero;
    }
  }

  void Move()
  {
    if(HSpeed.magnitude > 0)
    {
      VelocityFacer.rotation = Quaternion.LookRotation(transform.TransformVector(HSpeed));
    }
    Vector3 rayDirection = VelocityFacer.forward;

    Debug.DrawRay(transform.position, rayDirection, Color.green);
    Ray[] movementCheckRays = new Ray[9];
    List<RaycastHit> wallHitInfos = new List<RaycastHit>();

    //Ray definitions
    //TO-DO: Currently we are casting in the forward direction, but we ultimately should cast in our HSpeed direction
    //Also, the source position of each ray should be oriented around the HSpeed direction as well
    movementCheckRays[0] = new Ray(transform.position + VelocityFacer.TransformVector(Vector3.up * StepHeight + Vector3.forward * 0.15f + Vector3.forward * -0.05f), rayDirection);
    movementCheckRays[1] = new Ray(transform.position + VelocityFacer.TransformVector(Vector3.up * StepHeight + -Vector3.right * 0.1f + Vector3.forward * -0.05f), rayDirection);
    movementCheckRays[2] = new Ray(transform.position + VelocityFacer.TransformVector(Vector3.up * StepHeight + Vector3.right *  0.1f + Vector3.forward * -0.05f), rayDirection);

    movementCheckRays[3] = new Ray(transform.position + VelocityFacer.TransformVector(Vector3.forward * 0.15f + Vector3.up * 0.5f + Vector3.forward * -0.05f), rayDirection);
    movementCheckRays[4] = new Ray(transform.position + VelocityFacer.TransformVector(-Vector3.right * 0.1f + Vector3.up * 0.5f + Vector3.forward * -0.05f), rayDirection);
    movementCheckRays[5] = new Ray(transform.position + VelocityFacer.TransformVector(Vector3.right *  0.1f + Vector3.up * 0.5f + Vector3.forward * -0.05f), rayDirection);

    movementCheckRays[6] = new Ray(transform.position + VelocityFacer.TransformVector(Vector3.forward * 0.15f + Vector3.up + Vector3.forward * -0.05f), rayDirection);
    movementCheckRays[7] = new Ray(transform.position + VelocityFacer.TransformVector(-Vector3.right * 0.1f + Vector3.up + Vector3.forward * -0.05f), rayDirection);
    movementCheckRays[8] = new Ray(transform.position + VelocityFacer.TransformVector(Vector3.right *  0.1f + Vector3.up + Vector3.forward * -0.05f), rayDirection);

    //Physics raycast and debug draw each ray
    for(int i = 0; i < 9; ++i)
    {
      Ray ray = movementCheckRays[i];
      RaycastHit hit;
      int layerMask = LayerMask.GetMask("Default");      
      if(Physics.Raycast(ray, out hit, HSpeed.magnitude * Time.deltaTime + 0.05f, layerMask))
      {
        if(Vector3.Dot(Vector3.up, hit.normal) >= GroundDotValue)
        {
          continue;
        }
        wallHitInfos.Add(hit);
      }
      Debug.DrawRay(ray.origin, ray.direction, Color.blue);
    }

    float smallestDistance = float.MaxValue;
    RaycastHit nearestHit = new RaycastHit();
    foreach(RaycastHit hit in wallHitInfos)
    {
      if(hit.distance < smallestDistance)
      {
        smallestDistance = hit.distance;
        nearestHit = hit;
      }
    }

    Vector3 hSpeed = HSpeed;
    //Scale HSpeed based on the angle of the wall you are hitting
    if(wallHitInfos.Count > 0)
    {
      //Doug is skeptical about this, no one knows why
      hSpeed = hSpeed * (1 - Vector3.Dot(nearestHit.normal, -transform.forward));
    }

    if(OnGround)
    {
      Vector3 deFactoSlope = Vector3.Cross(Vector3.Cross(GetGroundNormal(), transform.forward), GetGroundNormal());
      if(Vector3.Dot(GetGroundNormal(), transform.forward) < 0)
      {
        hSpeed *= Vector3.Dot(deFactoSlope, transform.forward);
      }
    }

    Vector3 previousPosition = transform.position;
    //Move Horizontal
    hSpeed = transform.TransformVector(hSpeed);
    transform.position += hSpeed * Time.deltaTime;
    if(OnGround)
    {
      SnapToGround();
      if(CeilingCheck())
      {
        transform.position = previousPosition;
        HSpeed = Vector3.zero;
      }
    }

    //Eject from wall collisions
    if(wallHitInfos.Count > 0)
    {
      
      Physics.ComputePenetration( MovementCollider,
                                  transform.position,
                                  transform.rotation,
                                  nearestHit.collider,
                                  nearestHit.transform.position,
                                  nearestHit.transform.rotation,
                                  out Vector3 direction,
                                  out float distance);
      transform.position += direction * distance;
      Vector3 newPosition = transform.position;
      Vector3 diffVector = (previousPosition - newPosition);
      diffVector.y = 0;
      float diffVectorDistance = diffVector.magnitude;
      float adjustedHSpeedMagnitude = diffVectorDistance / Time.deltaTime;
      float hSpeedMagnitude = HSpeed.magnitude;
      HSpeed = HSpeed.normalized * Mathf.Min(hSpeedMagnitude, adjustedHSpeedMagnitude);
    }

    //Move Vertical
    transform.position += Vector3.up * VSpeed * Time.deltaTime;

    PreviousOnGround = OnGround;
  }

  //Returns Vector3.zero if it's fucked up
  Vector3 GetGroundNormal()
  {
    Vector3 groundNormal = new Vector3();
    int layerMask = LayerMask.GetMask("Default");
    Ray ray = new Ray(transform.position + transform.up, -transform.up);
    if(Physics.Raycast(ray, out RaycastHit hit, 2, layerMask))
    {
      groundNormal = hit.normal;
    }
    return groundNormal;
  }

  void SnapToGround()
  {
    if(JumpLaunching)
    {
      return;
    }
    int layerMask = LayerMask.GetMask("Default");
    Ray ray = new Ray(transform.position + transform.up, -transform.up);
    //The "2" here is kind of a magic number
    if(Physics.Raycast(ray, out RaycastHit hit, 2, layerMask))
    {
      transform.position = hit.point;
    }
  }
}
