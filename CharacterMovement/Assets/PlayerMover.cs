using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMover : MonoBehaviour
{
  public float GravityAcceleration = -20.0f;
  public float TurnSpeed = 900.0f;
  public float GroundAcceleration = 12.0f;
  public float GroundDeceleration = 24.0f;
  public float BaseMaxGroundSpeed = 6.0f;
  public float JumpStrengthInitial = 50.0f;
  public float JumpStrengthAdditive = 10.0f;
  public float AirControl = 9.0f;
  public float JumpTime = 0.1f;
  public float JumpHeightScalar = 0.1f;
  public float StepHeight = 0.5f;
  public GameObject Graphicals;

  private Vector3 RequestedMoveDirection = new Vector3();
  private bool Grounded = false;
  private List<ContactPoint> Contacts = new List<ContactPoint>();
  private float HSpeed = 0.0f;
  private float VSpeed = 0.0f;
  private bool Jumping = false;
  private float JumpTimer = 0.0f;
  private Vector3 PositionPreviousFrame = new Vector3();

  private Gamepad Gamepad1 = new Gamepad();

  void Start()
  {
    this.Gamepad1 = Gamepad.current;
  }

  void Update()
  {
    if(this.Gamepad1 == null)
    {
      print("No gamepad found.");
      return;
    }
    Vector2 v2LeftStick = this.Gamepad1.leftStick.ReadValue();
    HandleJumping(this.Gamepad1);
    UpdateRequestedMoveDirection(v2LeftStick);
    UpdateRotation(v2LeftStick);
    UpdateSpeed(v2LeftStick);
    Move();
    this.PositionPreviousFrame = transform.position;
  }

  void HandleJumping(Gamepad gamepad)
  {
    if(gamepad.buttonSouth.ReadValue() == 1)
    {
      AttemptJump();
    }
    if(this.Jumping)
    {
      this.JumpTimer += Time.deltaTime;
      if(this.JumpTimer >= this.JumpTime)
      {
        this.Jumping = false;
        this.JumpTimer = 0.0f;
      }
    }
  }

  void UpdateRequestedMoveDirection(Vector2 leftStick)
  {
    //This function takes the raw left stick input and puts it into camera space
    //There is no Y component to this 3D vector. The left stick cannot influence Y position directly.
    var camera = Camera.main;
    Vector3 v3LeftStick = new Vector3(leftStick.x, 0, leftStick.y);
    Vector3 v3RequestedWorldDirection = camera.transform.TransformDirection(v3LeftStick);
    v3RequestedWorldDirection.y = 0;
    v3RequestedWorldDirection.Normalize();
    this.RequestedMoveDirection = v3RequestedWorldDirection;
  }

  void UpdateRotation(Vector2 leftStick)
  {
    if(this.RequestedMoveDirection == Vector3.zero || !this.Grounded)
    {
      return;
    }
    Vector3 lookTarget = this.RequestedMoveDirection;
    Quaternion targetRotation = Quaternion.LookRotation(lookTarget);
    var step = this.TurnSpeed * Time.deltaTime;
    transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, step);
  }

  void SnapToGroundNormal(Vector3 groundNormal)
  {
    Vector3 newForward = Vector3.Cross(transform.right, groundNormal);
    this.Graphicals.transform.rotation = Quaternion.LookRotation(newForward, groundNormal);
  }

  void UpdateSpeed(Vector2 leftStick)
  {
    if(!this.Grounded)
    {
      this.VSpeed += this.GravityAcceleration * Time.deltaTime;
      return;
    }
    if(leftStick.magnitude > 0)
    {
      if(this.HSpeed < this.BaseMaxGroundSpeed)
      {
        this.HSpeed += this.GroundAcceleration * Time.deltaTime;
      }
    }
    else
    {
      if(this.HSpeed > 0)
      {
        if(this.HSpeed < this.GroundDeceleration * Time.deltaTime)
        {
          this.HSpeed = 0;
        }
        else
        {
          this.HSpeed += -this.GroundDeceleration * Time.deltaTime;
        }
      }
      else
      {
        if(this.HSpeed > -this.GroundDeceleration * Time.deltaTime)
        {
          this.HSpeed = 0;
        }
        else
        {
          this.HSpeed += this.GroundDeceleration * Time.deltaTime;
        }
      }
    }
  }

  void Move()
  {
    Rigidbody rigidbody = GetComponent<Rigidbody>();
    Vector3 Velocity = rigidbody.velocity;
    if(this.Grounded)
    {
      Velocity = this.Graphicals.transform.forward * this.HSpeed;
      Velocity.y += this.VSpeed;
      Debug.DrawRay(transform.position, this.Graphicals.transform.forward * this.HSpeed);
    }
    else
    {
      Vector3 newHVelocity = new Vector3(Velocity.x, 0, Velocity.z) + this.RequestedMoveDirection * this.AirControl * Time.deltaTime;
      if(newHVelocity.magnitude < this.BaseMaxGroundSpeed)
      {
        Velocity += this.RequestedMoveDirection * this.AirControl * Time.deltaTime;
      }
      Velocity.y = this.VSpeed;
    }
    
    rigidbody.velocity = Velocity;
    //HandleLedgeStep();
  }
  
  void HandleLedgeStep()
  {
    if(this.Jumping)
    {
      return;
    }
    RaycastHit hitInfo;
    if(GetComponent<Rigidbody>().SweepTest(Vector3.down, out hitInfo, this.StepHeight))
    {
      //transform.position += Vector3.down * hitInfo.distance;
    }
  }

  private void OnCollisionExit(Collision collision)
  {
    if(collision.contactCount == 0)
    {
      if(this.Grounded)
      {
        this.Grounded = false;
      }
    }
  }

  private void OnCollisionStay(Collision collision)
  {
    Vector3 lowestContactPoint = new Vector3(0, 0.6f, 0);
    Vector3 lowestContactPointNormal = new Vector3();
    bool feetOnGround = false;
    List<ContactPoint> eventContacts = new List<ContactPoint>();
    collision.GetContacts(eventContacts);

    foreach(ContactPoint contact in collision.contacts)
    {
      Vector3 localContactPoint = transform.InverseTransformPoint(contact.point);
      if(localContactPoint.y < 0.3f)
      {
        if(localContactPoint.y < lowestContactPoint.y)
        {
          lowestContactPoint = localContactPoint;
          lowestContactPointNormal = contact.normal;
        }
        feetOnGround = true;
      }
    }
    if(feetOnGround)
    {
      SnapToGroundNormal(lowestContactPointNormal);
      if(!this.Grounded)
      {
        this.VSpeed = 0;
        this.Grounded = true;
        if(this.Gamepad1.leftStick.ReadValue().magnitude == 0)
        {
          this.HSpeed = 0;
        }
        else
        {
          this.HSpeed = Vector3.Dot(GetComponent<Rigidbody>().velocity, transform.forward);
        }
      }
    }
  }

  void AttemptJump()
  {
    if(this.Grounded || this.Jumping)
    {
      if(!this.Jumping)
      {
        this.VSpeed += this.JumpStrengthInitial + this.HSpeed * this.JumpHeightScalar;
        this.Jumping = true;
      }
      this.VSpeed += this.JumpStrengthAdditive * Time.deltaTime;
    }
  }
}