using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pixelplacement;

[RequireComponent(typeof(InputListener))]

public class PlayerMover : MonoBehaviour
{
  //Public Variables
  public float MaxTurnSpeed = 8.0f;                               //This is our turn speed when our HSpeed crosses the InstantTurnThreshold
  public float MinTurnSpeed = 3.0f;                               //This is our turn speed when our HSpeed reaches MaxRunSpeed
  public float Acceleration = 5.0f;                               //
  public float MaxRunSpeed = 4.0f;                                //
  public float Gravity = 10.0f;                                   //
  public float StandableGroundAngle = 50.0f;                      //
  public float GroundAngle = 89.0f;                               //
  public float CeilingAngle = 50.0f;                              //
  public Collider MovementCollider;                               //
  public float StepHeight = 0.2f;                                 //
  public float InitialJumpStrength = 5.0f;                        //
  public float AdditiveJumpStrength = 1.0f;                       //
  public float JumpLaunchTime = 5f/60f;                           //
  public Transform VelocityFacer;                                 //
  public float MinimumJumpLaunchTime = 5/60;                      //
  public float JumpStrengthSpeedScalar = 1.8f;                    //
  public float DiveForwardStrength = 2.0f;                        //
  public float DiveDownwardStrength = 1.0f;                       //
  public float SteepSlopeAccelerationInfluence = 5.0f;            //
  public float SteepSlopeTurnSpeed = 1.0f;                        //
  public HoldHandle m_PlayerUpdateHoldHandle;                     //
  public HoldHandle m_PlayerInputHoldHandle;                      //
  public float WallKickTime = 0.13f;                              //Amount of time the player has to press A to wallkick after bonking in midair
  public float BonkTime = 1.5f;                                   //Amount of time it takes to recover from a bonk after touching the ground
  public float BonkSpeed = 2.0f;                                  //The speed the player moves backwards after bonking
  public float GroundBonkSpeedThreshold = 6.0f;                   //Speed required to bonk when on the ground
  public float AirBonkSpeedThreshold = 3.0f;                      //Speed required to bonk when in the air
  public float HardBonkThreshold = 8.0f;                          //Speed at which a bonk will be forced to be a hard bonk
  public float InstantTurnThreshold = 1.0f;                       //If the player is moving slower than this speed, their turn speed is instantaneous
  public float TurnKickSpeedThreshold = 3.0f;                     //Speed required for a turn kick to be possible
  public float TurnKickDeceleration = 25.0f;                      //Deceleration rate after a turn kick begins

  //SFX References
  public AudioClip JumpSound;
  public AudioClip BonkSound;
  public AudioClip WallImpactSound;
  public AudioClip WallKickSound;
  public AudioClip DiveSound;
  public AudioClip RolloutSound;

  //Components                                                    //
  private InputListener IL;                                       //

  //Private variables                                             //
  private GameObject GameCamera;                                  //
  private Vector3 HSpeed = new Vector3();                         //
  private float VSpeed = 0.0f;                                    //
  private bool OnGround = true;                                   //
  private bool Sliding = true;                                    //
  private bool Diving = false;                                    //
  private bool PreviousOnGround = true;                           //
  private float Deceleration;                                     //
  private ContactPoint[] Contacts = new ContactPoint[0];          //
  private float StandableGroundDotValue;                          //
  private float GroundDotValue;                                   //
  private float CeilingDotValue;                                  //
  private Vector3 MovementDirection;                              //
  private float JumpLaunchStopwatch = 0.0f;                       //
  private bool JumpLaunching = false;                             //
  private float JiggyJigTimer = 0.0f;                             //
  private bool WaitingToJig = false;                              //
  private GameObject DeathPlane;                                  //
  private Vector3 LastKnownSafePosition;                          //
  private Vector3 LastKnownGoodCameraPosition;                    //
  private Quaternion LastKnownGoodCameraRotation;                 //
  private bool Bonking = false;                                   //Flag for whether or not we are in a bonk state. Is set to true as soon as a wall collision begins
  private bool WallKickWindow = false;                            //Is true during the first portion of an aerial bonk where a wallkick is possible.
  private bool HardBonk;                                          //Variable for storing whether or not the next bonk will be a hard bonk
  private Vector3 BonkNormal;                                     //Variable for storing the normal of the wall we bonked on
  private Vector3 BonkVelocity;                                   //Variable for storing the velocity we had when we hit the wall
  private float WallKickStopWatch = 0.0f;                         //Stop watch for timing the window after a wall collision but before a bonk
  private float BonkStopWatch = 0.0f;                             //Stop watch for timing the bonk
  private Ray[] MovementCheckRays = new Ray[9];                   //Array of rays used to check for wall collision in our forward direction
  private float HMovementRaySideDepth = 0.05f;                    //Distance sidemost rays are inset from the edge of the cylinder collider
  private float ColliderRadius = 0.15f;                           //Radius of the cylinder collider
  private RaycastHit NearestHit;                                  //Variable used to store our nearest collider hit from our HMovement rays
  private List<RaycastHit> WallHitInfos;                          //List used to store all our raycast hits from HMovement rays
  private bool TurnKicking = false;                               //Bool for controlling whether or not we are in a turn kicking state

  void Start()
  {
    IL = GetComponent<InputListener>();
    Deceleration = Acceleration * 3;
    StandableGroundDotValue = Mathf.Cos(Mathf.Deg2Rad * StandableGroundAngle);
    GroundDotValue = Mathf.Cos(Mathf.Deg2Rad * GroundAngle);
    CeilingDotValue = Mathf.Cos(Mathf.Deg2Rad * CeilingAngle);
    FindGameCamera();
    DeathPlane = GameObject.Find("DeathPlane");
  }

  void Update()
  {
    if (m_PlayerUpdateHoldHandle.HasHolds) return;

    Turn();
    HSpeedUpdate();
    VSpeedUpdate();
    Move();
    UpdateTimers();
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
        if(Physics.Raycast(ray, out RaycastHit hit, 2, layerMask, QueryTriggerInteraction.Ignore))
        {
          Vector3 slopeDownDirection= hit.normal;
          slopeDownDirection.y = 0;
          slopeDownDirection.Normalize();

          //Put the left stick input into camera space
          Vector3 stickDirection = GameCamera.transform.TransformDirection(new Vector3(IL.GetLeftStickVector().x, 0, IL.GetLeftStickVector().y));
          //We never want the character to look up or down, so 0 this value out
          stickDirection.y = 0;
          //Normalize the vector for good measure since we modified the Y value, though this probably doesn't matter
          stickDirection.Normalize();

          stickDirection = transform.InverseTransformDirection(stickDirection);

          Vector3 stickLateralDirection = new Vector3(stickDirection.x, 0, 0);

          stickLateralDirection = transform.TransformDirection(stickLateralDirection);
          
          Vector3 targetLookDirection = slopeDownDirection;

          //If this is true, we are facing down a steep slope. If you are facing uphill, you aren't allowed to affect your turn with the left stick
          if(Vector3.Dot(transform.forward, slopeDownDirection) > 0)
          {
            float leftStickXMagnitude = Mathf.Abs(IL.GetLeftStickVector().x);
            targetLookDirection += stickLateralDirection * leftStickXMagnitude;
          }
          Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetLookDirection, SteepSlopeTurnSpeed * Time.deltaTime, 0.0f);
          transform.rotation = Quaternion.LookRotation(newDirection, Vector3.up);
          Debug.DrawLine(transform.position, transform.position + stickLateralDirection * 4);
        }
      }
      else
      {
        if(TurnKicking)
        {
          Vector3 decelerationDirection = -HSpeed.normalized;
          //TO-DO: Set up a property on SurfaceProperties that can modify TurnKickDeceleration to make certain surfaces more grippy or more slippy
          HSpeed += decelerationDirection * TurnKickDeceleration * Time.deltaTime;
          if(Vector3.Dot(HSpeed, decelerationDirection) > 0)
          {
            HSpeed = Vector3.zero;
          }
          if(HSpeed == Vector3.zero)
          {
            transform.rotation = Quaternion.LookRotation(-transform.forward, Vector3.up);
            TurnKicking = false;
            return;
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

          //TurnKick check
          if( Vector3.Dot(targetLookDirection, transform.forward) < -0.80f &&
              HSpeed.magnitude > TurnKickSpeedThreshold &&
              IL.GetLeftStickVector().magnitude > 0.5f)
          {
            TurnKicking = true;
            return;
          }

          //Lerp our turn speed based on our HSpeed
          float turnSpeed = Mathf.Lerp(MaxTurnSpeed, MinTurnSpeed, HSpeed.magnitude/MaxRunSpeed);
          //Rotate towards the target rotation at a set speed
          Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetLookDirection, turnSpeed * Time.deltaTime, 0.0f);

          if(HSpeed.magnitude <= InstantTurnThreshold && IL.GetLeftStickVector().magnitude >= 0.25f)
          {
            transform.rotation = Quaternion.LookRotation(targetLookDirection, Vector3.up);
          }
          else
          {
            transform.rotation = Quaternion.LookRotation(newDirection, Vector3.up);
          }
        }
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
        if(Physics.Raycast(ray, out RaycastHit hit, 2, layerMask, QueryTriggerInteraction.Ignore))
        {
          Vector3 targetSlideDirection = hit.normal;
          targetSlideDirection.y = 0;
          targetSlideDirection.Normalize();

          //Put the left stick input into camera space
          Vector3 leftStickDirection = GameCamera.transform.TransformDirection(new Vector3(IL.GetLeftStickVector().x, 0, IL.GetLeftStickVector().y));
          //We never want the character to look up or down, so 0 this value out
          leftStickDirection.y = 0;
          //Normalize the vector for good measure since we modified the Y value, though this probably doesn't matter
          leftStickDirection.Normalize();
          //Put the vector into local player space
          leftStickDirection = transform.InverseTransformDirection(leftStickDirection);

          float slidingAcceleration;

          slidingAcceleration = Mathf.Lerp(Gravity, 0, (Vector3.Dot(hit.normal, Vector3.up))) + (leftStickDirection.z * SteepSlopeAccelerationInfluence);
          if(slidingAcceleration < 0)
          {
            slidingAcceleration = 0;
          }
          

          HSpeed += (Vector3.forward * slidingAcceleration * Time.deltaTime);
        }
      }
      else if(Diving)
      {
        if(HSpeed.magnitude >= Deceleration * Time.deltaTime)
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
    ceilingCheckRays[0] = new Ray(transform.position + transform.up * 0.5f, transform.up);
    ceilingCheckRays[1] = new Ray(transform.position + transform.up * 0.5f + transform.right * 0.14f, transform.up);
    ceilingCheckRays[2] = new Ray(transform.position + transform.up * 0.5f + -transform.right * 0.14f, transform.up);
    ceilingCheckRays[3] = new Ray(transform.position + transform.up * 0.5f + transform.forward * 0.14f, transform.up);
    ceilingCheckRays[4] = new Ray(transform.position + transform.up * 0.5f + -transform.forward * 0.14f, transform.up);

    foreach(Ray ray in ceilingCheckRays)
    {
      //Debug.DrawRay(ray.origin, ray.direction, Color.blue);

      float castLength = 1 - StepHeight + VSpeed * Time.deltaTime;

      int layerMask = LayerMask.GetMask("Default");
      if (Physics.Raycast(ray, out RaycastHit hit, castLength, layerMask, QueryTriggerInteraction.Ignore))
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
      if(Physics.Raycast(ray, out RaycastHit hit, castLength, layerMask, QueryTriggerInteraction.Ignore))
      {
        if(Vector3.Dot(Vector3.up, hit.normal) >= GroundDotValue)
        {
          OnGround = true;
          if(WaitingToJig)
          {
            BeginJiggyJig();
          }
          SnapToGround();

          float currentStandableGroundDotValue;
          if(hit.collider.gameObject.GetComponent<SurfaceProperties>())
          {
            currentStandableGroundDotValue = Mathf.Cos(Mathf.Deg2Rad * hit.collider.gameObject.GetComponent<SurfaceProperties>().GetStandableGroundAngle());
          }
          else
          {
            currentStandableGroundDotValue = StandableGroundDotValue;
          }
          if(Vector3.Dot(Vector3.up, hit.normal) >= currentStandableGroundDotValue)
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
    if (IL.GetBottomButtonDown())
    {
      if(OnGround && !Diving && !Sliding)
      {
        AudioSource.PlayClipAtPoint(JumpSound, transform.position);
        JumpLaunching = true;
        if(TurnKicking)
        {
          transform.rotation = Quaternion.LookRotation(-transform.forward, Vector3.up);
          TurnKicking = false;
        }
      }
      if(Bonking && !Diving && WallKickWindow)
      {
        WallKick();
      }
    }
  }

  void WallKick()
  {
    AudioSource.PlayClipAtPoint(WallKickSound, transform.position);
    Bonking = false;
    VSpeed = 0;
    float firstieScalar = Mathf.Lerp(1.2f, 0.5f, WallKickStopWatch / WallKickTime);
    HSpeed = BonkVelocity * firstieScalar;
    JumpLaunching = true;
    Vector3 newForward = Vector3.Reflect(transform.forward, BonkNormal);
    newForward.y = 0;
    transform.rotation = Quaternion.LookRotation(newForward, Vector3.up);
  }

  void AttemptDive()
  {
    if(IL.GetLeftButtonDown() && !Diving)
    {
      AudioSource.PlayClipAtPoint(DiveSound, transform.position);
      Diving = true;
      if(OnGround)
      {
        if(TurnKicking)
        {
          transform.rotation = Quaternion.LookRotation(-transform.forward, Vector3.up);
          HSpeed.z = DiveForwardStrength * 2.5f;
          VSpeed += InitialJumpStrength * 1.5f;
          TurnKicking = false;
        }
        else
        {
          HSpeed.z += DiveForwardStrength;
          VSpeed += InitialJumpStrength;
        }
        OnGround = false;
        Diving = true;
      }
      else
      {
        HSpeed.z += DiveForwardStrength;
        VSpeed -= DiveDownwardStrength;
      }
    }
    else if((IL.GetLeftButtonDown() || IL.GetBottomButtonDown()) && Diving && OnGround)
    {
      AudioSource.PlayClipAtPoint(RolloutSound, transform.position);
      VSpeed += InitialJumpStrength;
      OnGround = false;
      Diving = false;
    }
  }

  void UpdateGraphicals()
  {
    Transform graphicals = transform.Find("Graphicals").transform;
    if(Bonking && !WallKickWindow)
    {
      graphicals.localPosition = new Vector3(0, 0, 0f);
      graphicals.localEulerAngles = new Vector3(-35, 0, 0);
    }
    else if(Diving)
    {
      graphicals.localPosition = new Vector3(0, 0, -0.4f);
      graphicals.localEulerAngles = new Vector3(90, 0, 0);
    }
    else if(Sliding)
    {
      graphicals.localPosition = new Vector3(0, -0.3f, 0);
      graphicals.localEulerAngles = Vector3.zero;
    }
    else if(TurnKicking)
    {
      graphicals.localPosition = new Vector3(0, 0, 0);
      graphicals.localEulerAngles = new Vector3(-15, 0, 0);
    }
    else
    {
      graphicals.localPosition = Vector3.zero;
      graphicals.localEulerAngles = Vector3.zero;
    }
  }

  void Move()
  {
    ConstructHMovementRays();
    PhysicsCastHMovementRays();

    //Bonk check
    if(WallHitInfos.Count > 0)
    {
      //Our bonk threshold is higher on the ground than it is in the air
      float bonkSpeedThreshold = OnGround ? GroundBonkSpeedThreshold : AirBonkSpeedThreshold;
      if(HSpeed.magnitude > bonkSpeedThreshold)
      {
        if(Vector3.Dot(transform.forward, NearestHit.normal) <= -0.5f)
        {
          //The bonk will be a hard bonk if they are on the ground, or if they are diving during an air bonk
          bool hardBonk = false;
          if(OnGround)
          {
            hardBonk = true;
          }
          else
          {
            if(Diving || HSpeed.magnitude >= HardBonkThreshold)
            {
              hardBonk = true;
            }
          }
          Bonk(hardBonk, NearestHit.normal, HSpeed);
        }
      }
    }

    //Temporary variable for modification
    Vector3 hSpeed = HSpeed;
    //Scale HSpeed based on the angle of the wall you are hitting
    //If we're hitting any walls at all...
    if(WallHitInfos.Count > 0)
    {
      //Doug is skeptical about this, no one knows why
      //Alek's stamp of approval
      hSpeed = hSpeed * (1 - Vector3.Dot(NearestHit.normal, -transform.forward));
    }

    //Scale our hSpeed based on whether we are facing up or down hill
    if(OnGround)
    {
      Vector3 deFactoSlope = Vector3.Cross(Vector3.Cross(GetGroundNormal(), transform.forward), GetGroundNormal());
      if(Vector3.Dot(GetGroundNormal(), transform.forward) < 0)
      {
        hSpeed *= Vector3.Dot(deFactoSlope, transform.forward);
      }
    }

    //Store out our previous position before moving
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
    if(WallHitInfos.Count > 0)
    {
      int numberOfIterations = 0;
      //Snap to ground repeatedly until EjectFromWalls() does not move the player
      while(EjectFromWalls().sqrMagnitude > 0)
      {
        SnapToGround();
        ++numberOfIterations;
        if(numberOfIterations > 100)
        {
          //This break is a failsafe, though hitting it means we probably have an issue. We hit it from time to time :)
          break;
        }
      }

      //Adjusting our HSpeed for the frame we collided with the wall to reflect the speed it took for us to reach contact with the wall
      Vector3 newPosition = transform.position;
      Vector3 diffVector = (previousPosition - newPosition);
      diffVector.y = 0;
      float diffVectorDistance = diffVector.magnitude;
      float adjustedHSpeedMagnitude = diffVectorDistance / Time.deltaTime;
      float hSpeedMagnitude = HSpeed.magnitude;
      HSpeed = HSpeed.normalized * Mathf.Min(hSpeedMagnitude, adjustedHSpeedMagnitude);
      //Bounce off the walls if we are sliding
      if (Sliding)
      {
        Vector3 newForward = Vector3.Reflect(transform.forward, NearestHit.normal);
        newForward.y = 0;
        newForward.Normalize();
        transform.rotation = Quaternion.LookRotation(newForward, Vector3.up);
      }
    }

    //Move Vertical
    transform.position += Vector3.up * VSpeed * Time.deltaTime;

    //Deathplane check
    if(DeathPlane)
    {
      if(transform.position.y < DeathPlane.transform.position.y)
      {
        RespawnAtLastKnownSafePosition();
      }
    }
    PreviousOnGround = OnGround;
  }

  void ConstructHMovementRays()
  {
    //Orient our velocity facer if the player is moving. The velocity facer is used to make sure our movement check rays are pointed the right direction
    if(HSpeed.magnitude > 0)
    {
      VelocityFacer.rotation = Quaternion.LookRotation(transform.TransformVector(HSpeed));
    }
    Vector3 rayDirection = VelocityFacer.forward;
    //Draw the velocity facer's forward in green
    Debug.DrawRay(transform.position, rayDirection, Color.green);   

    //Ray definitions
    //TO-DO: Currently we are casting in the forward direction, but we ultimately should cast in our HSpeed direction
    //Also, the source position of each ray should be oriented around the HSpeed direction as well
    //Bottom middle
    MovementCheckRays[0] = new Ray(transform.position + VelocityFacer.TransformVector(Vector3.up * StepHeight), rayDirection);
    //Bottom left
    MovementCheckRays[1] = new Ray(transform.position + VelocityFacer.TransformVector(Vector3.up * StepHeight + -Vector3.right * (ColliderRadius - HMovementRaySideDepth)), rayDirection);
    //Bottom right
    MovementCheckRays[2] = new Ray(transform.position + VelocityFacer.TransformVector(Vector3.up * StepHeight + Vector3.right *  (ColliderRadius - HMovementRaySideDepth)), rayDirection);

    //Middle middle
    MovementCheckRays[3] = new Ray(transform.position + VelocityFacer.TransformVector(Vector3.up * 0.5f), rayDirection);
    //Middle left
    MovementCheckRays[4] = new Ray(transform.position + VelocityFacer.TransformVector(Vector3.up * 0.5f + -Vector3.right * (ColliderRadius - HMovementRaySideDepth)), rayDirection);
    //Middle right
    MovementCheckRays[5] = new Ray(transform.position + VelocityFacer.TransformVector(Vector3.up * 0.5f + Vector3.right *  (ColliderRadius - HMovementRaySideDepth)), rayDirection);

    //Top Middle
    MovementCheckRays[6] = new Ray(transform.position + VelocityFacer.TransformVector(Vector3.up), rayDirection);
    //Top left
    MovementCheckRays[7] = new Ray(transform.position + VelocityFacer.TransformVector(Vector3.up + -Vector3.right * (ColliderRadius - HMovementRaySideDepth)), rayDirection);
    //Top right
    MovementCheckRays[8] = new Ray(transform.position + VelocityFacer.TransformVector(Vector3.up + Vector3.right *  (ColliderRadius - HMovementRaySideDepth)), rayDirection);
  }

  void PhysicsCastHMovementRays()
  {
    WallHitInfos = new List<RaycastHit>();
    //The side rays inset from the player's collider slightly, so we need to modify their distance based on where the ray EXITS the player's collider
    float sideCastDistanceModifier = Mathf.Sqrt(ColliderRadius * ColliderRadius - (ColliderRadius - HMovementRaySideDepth) * (ColliderRadius - HMovementRaySideDepth));

    //Physics raycast and debug draw each ray
    for(int i = 0; i < 9; ++i)
    {
      //Cast distance will be a little extra on our top center, middle center, and bottom center rays to simulate the shape of our cylinder collider
      float castDistance = ((i % 3 == 0) ? ColliderRadius : sideCastDistanceModifier) + HSpeed.magnitude * Time.deltaTime;
      Ray ray = MovementCheckRays[i];
      RaycastHit hit;
      int layerMask = LayerMask.GetMask("Default");

      //Raycast the current indexed ray
      if(Physics.Raycast(ray, out hit, castDistance, layerMask, QueryTriggerInteraction.Ignore))
      {
        //If the thing we hit is ground, don't worry about it
        if(Vector3.Dot(Vector3.up, hit.normal) >= GroundDotValue)
        {
          continue;
        }
        //If the thing we hit is ceiling, don't worry about it
        else if(Vector3.Dot(Vector3.down, hit.normal) >= CeilingDotValue)
        {
          continue;
        }
        //The thing we hit is definitely a wall, so add the hit info to our list
        WallHitInfos.Add(hit);
      }
      Debug.DrawLine(ray.origin, ray.direction * castDistance + ray.origin, Color.blue);
    }
    
    //Find which hit result is nearest
    float smallestDistance = float.MaxValue;
    foreach(RaycastHit hit in WallHitInfos)
    {
      if(hit.distance < smallestDistance)
      {
        smallestDistance = hit.distance;
        NearestHit = hit;
      }
    }
  }

  //Returns ejection vector
  Vector3 EjectFromWalls()
  {
    Vector3 previousPosition = transform.position;
    Physics.ComputePenetration( MovementCollider,
                                  transform.position,
                                  transform.rotation,
                                  NearestHit.collider,
                                  NearestHit.transform.position,
                                  NearestHit.transform.rotation,
                                  out Vector3 direction,
                                  out float distance);
    transform.position += direction * distance;

    return transform.position - previousPosition;
  }

  void UpdateTimers()
  {
    //Bonk timer logic
    if(Bonking)
    {
      //If we're on the ground, there is no opportunity for a wall jump. An input hold should already be applied from the Bonk() function
      if(OnGround)
      {
        Diving = false;
        BonkStopWatch += Time.deltaTime;
        //Bonk is over when the bonk stop watch finishes or if we are not hard bonking
        if(BonkStopWatch >= BonkTime || !HardBonk)
        {
          Bonking = false;
          m_PlayerInputHoldHandle.Release(this);
        }
        else
        {
          //If our bonk timer isn't expired yet but we are on the ground, we slowly decelerate to a stop via lerping HSpeed over the course of our bonk duration
          float bonkSpeed = Mathf.Lerp(BonkSpeed, 0, BonkStopWatch / BonkTime);
          HSpeed = -Vector3.forward * bonkSpeed;
        }
      }
      //If we bonk in the air, we have to do some wall kick logic
      else
      {
        //The bonk stop watch should always be reset to 0 if you are in midair. This will allow the player to bonk and fall down a flight of stairs, for instance
        BonkStopWatch = 0.0f;
        //Check if we are still in our wall kick window. WallKickStopWatch is set to 0 in the Bonk() function. There is no wall kick window if the player is diving.
        if(WallKickStopWatch < WallKickTime && !Diving)
        {
          //Set the WallKickWindow flag and increment the WallKickStopWatch. VSpeed is set to 0 during this time so you stick to the wall for a moment before bonking
          WallKickWindow = true;
          WallKickStopWatch += Time.deltaTime;
          VSpeed = 0;
        }
        else
        {
          //Is this the first frame of our wall kick window expiring?
          if(WallKickWindow)
          {
            AudioSource.PlayClipAtPoint(BonkSound, transform.position);
            //Toggle the flag and add a hold
            WallKickWindow = false;
            m_PlayerInputHoldHandle.Add(this);
          }
          //Player moves backwards at a fixed speed when bonking in the air
          HSpeed = -Vector3.forward * BonkSpeed;
        }
      }
    }
  }

  //Returns Vector3.zero if it's fucked up
  Vector3 GetGroundNormal()
  {
    Vector3 groundNormal = new Vector3();
    int layerMask = LayerMask.GetMask("Default");
    Ray ray = new Ray(transform.position + transform.up, -transform.up);
    if(Physics.Raycast(ray, out RaycastHit hit, 2, layerMask, QueryTriggerInteraction.Ignore))
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
    float castDistance = (OnGround) ? 2 : VSpeed * Time.deltaTime;
    if(Physics.Raycast(ray, out RaycastHit hit, castDistance, layerMask, QueryTriggerInteraction.Ignore))
    {
      transform.position = hit.point;
      if(Sliding == false)
      {
        UpdateLastKnownSafePosition();
      }
    }
  }

  private void OnTriggerEnter(Collider other)
  {
    if(other.gameObject.GetComponent<StarScript>())
    {
      StarCollision();
    }
  }

  void StarCollision()
  {
    WaitingToJig = true;
    HSpeed = Vector3.zero;
    m_PlayerInputHoldHandle.Add(this);
    //timer
  }

  void BeginJiggyJig()
  {
    WaitingToJig = false;
    float JiggyJigTime = 2.0f;
    Tween.Value(0, 0, (x)=>JiggyJigTimer = x, JiggyJigTime, 0.0f, completeCallback:OnJiggyJigComplete);
  }

  void OnJiggyJigComplete()
  {
    m_PlayerInputHoldHandle.Release(this);
  }

  void KillHSpeed()
  {
    HSpeed = Vector3.zero;
  }

  void KillVSpeed()
  {
    VSpeed = 0;
  }

  public bool GetOnGround()
  {
    return OnGround;
  }

  void RespawnAtLastKnownSafePosition()
  {
    KillHSpeed();
    KillVSpeed();
    transform.position = LastKnownSafePosition;
    Diving = false;
    OnGround = true;
    GameCamera.transform.position = LastKnownGoodCameraPosition;
    GameCamera.transform.rotation = LastKnownGoodCameraRotation;
  }

  void UpdateLastKnownSafePosition()
  {
    LastKnownSafePosition = transform.position;
    LastKnownGoodCameraPosition = GameCamera.transform.position;
    LastKnownGoodCameraRotation = GameCamera.transform.rotation;
  }

  void Bonk(bool hardBonk, Vector3 bonkNormal, Vector3 bonkVelocity)
  {
    if(OnGround)
    {
      AudioSource.PlayClipAtPoint(BonkSound, transform.position);
      m_PlayerInputHoldHandle.Add(this);
    }
    else
    {
      WallKickWindow = true;
      WallKickStopWatch = 0.0f;
    }
    AudioSource.PlayClipAtPoint(WallImpactSound, transform.position);
    HardBonk = hardBonk;
    Bonking = true;
    BonkStopWatch = 0.0f;
    BonkNormal = bonkNormal;
    BonkVelocity = bonkVelocity;
  }

  public void OnLeftStickSpin()
  {
    print("SPEEN");
    if(!OnGround && !Bonking && !Diving)
    {
      HSpeed *= 0.15f;
      VSpeed *= 0.15f;
    }
  }
}
