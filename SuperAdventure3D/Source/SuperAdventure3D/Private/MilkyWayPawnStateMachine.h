#pragma once

#include "CoreMinimal.h"
#include "Components/ActorComponent.h"
#include "MilkyWayPawnState.h"
#include "MilkyWayPawnStateMachine.generated.h"

class AMilkyWayPawn;


UCLASS( ClassGroup=(Custom), meta=(BlueprintSpawnableComponent) )
class UMilkyWayPawnStateMachine : public UActorComponent
{
	GENERATED_BODY()

public:	
	// Sets default values for this component's properties
	UMilkyWayPawnStateMachine();
	UMilkyWayPawnStateMachine(const FObjectInitializer&);
	~UMilkyWayPawnStateMachine();

	void Setup(AMilkyWayPawn* owner);

	MilkyWayPawnState* PreviousState;
	MilkyWayPawnState* CurrentState;
	AMilkyWayPawn* Owner;

	MilkyWayPawnState* Idle;
	MilkyWayPawnState* Walk;
	MilkyWayPawnState* Stop;
	MilkyWayPawnState* Jump;
	MilkyWayPawnState* Fall;
	MilkyWayPawnState* TurnKick;
	MilkyWayPawnState* Dive;
	MilkyWayPawnState* Rollout;
	MilkyWayPawnState* Bonk;
	MilkyWayPawnState* WallKick;
	MilkyWayPawnState* SideFlip;

protected:
	// Called when the game starts
	virtual void BeginPlay() override;

	void GetCurrentState();	

public:	
	// Called every frame
	virtual void TickComponent(float DeltaTime, ELevelTick TickType, FActorComponentTickFunction* ThisTickFunction) override;
	void ChangeState(FName newState);
};

class State_Idle : public MilkyWayPawnState
{
public:
	State_Idle(AMilkyWayPawn* owner);

	virtual void OnStateEnter() override;
	virtual void StateTick() override;
	virtual void OnStateExit() override;

};

class State_Walk : public MilkyWayPawnState
{
public:
	State_Walk(AMilkyWayPawn* owner);

	virtual void OnStateEnter() override;
	virtual void StateTick() override;
	virtual void OnStateExit() override;

};

class State_Stop : public MilkyWayPawnState
{
public:
	State_Stop(AMilkyWayPawn* owner);

	virtual void OnStateEnter() override;
	virtual void StateTick() override;
	virtual void OnStateExit() override;

};

class State_Jump : public MilkyWayPawnState
{
public:
	State_Jump(AMilkyWayPawn* owner);

	virtual void OnStateEnter() override;
	virtual void StateTick() override;
	virtual void OnStateExit() override;

};

class State_Fall : public MilkyWayPawnState
{
public:
	State_Fall(AMilkyWayPawn* owner);

	virtual void OnStateEnter() override;
	virtual void StateTick() override;
	virtual void OnStateExit() override;

};

class State_TurnKick : public MilkyWayPawnState
{
public:
	State_TurnKick(AMilkyWayPawn* owner);

	virtual void OnStateEnter() override;
	virtual void StateTick() override;
	virtual void OnStateExit() override;

};

class State_Dive : public MilkyWayPawnState
{
public:
	State_Dive(AMilkyWayPawn* owner);

	virtual void OnStateEnter() override;
	virtual void StateTick() override;
	virtual void OnStateExit() override;

};

class State_Rollout : public MilkyWayPawnState
{
public:
	State_Rollout(AMilkyWayPawn* owner);

	virtual void OnStateEnter() override;
	virtual void StateTick() override;
	virtual void OnStateExit() override;

};

class State_Bonk : public MilkyWayPawnState
{
public:
	State_Bonk(AMilkyWayPawn* owner);

	virtual void OnStateEnter() override;
	virtual void StateTick() override;
	virtual void OnStateExit() override;

	float BonkTime = 0.5;
	float BonkStopwatch = 0;
	bool BonkTimerActive = false;

	float WallKickWindow = 0.17;
	float WallKickStopwatch = 0;

};

class State_WallKick : public MilkyWayPawnState
{
public:
	State_WallKick(AMilkyWayPawn* owner);

	virtual void OnStateEnter() override;
	virtual void StateTick() override;
	virtual void OnStateExit() override;
};

class State_SideFlip : public MilkyWayPawnState
{
public:
	State_SideFlip(AMilkyWayPawn* owner);

	virtual void OnStateEnter() override;
	virtual void StateTick() override;
	virtual void OnStateExit() override;
};