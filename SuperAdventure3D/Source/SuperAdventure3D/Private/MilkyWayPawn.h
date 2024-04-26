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

	UPROPERTY(EditAnywhere)
	float PlayerRadius = 50;

	UPROPERTY(EditAnywhere)
	float PlayerHeight = 100;

	UPROPERTY(EditAnywhere)
	float StepHeight = 20;
	
	UPROPERTY(EditAnywhere, Category = "Debug")
	bool DebugDrawWallCollisionChecks = false;

private:
	float DeltaTime;
	FVector2D CurrentLeftStick;
	FVector2D CurrentRightStick;
	float HSpeed;
	float VSpeed;
	FVector WallCollisionRayStartPoints[9];
	FVector PreviousFrameLocation;

protected:
	enum PawnStates { Idle, Walk, Stop, Jump };
	enum SurfaceTypes { Wall, Ground, Ceiling };
	PawnStates CurrentState = Idle;
	void OnLeftStickVertical(float axisValue);
	void OnLeftStickHorizontal(float axisValue);
	void OnRightStickVertical(float axisValue);
	void OnRightStickHorizontal(float axisValue);
	void Idle_Tick();
	void Walk_Tick();
	void Stop_Tick();
	void Jump_Tick();
	void Move();
	FVector GetCameraRequestedMoveDirection();
	void UpdateWallCollisionRayStartPoints();
	FVector WallCollisionCheck();
	enum SurfaceTypes QuerySurfaceType(FVector surfaceNormal);


};
