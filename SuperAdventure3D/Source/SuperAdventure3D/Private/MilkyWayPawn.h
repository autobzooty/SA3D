// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Pawn.h"
#include "MilkyWayPawn.generated.h"

UCLASS()
class AMilkyWayPawn : public APawn
{
	GENERATED_BODY()

public:
	// Sets default values for this pawn's properties
	AMilkyWayPawn();
	
	UPROPERTY(EditDefaultsOnly, BlueprintReadWrite)
	class UArrowComponent* RootArrow;
	UPROPERTY(EditDefaultsOnly, BlueprintReadWrite)
	class USpringArmComponent* SpringArm;
	UPROPERTY(EditDefaultsOnly, BlueprintReadWrite)
	class UCameraComponent* Camera;
	

protected:
	// Called when the game starts or when spawned
	virtual void BeginPlay() override;

	

public:	
	// Called every frame
	virtual void Tick(float DeltaTime) override;

	// Called to bind functionality to input
	virtual void SetupPlayerInputComponent(class UInputComponent* PlayerInputComponent) override;

	UPROPERTY(EditAnywhere)
	float GroundAcceleration = 500;

	UPROPERTY(EditAnywhere)
	float GroundDeceleration = 1000;

	UPROPERTY(EditAnywhere)
	float TurnSpeed = 540;

	UPROPERTY(EditAnywhere)
	float BaseMaxGroundSpeed = 500;

	UPROPERTY(EditAnywhere)
	float CameraTurnSpeed = 180;

protected:

	void OnLeftStickVertical(float AxisValue);
	void OnLeftStickHorizontal(float AxisValue);
	void OnRightStickVertical(float AxisValue);
	void OnRightStickHorizontal(float AxisValue);
	void Idle_Tick();
	void Walk_Tick();
	void Stop_Tick();
	void Jump_Tick();
	void Move();
	FVector GetCameraRequestedMoveDirection();

private:
	float DeltaTime;
	enum PawnStates {Idle , Walk , Stop , Jump};
	PawnStates CurrentState = Idle;
	FVector2D CurrentLeftStick;
	FVector2D CurrentRightStick;
	float HSpeed;
	float VSpeed;

};
