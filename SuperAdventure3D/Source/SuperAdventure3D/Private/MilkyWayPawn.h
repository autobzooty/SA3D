// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Pawn.h"
#include "MilkyWayPawnState.h"
#include "MilkyWayPawn.generated.h"

UCLASS()
class AMilkyWayPawn : public APawn
{
	GENERATED_BODY()

public:
	// Sets default values for this pawn's properties
	AMilkyWayPawn();
	
	UPROPERTY(EditDefaultsOnly, BlueprintReadWrite)
	class USceneComponent* Root;
	UPROPERTY(EditDefaultsOnly, BlueprintReadWrite)
	class USpringArmComponent* SpringArm;
	UPROPERTY(EditDefaultsOnly, BlueprintReadWrite)
	class UCameraComponent* Camera;
	UPROPERTY(EditDefaultsOnly, BlueprintReadWrite)
	class USceneComponent* GraphicalsTransform;
	UPROPERTY()
	class UMilkyWayPawnStateMachine* StateMachine;

protected:
	// Called when the game starts or when spawned
	virtual void BeginPlay() override;

	

public:	
	// Called every frame
	virtual void Tick(float DeltaTime) override;

	// Called to bind functionality to input
	virtual void SetupPlayerInputComponent(class UInputComponent* PlayerInputComponent) override;

	UPROPERTY(EditAnywhere)
	float BaseGroundAcceleration = 500;

	UPROPERTY(EditAnywhere)
	float GroundDeceleration = 1000;

	UPROPERTY(EditAnywhere)
	float GroundSpeedDecay = 200;

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

	UPROPERTY(EditAnywhere)
	float Gravity = 10;

	UPROPERTY(EditAnywhere)
	float InitialJumpStrength = 100;

	UPROPERTY(EditAnywhere)
	float WallDotThreshold = 0.05;

	UPROPERTY(EditAnywhere)
	float TurnKickDotThreshold = -0.5;
	
	UPROPERTY(EditAnywhere)
	float BonkSpeed = -200;

	UPROPERTY(EditAnywhere)
	float BonkSpeedThreshold = 300;
	
	UPROPERTY(EditAnywhere)
	float BonkDotThreshold = -0.7;

	UPROPERTY(EditAnywhere)
	float DiveHImpulse = 500;
	UPROPERTY(EditAnywhere)
	float DiveVImpulse = 500;

	UPROPERTY(EditAnywhere)
	float SideFlipJumpScalar = 1.5;
	

	UPROPERTY(EditAnywhere)
	float AirControlAcceleration = 600;
	
	UPROPERTY(EditAnywhere, Category = "Debug")
	bool DebugDrawWallCollisionChecks = false;

	FVector LastHitWallVector;

private:
	float DeltaTime;
	FVector2D CurrentLeftStick;
	FVector2D CurrentRightStick;
	float CurrentMaxGroundSpeed;
	float CurrentGroundAcceleration;
	bool CurrentJumpButton = false;
	bool CurrentDiveButton = false;
	bool JumpButtonPressedThisFrame = false;
	bool DiveButtonPressedThisFrame = false;
	float HSpeed;
	float VSpeed;
	FVector AirControlVelocity;
	FVector WallCollisionRayStartPoints[9];
	FVector PreviousFrameLocation;
	bool PreviousFrameJumpButton = false;
	bool PreviousFrameDiveButton = false;
	bool OnGround = true;

protected:
	enum PawnStates { Idle, Walk, Stop, Jump, Fall };
	enum SurfaceTypes { Wall, Ground, Ceiling };
	PawnStates CurrentState = Idle;
	void OnLeftStickVertical(float axisValue);
	void OnLeftStickHorizontal(float axisValue);
	void OnRightStickVertical(float axisValue);
	void OnRightStickHorizontal(float axisValue);
	void OnJumpButtonPressed();
	void OnJumpButtonHeld();
	void OnJumpButtonReleased();
	void OnDiveButtonPressed();
	void OnDiveButtonHeld();
	void OnDiveButtonReleased();
	void Move();
	FVector GetCameraRequestedMoveDirection();
	void UpdateWallCollisionRayStartPoints();
	FVector WallCollisionCheck(FVector attemptedMoveLocation);
	enum SurfaceTypes QuerySurfaceType(FVector surfaceNormal);
	void GroundCheck();
	void CeilingCheck();
	void PreInitializeComponents() override;

	friend class State_Idle;
	friend class State_Walk;
	friend class State_Stop;
	friend class State_Jump;
	friend class State_Fall;
	friend class State_TurnKick;
	friend class State_Dive;
	friend class State_Rollout;
	friend class State_Bonk;
	friend class State_WallKick;
	friend class State_SideFlip;
};
