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

	Load = new State_Load(Owner);
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
	StarDance = new State_StarDance(Owner);
}


// Called when the game starts
void UMilkyWayPawnStateMachine::BeginPlay()
{
	Super::BeginPlay();
	CurrentState = Load;
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
	if (RequestedState != "")
	{
		if (GEngine)
			GEngine->AddOnScreenDebugMessage(-1, 15.0f, FColor::Yellow, "WARNING: Multiple state changes requested this frame!");
	}
	RequestedState = newStateName;
}

void UMilkyWayPawnStateMachine::ChangeState()
{
	if (Owner->PrintStateChanges)
	{
		if (GEngine)
			GEngine->AddOnScreenDebugMessage(-1, 15.0f, FColor::Yellow, RequestedState.ToString());
	}

	if (RequestedState == CurrentState->StateName)
		return;

	MilkyWayPawnState* newState = Load;

	if (RequestedState == "Load")
		newState = Load;
	else if (RequestedState == "Idle")
		newState = Idle;
	else if (RequestedState == "Walk")
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
	else if (RequestedState == "StarDance")
		newState = StarDance;

	CurrentState->OnStateExit();
	newState->OnStateEnter();
	CurrentState = newState;
	RequestedState = "";
}

#pragma region Load
State_Load::State_Load(AMilkyWayPawn* owner)
	:MilkyWayPawnState(owner)
{

}

void State_Load::OnStateEnter()
{

}

void State_Load::StateTick()
{
	LoadStopwatch += Owner->DeltaTime;
	if (LoadStopwatch >= Owner->LoadTime)
	{
		StateMachine->RequestStateChange("Idle");
	}
}

void State_Load::OnStateExit()
{

}
#pragma endregion

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
	FootStopwatch = 0;
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
#pragma region Graphicals Effects
	//Graphicals effects
	FootStopwatch += Owner->DeltaTime;
	float currentFootTime = FMath::Lerp(MaxFootTime, MinFootTime, Owner->HSpeed / (Owner->BaseMaxGroundSpeed * 2));
	currentFootTime = FMath::Clamp(currentFootTime, MinFootTime, MaxFootTime);

	if (FootStopwatch >= currentFootTime)
	{
		CurrentFoot = !CurrentFoot;
		FootStopwatch = 0;
	}

	
	float turnRollAlpha = Owner->GetCameraRequestedMoveDirection().Dot(Owner->GetActorRightVector());
	float currentTurnRoll = FMath::Lerp(0, TurnRoll, turnRollAlpha);
	FRotator turnRotation = FRotator(0, 0, currentTurnRoll);
	FRotator stepRotation;
	if(CurrentFoot == 0)
		stepRotation = FRotator(0, 0, StepRotation);
	else if(CurrentFoot == 1)
		stepRotation = FRotator(0, 0, -StepRotation);
	float pitchAlpha = (Owner->HSpeed - (Owner->BaseMaxGroundSpeed * 0.9)) / ((Owner->BaseMaxGroundSpeed * 2.0) - (Owner->BaseMaxGroundSpeed * 0.9));
	float currentPitch = FMath::Lerp(0, MaxSpeedTilt, pitchAlpha);
	FRotator speedRotation = FRotator(currentPitch, 0, 0);

	Owner->GraphicalsTransform->SetRelativeRotation(turnRotation + stepRotation + speedRotation);
#pragma endregion

	Owner->RotateTowardCameraRequestedMoveDirection(Owner->GetCurrentTurnSpeed());
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
	Owner->GraphicalsTransform->SetRelativeRotation(FRotator(0, 0, 0));
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

	if (!JumpThrustWindowActive)
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
		Owner->VSpeed += Owner->DiveVImpulse;
		Owner->CurrentGroundForward = Owner->GetActorForwardVector();
		Owner->OnGround = false;
		Owner->CurrentGroundForward = Owner->GetActorForwardVector();
	}
	Owner->HSpeed += Owner->DiveHImpulse;
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
		float deltaDeceleration = Owner->GroundDiveDeceleration * Owner->DeltaTime;
		if (abs(Owner->HSpeed) <= deltaDeceleration)
			Owner->HSpeed = 0;
		else if (Owner->HSpeed > 0)
			Owner->HSpeed -= deltaDeceleration;
		else
			Owner->HSpeed += deltaDeceleration;
		Owner->GroundCheck();
		Owner->RotateTowardCameraRequestedMoveDirection(Owner->GetCurrentTurnSpeed() * Owner->DiveTurnSpeedScalar);
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
	Owner->RotateTowardCameraRequestedMoveDirection(Owner->GetCurrentTurnSpeed());

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

#pragma region Star Dance
State_StarDance::State_StarDance(AMilkyWayPawn* owner)
	:MilkyWayPawnState(owner)

{
}

void State_StarDance::OnStateEnter()
{
	StarDancePhase = 0;
	StarDanceStopwatch = 0;
	Owner->HSpeed = 0;
	Owner->AirControlVelocity = FVector(0, 0, 0);
	if (GEngine)
		GEngine->AddOnScreenDebugMessage(-1, 15.0f, FColor::Yellow, "Entering Phase 0");

}

void State_StarDance::StateTick()
{
	Owner->Move();
	Owner->GroundCheck();
	if (StarDancePhase == 0)
	{
		if (Owner->OnGround)
		{
			StarDancePhase = 1;
			if (GEngine)
				GEngine->AddOnScreenDebugMessage(-1, 15.0f, FColor::Yellow, "Entering Phase 1");
		}
		else
		{
			Owner->VSpeed -= Owner->GetCurrentGravity() * Owner->DeltaTime;
		}
	}
	else if (StarDancePhase == 1)
	{
		StarDanceStopwatch += Owner->DeltaTime;
		//Backflip
		float flipSpeed = 720 * Owner->DeltaTime;
		FRotator deltaRotator = FRotator(flipSpeed, 0, 0);
		Owner->GraphicalsTransform->AddLocalRotation(deltaRotator);
		
		if (StarDanceStopwatch >= 1.0f)
		{
			StarDancePhase = 2;
			if (GEngine)
				GEngine->AddOnScreenDebugMessage(-1, 15.0f, FColor::Yellow, "Entering Phase 2");

		}
	}
	else if (StarDancePhase == 2)
	{
		StarDanceStopwatch += Owner->DeltaTime;

		//Speen
		float spinSpeed = 1080 * Owner->DeltaTime;
		FRotator deltaRotator = FRotator(0, spinSpeed, 0);
		Owner->GraphicalsTransform->AddLocalRotation(deltaRotator);

		//When speen completes, change to idle state
		if (StarDanceStopwatch >= 2.0)
		{
			StateMachine->RequestStateChange("Idle");
			if (GEngine)
				GEngine->AddOnScreenDebugMessage(-1, 15.0f, FColor::Yellow, "Entering Idle");

		}
	}
}

void State_StarDance::OnStateExit()
{
	Owner->GraphicalsTransform->SetRelativeRotation(FRotator(0, 0, 0));
}
#pragma endregion