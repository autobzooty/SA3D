// Fill out your copyright notice in the Description page of Project Settings.


#include "MilkyWayPawn.h"
#include "GameFramework/SpringArmComponent.h"
#include "Camera/CameraComponent.h"
#include "Components/ArrowComponent.h"
#include "Kismet/KismetMathLibrary.h"
#include "Kismet/KismetSystemLibrary.h"
#include "DrawDebugHelpers.h"
#include "MilkyWayPawnStateMachine.h"

// Sets default values
AMilkyWayPawn::AMilkyWayPawn()
{
	
	// Set this pawn to call Tick() every frame.  You can turn this off to improve performance if you don't need it.
	PrimaryActorTick.bCanEverTick = true;

	// Instantiating your class Components
	Root = CreateDefaultSubobject<USceneComponent>("Root");
	Camera = CreateDefaultSubobject<UCameraComponent>(TEXT("Camera"));
	SpringArm = CreateDefaultSubobject<USpringArmComponent>(TEXT("SpringArm"));
	StateMachine = CreateDefaultSubobject<UMilkyWayPawnStateMachine>(TEXT("Statema"));
	GraphicalsTransform = CreateDefaultSubobject<USceneComponent>(TEXT("GraphicalsRoot"));


	GraphicalsTransform->SetupAttachment(Root);

	//Attach the Spring Arm to the root
	//FRotator newRotation = FRotator(0, -20, 0);
	//SpringArm->SetRelativeRotation(newRotation);

	//SpringArm->AttachToComponent(Root, FAttachmentTransformRules::KeepRelativeTransform);
	SpringArm->SetupAttachment(Root);

	//Attach the Camera to the SpringArmComponent
	//Camera->AttachToComponent(SpringArm, FAttachmentTransformRules::KeepRelativeTransform);
	Camera->SetupAttachment(SpringArm);

	//Setting default properties of the SpringArmComp
	SpringArm->bUsePawnControlRotation = false;
	SpringArm->bInheritPitch = false;
	SpringArm->bInheritYaw = false;
	SpringArm->bInheritRoll = false;
	SpringArm->bEnableCameraLag = true;
	SpringArm->TargetArmLength = 1000.0f;
	FRotator newRotation = FRotator(0, -20, 0);
	SpringArm->SetRelativeRotation(newRotation);
}

void AMilkyWayPawn::PreInitializeComponents()
{
	StateMachine->Setup(this);
}

// Called when the game starts or when spawned
void AMilkyWayPawn::BeginPlay()
{
	Super::BeginPlay();
	FRotator newRotation = FRotator(-20, GetActorRotation().Yaw, 0);
	SpringArm->SetRelativeRotation(newRotation);
	//FString debugText = FString::Printf(TEXT("actor yaw: %f"), GetActorRotation().Yaw);
	//if (GEngine)
	//	GEngine->AddOnScreenDebugMessage(-1, 15.0f, FColor::Yellow, debugText);
}

// Called every frame
void AMilkyWayPawn::Tick(float _DeltaTime)
{
	Super::Tick(_DeltaTime);
	DeltaTime = _DeltaTime;

	if (CurrentJumpButton)
	{
		OnJumpButtonHeld();
	}
	if (CurrentDiveButton)
	{
		OnDiveButtonHeld();
	}
	PreviousFrameDiveButton = CurrentDiveButton;
	PreviousFrameJumpButton = CurrentJumpButton;
}

// Called to bind functionality to input
void AMilkyWayPawn::SetupPlayerInputComponent(UInputComponent* PlayerInputComponent)
{
	Super::SetupPlayerInputComponent(PlayerInputComponent);

	PlayerInputComponent->BindAxis(TEXT("LeftStickVertical"), this, &AMilkyWayPawn::OnLeftStickVertical);
	PlayerInputComponent->BindAxis(TEXT("LeftStickHorizontal"), this, &AMilkyWayPawn::OnLeftStickHorizontal);
	PlayerInputComponent->BindAxis(TEXT("RightStickVertical"), this, &AMilkyWayPawn::OnRightStickVertical);
	PlayerInputComponent->BindAxis(TEXT("RightStickHorizontal"), this, &AMilkyWayPawn::OnRightStickHorizontal);
	PlayerInputComponent->BindAction(TEXT("Jump"), IE_Pressed, this, &AMilkyWayPawn::OnJumpButtonPressed);
	PlayerInputComponent->BindAction(TEXT("Jump"), IE_Released, this, &AMilkyWayPawn::OnJumpButtonReleased);
	PlayerInputComponent->BindAction(TEXT("Dive"), IE_Pressed, this, &AMilkyWayPawn::OnDiveButtonPressed);
	PlayerInputComponent->BindAction(TEXT("Dive"), IE_Released, this, &AMilkyWayPawn::OnDiveButtonReleased);

	//Add input mapping context
	if (APlayerController* PlayerController = Cast<APlayerController>(Controller))
	{
		//if(UEnhancedInputLocalPlayerSubsystem* Subsystem = )
	}

}

void AMilkyWayPawn::OnLeftStickVertical(float axisValue)
{
	CurrentLeftStick.Y = axisValue;
	if(CurrentLeftStick.Length() > 1)
		CurrentLeftStick.Normalize();
}

void AMilkyWayPawn::OnLeftStickHorizontal(float axisValue)
{
	CurrentLeftStick.X = axisValue;
	if (CurrentLeftStick.Length() > 1)
		CurrentLeftStick.Normalize();
}

void AMilkyWayPawn::OnRightStickVertical(float axisValue)
{
	CurrentRightStick.Y = axisValue;
	if (CurrentRightStick.Length() > 1)
		CurrentRightStick.Normalize();

	float newPitch = CameraTurnSpeed * axisValue * DeltaTime;
	FRotator rotator = FRotator(newPitch, 0, 0);

	SpringArm->AddLocalRotation(rotator);
}

void AMilkyWayPawn::OnRightStickHorizontal(float axisValue)
{
	
	CurrentRightStick.X = axisValue;
	if (CurrentRightStick.Length() > 1)
		CurrentRightStick.Normalize();

	float newYaw = CameraTurnSpeed * axisValue * DeltaTime;
	FRotator rotator = FRotator(0, newYaw, 0);

	SpringArm->AddWorldRotation(rotator);
}

void AMilkyWayPawn::OnJumpButtonPressed()
{
	CurrentJumpButton = true;
	//if (StateMachine->CurrentState == StateMachine->Idle ||
	//	StateMachine->CurrentState == StateMachine->Walk ||
	//	StateMachine->CurrentState == StateMachine->Stop)
	//{
	//	StateMachine->RequestStateChange("Jump");
	//	return;
	//}
	//else if (StateMachine->CurrentState == StateMachine->TurnKick)
	//{
	//	StateMachine->RequestStateChange("SideFlip");
	//	return;
	//}
	//else if (StateMachine->CurrentState == StateMachine->Dive)
	//{
	//	if (OnGround)
	//	{
	//		StateMachine->RequestStateChange("Rollout");
	//		return;
	//	}
	//}
}

void AMilkyWayPawn::OnJumpButtonHeld()
{
	if (PreviousFrameJumpButton == false)
	{
		JumpButtonPressedThisFrame = true;
	}
	else
	{
		JumpButtonPressedThisFrame = false;
	}
}

void AMilkyWayPawn::OnJumpButtonReleased()
{
	CurrentJumpButton = false;
}

void AMilkyWayPawn::OnDiveButtonPressed()
{
	CurrentDiveButton = true;
}

void AMilkyWayPawn::OnDiveButtonHeld()
{
	if (PreviousFrameDiveButton == false)
	{
		DiveButtonPressedThisFrame = true;
	}
	else
	{
		DiveButtonPressedThisFrame = false;
	}
}

void AMilkyWayPawn::OnDiveButtonReleased()
{
	CurrentDiveButton = false;
}

void AMilkyWayPawn::ApplyGroundedMovementScalars()
{
	FHitResult hitResult;
	FVector startLocation = GetActorLocation() + FVector(0, 0, 1) * PlayerHeight;
	FVector endLocation = GetActorLocation() + FVector(0, 0, 1) * -StepHeight;
	if (GetWorld()->LineTraceSingleByChannel(hitResult, startLocation, endLocation, ECC_WorldStatic))
	{
		if (QuerySurfaceType(hitResult.ImpactNormal) == Ground)
		{

			//Scale acceleration and max ground speed based on the ground normal
			float slopeScalar = 1.0f;
			float dot = GetActorForwardVector().Dot(hitResult.ImpactNormal);

			if (dot > 0.0f)
				slopeScalar = (MaxSlopeScalar - 1.0f) * dot + 1.0f;
			else if (dot < 0.0f)
				slopeScalar = (1.0f - MinSlopeScalar) * (1.0f + dot) + MinSlopeScalar;

			//FString debugText = FString::Printf(TEXT("slopeScalar: %f"), slopeScalar);
			//if (GEngine)
			//	GEngine->AddOnScreenDebugMessage(-1, 15.0f, FColor::Yellow, debugText);

			CurrentGroundAcceleration = BaseGroundAcceleration * slopeScalar;
			CurrentMaxGroundSpeed = BaseMaxGroundSpeed * slopeScalar * CurrentLeftStick.Length();
		}
	}
}

void AMilkyWayPawn::Move()
{
	ApplyGroundedMovementScalars();
	PreviousFrameLocation = GetActorLocation();
	FVector attemptedMoveLocation = GetActorLocation() + (AirControlVelocity * DeltaTime) + (CurrentGroundForward * HSpeed * DeltaTime);
	SetActorLocation(WallCollisionCheck(attemptedMoveLocation));
	CeilingCheck();
	FVector vVector = FVector(0, 0, 1) * VSpeed * DeltaTime;
	AddActorLocalOffset(vVector);
}

FVector AMilkyWayPawn::GetCameraRequestedMoveDirection()
{
	FVector leftStick = FVector(CurrentLeftStick.Y, CurrentLeftStick.X, 0);
	if (leftStick.Length() == 0)
	{
		return FVector(0, 0, 0);
	}
	FVector cameraRequestedMoveDirection = UKismetMathLibrary::TransformDirection(Camera->GetComponentTransform(), leftStick);
	cameraRequestedMoveDirection.Z = 0;
	cameraRequestedMoveDirection.Normalize();
	return cameraRequestedMoveDirection;
}

void AMilkyWayPawn::UpdateWallCollisionRayStartPoints()
{
	float smallPlayerRadius = PlayerRadius * 0.99;
	//Bottom-left
	WallCollisionRayStartPoints[0] = UKismetMathLibrary::TransformLocation(GetActorTransform(), FVector(0, -smallPlayerRadius, StepHeight));
	//Bottom-middle
	WallCollisionRayStartPoints[1] = UKismetMathLibrary::TransformLocation(GetActorTransform(), FVector(-smallPlayerRadius, 0, StepHeight));
	//Bottom-right
	WallCollisionRayStartPoints[2] = UKismetMathLibrary::TransformLocation(GetActorTransform(), FVector(0, smallPlayerRadius, StepHeight));

	//Middle-left
	WallCollisionRayStartPoints[3] = UKismetMathLibrary::TransformLocation(GetActorTransform(), FVector(0, -smallPlayerRadius, PlayerHeight * 0.5));
	//Middle-middle
	WallCollisionRayStartPoints[4] = UKismetMathLibrary::TransformLocation(GetActorTransform(), FVector(-smallPlayerRadius, 0, PlayerHeight * 0.5));
	//Middle-Right
	WallCollisionRayStartPoints[5] = UKismetMathLibrary::TransformLocation(GetActorTransform(), FVector(0, smallPlayerRadius, PlayerHeight * 0.5));

	//Top-left
	WallCollisionRayStartPoints[6] = UKismetMathLibrary::TransformLocation(GetActorTransform(), FVector(0, -smallPlayerRadius, PlayerHeight));
	//Top-middle
	WallCollisionRayStartPoints[7] = UKismetMathLibrary::TransformLocation(GetActorTransform(), FVector(-smallPlayerRadius, 0, PlayerHeight));
	//Top-right
	WallCollisionRayStartPoints[8] = UKismetMathLibrary::TransformLocation(GetActorTransform(), FVector(0, smallPlayerRadius, PlayerHeight));
}

void AMilkyWayPawn::MoveWallCollisionRayStartPoints(FVector offset)
{
	//TO-DO: Dynamically determine number of rays
	for (int i = 0; i < 9; ++i)
	{
		WallCollisionRayStartPoints[i] += offset;
	}
}

FVector AMilkyWayPawn::WallCollisionCheck(FVector attemptedMoveLocation)
{
	UpdateWallCollisionRayStartPoints();
	FVector destination = attemptedMoveLocation;
	FVector startProxy = GetActorLocation();
	int numberOfIterations = 20;
	int iterationIndex = 0;

	if (DebugDrawWallCollisionChecks)
	{
		FVector differenceVector = destination - startProxy;
		for (int i = 0; i < 9; ++i)
		{
			FVector endPoint = WallCollisionRayStartPoints[i] + differenceVector;
			if (i == 1 || i == 4 || i == 7)
				endPoint += CurrentGroundForward * 2 * PlayerRadius;

			DrawDebugDirectionalArrow(GetWorld(), WallCollisionRayStartPoints[i], endPoint, 20, FColor::Red, false);
		}
	}
	

	bool headCollision = false;
	for (; iterationIndex < numberOfIterations; ++iterationIndex)
	{
		FVector differenceVector = destination - startProxy;
		FVector hitNormal;
		FVector hitPoint;
		float shortestCollisionDistance = differenceVector.Length() + 2 * PlayerRadius;
		bool wallHit = false;
		for (int i = 0; i < 9; ++i)
		{
			FVector endPoint = WallCollisionRayStartPoints[i] + differenceVector;
			if (i == 1 || i == 4 || i == 7)
				endPoint += CurrentGroundForward * 2 * PlayerRadius;


			FHitResult hitResult;

			if (GetWorld()->LineTraceSingleByChannel(hitResult, WallCollisionRayStartPoints[i], endPoint, ECC_WorldStatic))
			{

				if (QuerySurfaceType(hitResult.ImpactNormal) == Wall)
				{
					if (i == 6 || i == 7 || i == 8)
						headCollision = true;

					if (hitResult.Distance < shortestCollisionDistance)
					{
						wallHit = true;
						shortestCollisionDistance = hitResult.Distance;
						if (i == 1 || i == 4 || i == 7)
							shortestCollisionDistance = shortestCollisionDistance / 2 / PlayerRadius;

						hitNormal = hitResult.ImpactNormal;
						hitPoint = hitResult.ImpactPoint;
					}
				}
				if (QuerySurfaceType(hitResult.ImpactNormal) == Ceiling)
				{
					HSpeed = 0;
					return GetActorLocation();
				}
			}
		}
		if (wallHit)
		{
			//eject from wall
			FVector entryVector = destination - hitPoint;
			FVector flippedNormalizedEntryVector = -entryVector;
			flippedNormalizedEntryVector.Normalize();
			float cosine = hitNormal.Dot(flippedNormalizedEntryVector);
			float ejectionDistance = cosine * entryVector.Length() + PlayerRadius;
			FVector ejectionVector = ejectionDistance * hitNormal;
			LastHitWallVector = hitNormal;
			startProxy = destination;
			destination += ejectionVector;
			//TO-DO: Air control velocity should be reduced by a vector, not a float
			float airControlVelocityScalar = 1 + cosine;
			AirControlVelocity *= airControlVelocityScalar;

			if (StateMachine->CurrentState == StateMachine->Jump ||
				StateMachine->CurrentState == StateMachine->Fall ||
				StateMachine->CurrentState == StateMachine->Dive ||
				StateMachine->CurrentState == StateMachine->WallKick ||
				StateMachine->CurrentState == StateMachine->SideFlip)
			{
				//FString debugText = FString::Printf(TEXT("airControlVelocityScalar: %f"), airControlVelocityScalar);
				//if (GEngine)
				//	GEngine->AddOnScreenDebugMessage(-1, 15.0f, FColor::Yellow, debugText);

				if (HSpeed + AirControlVelocity.Length() > BonkSpeedThreshold && hitNormal.Dot(GetActorForwardVector()) < BonkDotThreshold)
				{
					if(headCollision)
						StateMachine->BonkedThisFrame = true;
				}
			}
			else if (StateMachine->CurrentState == StateMachine->Walk)
			{
				if (HSpeed > BaseMaxGroundSpeed * 1.1)
				{
					if(headCollision)
						StateMachine->BonkedThisFrame = true;
				}
				else
				{
					if(hitNormal.Dot(GetActorForwardVector()) < BonkDotThreshold)
						StateMachine->RequestStateChange("Push");
				}
			}
		}
		else
		{
			break;
		}
	}

	return destination;
}

enum AMilkyWayPawn::SurfaceTypes AMilkyWayPawn::QuerySurfaceType(FVector surfaceNormal)
{
	float surfaceNormalDot = surfaceNormal.Dot(FVector(0, 0, 1));
	if (surfaceNormalDot > WallDotThreshold)
		return Ground;
	else if (surfaceNormalDot < -WallDotThreshold)
		return Ceiling;
	else
		return Wall;
}

void AMilkyWayPawn::GroundCheck()
{
	FHitResult hitResult;
	FVector startLocation = GetActorLocation() + FVector(0, 0, 1) * PlayerHeight;
	FVector endLocation = GetActorLocation() + FVector(0, 0, 1) * -StepHeight;

	if(DebugDrawGroundCollisionCheck)
		DrawDebugDirectionalArrow(GetWorld(), startLocation, endLocation, 20, FColor::Blue, false, -1, 1);

	if (GetWorld()->LineTraceSingleByChannel(hitResult, startLocation, endLocation, ECC_WorldStatic))
	{
		if (QuerySurfaceType(hitResult.ImpactNormal) == Ground)
		{
			CurrentGroundNormal = hitResult.ImpactNormal;
			if (OnGround)
				CurrentGroundForward = GetActorRightVector().Cross(CurrentGroundNormal);
			else
				CurrentGroundForward = GetActorForwardVector();

			SetActorLocation(hitResult.ImpactPoint);
			OnGround = true;
			VSpeed = 0;

			return;
		}
	}
	CurrentGroundNormal = FVector(0, 0, 1);
	CurrentGroundForward = GetActorForwardVector();
	OnGround = false;
}

void AMilkyWayPawn::CeilingCheck()
{
	FHitResult hitResult;
	FVector startLocation = GetActorLocation() + FVector(0, 0, 1) * PlayerHeight;
	FVector endLocation = startLocation + FVector(0, 0, 1) * VSpeed * DeltaTime;
	if (GetWorld()->LineTraceSingleByChannel(hitResult, startLocation, endLocation, ECC_WorldStatic))
	{
		if (QuerySurfaceType(hitResult.ImpactNormal) == Ceiling)
		{
			VSpeed = 0;
		}
	}
}

void AMilkyWayPawn::UpdateAirControl()
{
	//Update air control
	FVector localAirControlAcceleration = UKismetMathLibrary::InverseTransformDirection(GetActorTransform(), GetCameraRequestedMoveDirection() * AirControlAcceleration * DeltaTime);
	//If we are above the base ground speed and the player is still pushing the stick forward, eliminate the forward component of our air acceleration
	if (HSpeed + AirControlVelocity.Length() + localAirControlAcceleration.Length() >= BaseMaxGroundSpeed &&
		GetCameraRequestedMoveDirection().Dot(GetActorForwardVector()) > 0)
		localAirControlAcceleration.X = 0;

	AirControlVelocity += UKismetMathLibrary::TransformDirection(GetActorTransform(), localAirControlAcceleration);
}

float AMilkyWayPawn::GetCurrentGravity()
{
	float currentGravity = abs(VSpeed) / JumpImpulse + MinGravity;
	currentGravity = FMath::Clamp(currentGravity, MinGravity, InitialGravity);

	return currentGravity;
}

float AMilkyWayPawn::GetCurrentTurnSpeed()
{
	//TO-DO: Curve this value non-linearly
	float turnSpeed = (BaseMaxGroundSpeed / HSpeed) * TurnSpeed;
	turnSpeed = FMath::Clamp(turnSpeed, TurnSpeed * 0.5, TurnSpeed);
	return turnSpeed;
}

float AMilkyWayPawn::GetCurrentJumpThrust()
{
	//TO-DO: Curve this value non-linearly
	float jumpThrust = (HSpeed / BaseGroundAcceleration) * JumpThrust;
	jumpThrust = FMath::Clamp(jumpThrust, JumpThrust * MinJumpThrustScalar, JumpThrust * MaxJumpThrustScalar);
	return jumpThrust;
}

void AMilkyWayPawn::RotateTowardCameraRequestedMoveDirection(float turnSpeed)
{
	//Rotate toward camera's requested direction
	float turnScalar = GetCameraRequestedMoveDirection().Dot(GetActorRightVector());
	float turnAmount = turnSpeed * turnScalar * DeltaTime;

	FRotator rotator = FRotator(0, turnAmount, 0);
	AddActorLocalRotation(rotator);
}
