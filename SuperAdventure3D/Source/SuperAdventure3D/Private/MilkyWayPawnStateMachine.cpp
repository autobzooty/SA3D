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
}


// Called when the game starts
void UMilkyWayPawnStateMachine::BeginPlay()
{
	Super::BeginPlay();
	CurrentState = Idle;
	CurrentState->OnStateEnter();
}


// Called every frame
void UMilkyWayPawnStateMachine::TickComponent(float DeltaTime, ELevelTick TickType, FActorComponentTickFunction* ThisTickFunction)
{
	Super::TickComponent(DeltaTime, TickType, ThisTickFunction);

	CurrentState->StateTick();	
}

void UMilkyWayPawnStateMachine::ChangeState(FName newStateName)
{
	//TO-DO: Add debug flag that shows info when states change
	if (newStateName == CurrentState->StateName)
		return;

	MilkyWayPawnState* newState = Idle;
	
	if (newStateName == "Walk")
		newState = Walk;
	else if (newStateName == "Stop")
		newState = Stop;
	else if (newStateName == "Jump")
		newState = Jump;
	else if (newStateName == "Fall")
		newState = Fall;

	CurrentState->OnStateExit();
	newState->OnStateEnter();
	CurrentState = newState;
}

#pragma region IdleState
State_Idle::State_Idle(AMilkyWayPawn* owner)
	:MilkyWayPawnState(owner)
{
	
}

void State_Idle::OnStateEnter()
{

}

void State_Idle::StateTick()
{
	Owner->GroundCheck();
	if (!Owner->OnGround)
	{
		StateMachine->ChangeState("Fall");
		return;
	}
	if (Owner->CurrentJumpButton)
	{
		Owner->VSpeed += Owner->InitialJumpStrength;
		Owner->OnGround = false;
		StateMachine->ChangeState("Jump");
		return;
	}
	if (Owner->CurrentLeftStick.Length() > 0)
	{
		//Snap rotation to camera's requested move direction
		FRotator rotation = UKismetMathLibrary::FindLookAtRotation(Owner->GetActorLocation(), Owner->GetActorLocation() + Owner->GetCameraRequestedMoveDirection());
		Owner->SetActorRotation(rotation);
		StateMachine->ChangeState("Walk");
		return;
	}
}

void State_Idle::OnStateExit()
{

}

#pragma endregion

#pragma region WalkState
State_Walk::State_Walk(AMilkyWayPawn* owner)
	:MilkyWayPawnState(owner)
{

}

void State_Walk::OnStateEnter()
{

}

void State_Walk::StateTick()
{
	Owner->GroundCheck();
	if (!Owner->OnGround)
	{
		StateMachine->ChangeState("Fall");
		return;
	}
	if (Owner->CurrentJumpButton)
	{
		Owner->VSpeed += Owner->InitialJumpStrength;
		Owner->OnGround = false;
		StateMachine->ChangeState("Jump");
		return;
	}
	if (Owner->CurrentLeftStick.Length() == 0)
	{
		StateMachine->ChangeState("Stop");
		return;
	}

	//Accelerate in facing direction
	if (Owner->HSpeed < Owner->BaseMaxGroundSpeed)
	{
		Owner->HSpeed += Owner->CurrentLeftStick.Length() * Owner->GroundAcceleration * Owner->DeltaTime;
	}

	//Rotate toward camera's requested direction
	float turnScalar = Owner->GetCameraRequestedMoveDirection().Dot(Owner->GetActorRightVector());
	float turnAmount = Owner->TurnSpeed * turnScalar * Owner->DeltaTime;

	FRotator rotator = FRotator(0, turnAmount, 0);
	Owner->AddActorLocalRotation(rotator);

	Owner->Move();
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
	Owner->GroundCheck();
	if (!Owner->OnGround)
	{
		StateMachine->ChangeState("Fall");
	}
	if (Owner->CurrentLeftStick.Length() > 0)
	{
		StateMachine->ChangeState("Walk");
	}
	if (Owner->HSpeed == 0)
	{
		StateMachine->ChangeState("Idle");
	}
	else if (Owner->HSpeed > 0)
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
	Owner->Move();
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

}

void State_Jump::StateTick()
{
	Owner->VSpeed -= Owner->Gravity * Owner->DeltaTime;
	if (Owner->VSpeed <= 0)
	{
		Owner->GroundCheck();
		if (Owner->OnGround)
		{
			Owner->VSpeed = 0;
			if (Owner->CurrentLeftStick.Length() == 0)
			{
				Owner->HSpeed = 0;
				StateMachine->ChangeState("Idle");
			}
			else
			{
				StateMachine->ChangeState("Walk");
			}
			return;
		}
	}
	Owner->Move();

}

void State_Jump::OnStateExit()
{

}

#pragma endregion

#pragma region Fall
State_Fall::State_Fall(AMilkyWayPawn* owner)
	:MilkyWayPawnState(owner)
{

}

void State_Fall::OnStateEnter()
{

}
void State_Fall::StateTick()
{
	Owner->VSpeed -= Owner->Gravity * Owner->DeltaTime;
	Owner->GroundCheck();
	if (Owner->OnGround)
	{
		Owner->VSpeed = 0;
		if (Owner->CurrentLeftStick.Length() == 0)
		{
			Owner->HSpeed = 0;
			StateMachine->ChangeState("Idle");
		}
		else
		{
			StateMachine->ChangeState("Walk");
		}
		return;
	}
	Owner->Move();
}

void State_Fall::OnStateExit()
{

}
#pragma endregion