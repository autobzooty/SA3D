#pragma once

#include "CoreMinimal.h"
#include "MilkyWayPawn.h"

class AMilkyWayPawn;
class UMilkyWayPawnStateMachine;

class MilkyWayPawnState
{
public:
	MilkyWayPawnState(AMilkyWayPawn* owner);
	virtual ~MilkyWayPawnState();

	FString StateName;
	AMilkyWayPawn* Owner;
	UMilkyWayPawnStateMachine* StateMachine;

	virtual void OnStateEnter() = 0;
	virtual void StateTick() = 0;
	virtual void OnStateExit() = 0;
};
