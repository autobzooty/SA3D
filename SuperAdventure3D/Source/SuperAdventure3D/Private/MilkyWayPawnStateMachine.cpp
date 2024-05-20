// Fill out your copyright notice in the Description page of Project Settings.


#include "MilkyWayPawnStateMachine.h"
#include "MilkyWayPawnState.h"
#include "MilkyWayPawn.h"
#include "Kismet/KismetMathLibrary.h"

UMilkyWayPawnStateMachine::UMilkyWayPawnStateMachine(const FObjectInitializer& init)
{
	PrimaryComponentTick.bCanEverTick = true;
}

// Sets default values for this component's properties
UMilkyWayPawnStateMachine::UMilkyWayPawnStateMachine()
{
	// Set this component to be initialized when the game starts, and to be ticked every frame.  You can turn these features
	// off to improve performance if you don't need them.
	PrimaryComponentTick.bCanEverTick = true;
}

UMilkyWayPawnStateMachine::~UMilkyWayPawnStateMachine()
{
	delete Idle;
}

void UMilkyWayPawnStateMachine::Setup(AMilkyWayPawn* owner)
{
	Owner = owner;

	Idle = new State_Idle(Owner);
	Walk = new State_Walk(Owner);
	Stop = new State_Stop(Owner);
	Jump = new State_Jump(Owner);
	Fall = new State_Fall(Owner);
	TurnKick = new State_TurnKick(Owner);
	Dive = new State_Dive(Owner);
	Rollout = new State_Rollout(Owner);
	Bonk = new State_Bonk(Owner);
	WallKick = new State_WallKick(Owner);
	SideFlip = new State_SideFlip(Owner);
	Push = new State_Push(Owner);
}


// Called when the game starts
void UMilkyWayPawnStateMachine::BeginPlay()
{
	Super::BeginPlay();
	CurrentState = Idle;
	CurrentState->OnStateEnter();
}

void UMilkyWayPawnStateMachine::GetCurrentState()
{

}


// Called every frame
void UMilkyWayPawnStateMachine::TickComponent(float DeltaTime, ELevelTick TickType, FActorComponentTickFunction* ThisTickFunction)
{
	Super::TickComponent(DeltaTime, TickType, ThisTickFunction);

	CurrentState->StateTick();
	if (BonkedThisFrame)
	{
		RequestStateChange("Bonk");
		BonkedThisFrame = false;
	}
	if (RequestedState != "")
	{
		ChangeState();
	}
}

void UMilkyWayPawnStateMachine::RequestStateChange(FName newStateName)
{
	RequestedState = newStateName;
}

void UMilkyWayPawnStateMachine::ChangeState()
{
	if (PrintStateChanges)
	{
		if (GEngine)
			GEngine->AddOnScreenDebugMessage(-1, 15.0f, FColor::Yellow, RequestedState.ToString());
	}

	if (RequestedState == CurrentState->StateName)
		return;

	MilkyWayPawnState* newState = Idle;

	if (RequestedState == "Walk")
		newState = Walk;
	else if (RequestedState == "Stop")
		newState = Stop;
	else if (RequestedState == "Jump")
		newState = Jump;
	else if (RequestedState == "Fall")
		newState = Fall;
	else if (RequestedState == "TurnKick")
		newState = TurnKick;
	else if (RequestedState == "Dive")
		newState = Dive;
	else if (RequestedState == "Rollout")
		newState = Rollout;
	else if (RequestedState == "Bonk")
		newState = Bonk;
	else if (RequestedState == "WallKick")
		newState = WallKick;
	else if (RequestedState == "SideFlip")
		newState = SideFlip;
	else if (RequestedState == "Push")
		newState = Push;

	CurrentState->OnStateExit();
	newState->OnStateEnter();
	CurrentState = newState;
	RequestedState = "";
}

#pragma region Idle
State_Idle::State_Idle(AMilkyWayPawn* owner)
	:MilkyWayPawnState(owner)
{
	
}

void State_Idle::OnStateEnter()
{
	Owner->HSpeed = 0;
}

void State_Idle::StateTick()
{
	Owner->GroundCheck();
	if (!Owner->OnGround)
	{
		StateMachine->RequestStateChange("Fall");
		return;
	}
	if (Owner->JumpButtonPressedThisFrame)
	{
		StateMachine->RequestStateChange("Jump");
		return;
	}
	if (Owner->CurrentLeftStick.Length() > 0)
	{
		//Snap rotation to camera's requested move direction
		FRotator rotation = UKismetMathLibrary::FindLookAtRotation(Owner->GetActorLocation(), Owner->GetActorLocation() + Owner->GetCameraRequestedMoveDirection());
		Owner->SetActorRotation(rotation);
		StateMachine->RequestStateChange("Walk");
		return;
	}
}

void State_Idle::OnStateExit()
{

}

#pragma endregion

#pragma region Walk
State_Walk::State_Walk(AMilkyWayPawn* owner)
	:MilkyWayPawnState(owner)
{

}

void State_Walk::OnStateEnter()
{

}

void State_Walk::StateTick()
{
	if (Owner->JumpButtonPressedThisFrame)
	{
		StateMachine->RequestStateChange("Jump");
		return;
	}
	if (Owner->DiveButtonPressedThisFrame)
	{
		StateMachine->RequestStateChange("Dive");
		return;
	}
	if (Owner->CurrentLeftStick.Length() == 0)
	{
		StateMachine->RequestStateChange("Stop");
		return;
	}
	if (Owner->GetCameraRequestedMoveDirection().Dot(Owner->GetActorForwardVector()) < Owner->TurnKickDotThreshold)
	{
		StateMachine->RequestStateChange("TurnKick");
		return;
	}

	//Accelerate in facing direction
	if (Owner->HSpeed < Owner->CurrentMaxGroundSpeed)
	{
		Owner->HSpeed += Owner->CurrentLeftStick.Length() * Owner->CurrentGroundAcceleration * Owner->DeltaTime;
	}
	//Apply speed decay if we are above our ground speed limit and still trying to move
	else
	{
		if (Owner->CurrentLeftStick.Length() > 0)
		{
			Owner->HSpeed -= Owner->GroundSpeedDecay * Owner->DeltaTime;
		}
	}

	Owner->RotateTowardCameraRequestedMoveDirection();
	Owner->Move();

	Owner->GroundCheck();
	if (!Owner->OnGround)
	{
		StateMachine->RequestStateChange("Fall");
		return;
	}

}

void State_Walk::OnStateExit()
{

}
#pragma endregion

#pragma region Stop
State_Stop::State_Stop(AMilkyWayPawn* owner)
	:MilkyWayPawnState(owner)
{

}

void State_Stop::OnStateEnter()
{

}

void State_Stop::StateTick()
{
	Owner->Move();

	Owner->GroundCheck();
	if (!Owner->OnGround)
	{
		StateMachine->RequestStateChange("Fall");
		return;
	}
	else if (Owner->CurrentLeftStick.Length() > 0)
	{
		StateMachine->RequestStateChange("Walk");
		return;
	}
	else if (Owner->HSpeed == 0)
	{
		StateMachine->RequestStateChange("Idle");
		return;
	}
	else if (Owner->JumpButtonPressedThisFrame)
	{
		StateMachine->RequestStateChange("Jump");
		return;
	}
	
	if (Owner->HSpeed > 0)
	{
		Owner->HSpeed -= Owner->GroundDeceleration * Owner->DeltaTime;
	}
	else if (Owner->HSpeed < 0)
	{
		Owner->HSpeed += Owner->GroundDeceleration * Owner->DeltaTime;
	}
	if (abs(Owner->HSpeed) < Owner->GroundDeceleration * Owner->DeltaTime)
	{
		Owner->HSpeed = 0;
	}
}

void State_Stop::OnStateExit()
{

}
#pragma endregion

#pragma region Jump
State_Jump::State_Jump(AMilkyWayPawn* owner)
	:MilkyWayPawnState(owner)
{
	
}

void State_Jump::OnStateEnter()
{
	Owner->AirControlVelocity = FVector(0, 0, 0);
	Owner->VSpeed += Owner->JumpImpulse;
	Owner->CurrentGroundForward = Owner->GetActorForwardVector();
	Owner->OnGround = false;
	JumpThrustWindowActive = true;
	JumpThrustStopwatch = 0;
	StillHoldingJump = true;
}

void State_Jump::StateTick()
{
	if (!Owner->CurrentJumpButton)
		StillHoldingJump = false;
	float gravityAcceleration = Owner->GetCurrentGravity() * Owner->DeltaTime;
	if (!StillHoldingJump)
		gravityAcceleration *= Owner->JumpReleaseGravityScalar;
	Owner->VSpeed -= gravityAcceleration;
	if (JumpThrustWindowActive)
	{
		JumpThrustStopwatch += Owner->DeltaTime;
		if (JumpThrustStopwatch >= Owner->JumpThrustWindow)
		{
			JumpThrustWindowActive = false;
		}
		else
		{
			Owner->VSpeed += Owner->GetCurrentJumpThrust() * Owner->DeltaTime;
		}
		if (Owner->CurrentJumpButton == false)
		{
			JumpThrustWindowActive = false;
		}
	}
	
	if (Owner->DiveButtonPressedThisFrame)
	{
		StateMachine->RequestStateChange("Dive");
		return;
	}

	Owner->UpdateAirControl();
	Owner->Move();

	if (Owner->VSpeed <= 0)
	{
		Owner->GroundCheck();
		if (Owner->OnGround)
		{
			if (Owner->CurrentLeftStick.Length() == 0)
			{
				Owner->HSpeed = 0;
				StateMachine->RequestStateChange("Idle");
			}
			else
			{
				//Add forward momentum from air control to HSpeed
				FVector localAirControlVelocity = UKismetMathLibrary::InverseTransformDirection(Owner->GetActorTransform(), Owner->AirControlVelocity);
				Owner->HSpeed += localAirControlVelocity.X;

				StateMachine->RequestStateChange("Walk");
			}
			return;
		}
	}

}

void State_Jump::OnStateExit()
{
	Owner->AirControlVelocity = FVector(0, 0, 0);
}

#pragma endregion

#pragma region Fall
State_Fall::State_Fall(AMilkyWayPawn* owner)
	:MilkyWayPawnState(owner)
{

}

void State_Fall::OnStateEnter()
{
	Owner->CurrentGroundForward = Owner->GetActorForwardVector();
	StillHoldingJump = true;
}
void State_Fall::StateTick()
{
	if (!Owner->CurrentJumpButton)
		StillHoldingJump = false;
	float gravityAcceleration = Owner->GetCurrentGravity() * Owner->DeltaTime;
	if (!StillHoldingJump)
		gravityAcceleration *= Owner->JumpReleaseGravityScalar;
	Owner->VSpeed -= gravityAcceleration;
	if (Owner->DiveButtonPressedThisFrame)
	{
		StateMachine->RequestStateChange("Dive");
		return;
	}

	Owner->Move();

	Owner->GroundCheck();
	if (Owner->OnGround)
	{
		if (Owner->CurrentLeftStick.Length() == 0)
		{
			Owner->HSpeed = 0;
			StateMachine->RequestStateChange("Idle");
		}
		else
		{
			//Add forward momentum from air control to HSpeed
			FVector localAirControlVelocity = UKismetMathLibrary::InverseTransformDirection(Owner->GetActorTransform(), Owner->AirControlVelocity);
			Owner->HSpeed += localAirControlVelocity.X;

			StateMachine->RequestStateChange("Walk");
		}
		return;
	}

}

void State_Fall::OnStateExit()
{

}
#pragma endregion

#pragma region Turn Kick
State_TurnKick::State_TurnKick(AMilkyWayPawn* owner)
	:MilkyWayPawnState(owner)
{
}
void State_TurnKick::OnStateEnter()
{
	Owner->HSpeed *= -1;
	FRotator rotator = FRotator(0, 180, 0);
	Owner->AddActorLocalRotation(rotator);
}
void State_TurnKick::StateTick()
{
	if (Owner->HSpeed >= 0)
	{
		StateMachine->RequestStateChange("Walk");
		return;
	}
	if (Owner->JumpButtonPressedThisFrame)
	{
		StateMachine->RequestStateChange("SideFlip");
		return;
	}
	Owner->HSpeed += Owner->GroundDeceleration * Owner->DeltaTime;
	Owner->Move();
	Owner->GroundCheck();
	if (!Owner->OnGround)
	{
		StateMachine->RequestStateChange("Fall");
		return;
	}

}
void State_TurnKick::OnStateExit()
{
}
#pragma endregion

#pragma region Dive

State_Dive::State_Dive(AMilkyWayPawn* owner)
	:MilkyWayPawnState(owner)
{
	
}

void State_Dive::OnStateEnter()
{
	Owner->GraphicalsTransform->AddLocalRotation(FRotator(-50, 0, 0));
	if (Owner->OnGround)
	{
		Owner->HSpeed += Owner->DiveHImpulse * 2;
		Owner->VSpeed += Owner->DiveVImpulse;
		Owner->CurrentGroundForward = Owner->GetActorForwardVector();
		Owner->OnGround = false;
		Owner->CurrentGroundForward = Owner->GetActorForwardVector();
	}
	else
	{
		Owner->HSpeed += Owner->DiveHImpulse;
	}
}
void State_Dive::StateTick()
{
	Owner->Move();
	if (Owner->OnGround)
	{
		if (Owner->DiveButtonPressedThisFrame || Owner->JumpButtonPressedThisFrame)
		{
			StateMachine->RequestStateChange("Rollout");
			return;
		}
		float deltaDeceleration = Owner->GroundDeceleration * Owner->DeltaTime;
		if (abs(Owner->HSpeed) <= deltaDeceleration)
			Owner->HSpeed = 0;
		else if (Owner->HSpeed > 0)
			Owner->HSpeed -= deltaDeceleration;
		else
			Owner->HSpeed += deltaDeceleration;
		Owner->GroundCheck();

	}
	else
	{

		Owner->VSpeed -= Owner->GetCurrentGravity() * Owner->DeltaTime;
	}
	if (Owner->VSpeed < 0)
	{
		Owner->GroundCheck();
	}
	if (Owner->OnGround && Owner->HSpeed <= 0)
	{
		StateMachine->RequestStateChange("Idle");
		return;
	}
}
void State_Dive::OnStateExit()
{
	Owner->GraphicalsTransform->AddLocalRotation(FRotator(50, 0, 0));
}
#pragma endregion

#pragma region Rollout
State_Rollout::State_Rollout(AMilkyWayPawn* owner)
	:MilkyWayPawnState(owner)
{

}

void State_Rollout::OnStateEnter()
{
	Owner->VSpeed += Owner->DiveVImpulse;
	Owner->CurrentGroundForward = Owner->GetActorForwardVector();
	Owner->OnGround = false;
	Owner->CurrentGroundForward = Owner->GetActorForwardVector();

}

void State_Rollout::StateTick()
{
	Owner->VSpeed -= Owner->GetCurrentGravity() * Owner->DeltaTime;
	Owner->Move();

	if (Owner->VSpeed < 0)
	{
		Owner->GroundCheck();
		if (Owner->OnGround)
		{
			if (Owner->GetCameraRequestedMoveDirection().Length() > 0)
				StateMachine->RequestStateChange("Walk");
			else
				StateMachine->RequestStateChange("Idle");
			return;
		}
	}

	float spinSpeed = 1080;
	FRotator rotator = FRotator(-1, 0, 0) * Owner->DeltaTime * spinSpeed;
	Owner->GraphicalsTransform->AddLocalRotation(rotator);
}

void State_Rollout::OnStateExit()
{
	Owner->GraphicalsTransform->SetRelativeRotation(FRotator(0, 0, 0));
}
#pragma endregion

#pragma region Bonk
State_Bonk::State_Bonk(AMilkyWayPawn* owner)
	:MilkyWayPawnState(owner)
{
}

void State_Bonk::OnStateEnter()
{
	WallKickStopwatch = 0;
	BonkStopwatch = 0;

	Owner->VSpeed = 0;
	Owner->HSpeed = Owner->BonkSpeed;

	FRotator rotator = FRotator(180, 0, 0);
	Owner->GraphicalsTransform->AddLocalRotation(rotator);
	FVector deltaVector = FVector(0, 0, -Owner->PlayerHeight);
	Owner->GraphicalsTransform->AddLocalOffset(deltaVector);
}

void State_Bonk::StateTick()
{
	if (BonkTimerActive)
	{
		BonkStopwatch += Owner->DeltaTime;
	}
	if (BonkStopwatch >= BonkTime)
	{
		StateMachine->RequestStateChange("Idle");
		return;
	}

	if (Owner->OnGround)
	{
		BonkTimerActive = true;
		Owner->Move();
		Owner->GroundCheck();
	}
	else
	{
		BonkTimerActive = false;
		BonkStopwatch = 0;

		WallKickStopwatch += Owner->DeltaTime;
		if (WallKickStopwatch <= WallKickWindow)
		{
			if (Owner->JumpButtonPressedThisFrame)
			{
				StateMachine->RequestStateChange("WallKick");
				return;
			}
		}
		else
		{
			Owner->VSpeed -= Owner->GetCurrentGravity() * Owner->DeltaTime;
			Owner->Move();
			Owner->GroundCheck();
		}
	}
}

void State_Bonk::OnStateExit()
{
	FRotator rotator = FRotator(-180, 0, 0);
	Owner->GraphicalsTransform->AddLocalRotation(rotator);
	FVector deltaVector = FVector(0, 0, -Owner->PlayerHeight);
	Owner->GraphicalsTransform->SetRelativeLocation(FVector(0, 0, 0));
}
#pragma endregion

#pragma region Wall Kick
State_WallKick::State_WallKick(AMilkyWayPawn* owner)
	:MilkyWayPawnState(owner)
{

}

void State_WallKick::OnStateEnter()
{
	StillHoldingJump = true;
	Owner->AirControlVelocity = FVector(0, 0, 0);
	Owner->CurrentGroundForward = Owner->GetActorForwardVector();


	FVector forward = Owner->GetActorForwardVector();	
	FVector newForward = FMath::GetReflectionVector(forward, Owner->LastHitWallVector);
	FRotator rotator = UKismetMathLibrary::FindLookAtRotation(Owner->GetActorLocation(), Owner->GetActorLocation() + newForward);
	rotator.Pitch = 0;
	rotator.Roll = 0;
	Owner->HSpeed = 800;
	Owner->VSpeed = Owner->JumpImpulse * Owner->WallKickJumpScalar;
	Owner->SetActorRotation(rotator);
}

void State_WallKick::StateTick()
{

	if (!Owner->CurrentJumpButton)
		StillHoldingJump = false;
	float gravityAcceleration = Owner->GetCurrentGravity() * Owner->DeltaTime;
	if (!StillHoldingJump)
		gravityAcceleration *= Owner->JumpReleaseGravityScalar;
	Owner->VSpeed -= gravityAcceleration;
	Owner->Move();
	Owner->GroundCheck();
	if (Owner->OnGround)
	{
		if (Owner->CurrentLeftStick.Length() == 0)
		{
			Owner->HSpeed = 0;
			StateMachine->RequestStateChange("Idle");
		}
		else
		{
			//Add forward momentum from air control to HSpeed
			FVector localAirControlVelocity = UKismetMathLibrary::InverseTransformDirection(Owner->GetActorTransform(), Owner->AirControlVelocity);
			Owner->HSpeed += localAirControlVelocity.X;

			StateMachine->RequestStateChange("Walk");
		}
		return;
	}
	if (Owner->DiveButtonPressedThisFrame)
	{
		StateMachine->RequestStateChange("Dive");
		return;
	}
	
	Owner->UpdateAirControl();
}

void State_WallKick::OnStateExit()
{
	Owner->AirControlVelocity = FVector(0, 0, 0);
}
#pragma endregion

#pragma region Side flip
State_SideFlip::State_SideFlip(AMilkyWayPawn* owner)
	:MilkyWayPawnState(owner)
{

}

void State_SideFlip::OnStateEnter()
{
	StillHoldingJump = true;
	Owner->AirControlVelocity = FVector(0, 0, 0);

	Owner->HSpeed *= -1;
	Owner->VSpeed = Owner->JumpImpulse * Owner->SideFlipJumpScalar;
	Owner->OnGround = false;
	Owner->CurrentGroundForward = Owner->GetActorForwardVector();
}

void State_SideFlip::StateTick()
{
	
	Owner->UpdateAirControl();

	if (!Owner->CurrentJumpButton)
		StillHoldingJump = false;
	float gravityAcceleration = Owner->GetCurrentGravity() * Owner->DeltaTime;
	if (!StillHoldingJump)
		gravityAcceleration *= Owner->JumpReleaseGravityScalar;
	Owner->VSpeed -= gravityAcceleration;
	Owner->Move();
	Owner->GroundCheck();
	if (Owner->OnGround)
	{
		if (Owner->CurrentLeftStick.Length() == 0)
		{
			Owner->HSpeed = 0;
			StateMachine->RequestStateChange("Idle");
		}
		else
		{
			//Add forward momentum from air control to HSpeed
			FVector localAirControlVelocity = UKismetMathLibrary::InverseTransformDirection(Owner->GetActorTransform(), Owner->AirControlVelocity);
			Owner->HSpeed += localAirControlVelocity.X;

			StateMachine->RequestStateChange("Walk");
		}
		return;
	}
	if (Owner->DiveButtonPressedThisFrame)
	{
		StateMachine->RequestStateChange("Dive");
		return;
	}

}

void State_SideFlip::OnStateExit()
{
	Owner->AirControlVelocity = FVector(0, 0, 0);
}
#pragma endregion

#pragma region Push
State_Push::State_Push(AMilkyWayPawn* owner)
	:MilkyWayPawnState(owner)
{

}

void State_Push::OnStateEnter()
{
	Owner->HSpeed = 0;
	Owner->GraphicalsTransform->AddLocalRotation(FRotator(-10, 0, 0));
}

void State_Push::StateTick()
{
	Owner->RotateTowardCameraRequestedMoveDirection();

	if (Owner->JumpButtonPressedThisFrame)
	{
		StateMachine->RequestStateChange("Jump");
		return;
	}
	if (Owner->GetCameraRequestedMoveDirection().Dot(Owner->LastHitWallVector) > Owner->BonkDotThreshold)
	{
		StateMachine->RequestStateChange("Walk");
		return;
	}
}

void State_Push::OnStateExit()
{
	Owner->GraphicalsTransform->AddLocalRotation(FRotator(10, 0, 0));
}
#pragma endregion