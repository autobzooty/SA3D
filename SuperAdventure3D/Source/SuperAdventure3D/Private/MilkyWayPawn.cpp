// Fill out your copyright notice in the Description page of Project Settings.


#include "MilkyWayPawn.h"
#include "GameFramework/SpringArmComponent.h"
#include "Camera/CameraComponent.h"
#include "Components/ArrowComponent.h"
#include "Kismet/KismetMathLibrary.h"
#include "Kismet/KismetSystemLibrary.h"
#include "DrawDebugHelpers.h"



// Sets default values
AMilkyWayPawn::AMilkyWayPawn()
{
	
	// Set this pawn to call Tick() every frame.  You can turn this off to improve performance if you don't need it.
	PrimaryActorTick.bCanEverTick = true;

	// Instantiating your class Components
	RootArrow = CreateDefaultSubobject<UArrowComponent>(TEXT("RootArrow"));
	Camera = CreateDefaultSubobject<UCameraComponent>(TEXT("Camera"));
	SpringArm = CreateDefaultSubobject<USpringArmComponent>(TEXT("SpringArm"));

	RootArrow->SetupAttachment(GetRootComponent());

	//Attach the Spring Arm to the root
	SpringArm->SetupAttachment(RootArrow);

	//Attach the Camera to the SpringArmComponent
	Camera->AttachToComponent(SpringArm, FAttachmentTransformRules::KeepRelativeTransform);

	//Setting default properties of the SpringArmComp
	SpringArm->bUsePawnControlRotation = false;
	SpringArm->bInheritPitch = false;
	SpringArm->bInheritYaw = false;
	SpringArm->bInheritRoll = false;
	SpringArm->bEnableCameraLag = true;
	SpringArm->TargetArmLength = 1000.0f;	
}

// Called when the game starts or when spawned
void AMilkyWayPawn::BeginPlay()
{
	Super::BeginPlay();
	
}

// Called every frame
void AMilkyWayPawn::Tick(float _DeltaTime)
{
	Super::Tick(_DeltaTime);
	DeltaTime = _DeltaTime;

	switch (CurrentState)
	{
		case Idle:
			Idle_Tick();
			break;
		case Walk:
			Walk_Tick();
			break;
		case Stop:
			Stop_Tick();
			break;
		case Jump:
			Jump_Tick();
			break;
		case Fall:
			Fall_Tick();
			break;
	}
}

// Called to bind functionality to input
void AMilkyWayPawn::SetupPlayerInputComponent(UInputComponent* PlayerInputComponent)
{
	Super::SetupPlayerInputComponent(PlayerInputComponent);

	PlayerInputComponent->BindAxis(TEXT("LeftStickVertical"), this, &AMilkyWayPawn::OnLeftStickVertical);
	PlayerInputComponent->BindAxis(TEXT("LeftStickHorizontal"), this, &AMilkyWayPawn::OnLeftStickHorizontal);
	PlayerInputComponent->BindAxis(TEXT("RightStickVertical"), this, &AMilkyWayPawn::OnRightStickVertical);
	PlayerInputComponent->BindAxis(TEXT("RightStickHorizontal"), this, &AMilkyWayPawn::OnRightStickHorizontal);

	//Add input mapping context
	if (APlayerController* PlayerController = Cast<APlayerController>(Controller))
	{
		//if(UEnhancedInputLocalPlayerSubsystem* Subsystem = )
	}

}

void AMilkyWayPawn::OnLeftStickVertical(float axisValue)
{
	CurrentLeftStick.Y = axisValue;
	CurrentLeftStick.Normalize();
}

void AMilkyWayPawn::OnLeftStickHorizontal(float axisValue)
{
	CurrentLeftStick.X = axisValue;
	CurrentLeftStick.Normalize();
}

void AMilkyWayPawn::OnRightStickVertical(float axisValue)
{
	CurrentRightStick.Y = axisValue;
	CurrentRightStick.Normalize();

	float newPitch = CameraTurnSpeed * axisValue * DeltaTime;
	FRotator rotator = FRotator(newPitch, 0, 0);

	SpringArm->AddLocalRotation(rotator);
}

void AMilkyWayPawn::OnRightStickHorizontal(float axisValue)
{
	
	CurrentRightStick.X = axisValue;
	CurrentRightStick.Normalize();

	float newYaw = CameraTurnSpeed * axisValue * DeltaTime;
	FRotator rotator = FRotator(0, newYaw, 0);

	SpringArm->AddWorldRotation(rotator);
}

void AMilkyWayPawn::Idle_Tick()
{
	GroundCheck();
	if (!OnGround)
	{
		CurrentState = Fall;
	}

	if (CurrentLeftStick.Length() > 0)
	{
		//Snap rotation to camera's requested move direction
		FRotator rotation = UKismetMathLibrary::FindLookAtRotation(GetActorLocation(), GetActorLocation() + GetCameraRequestedMoveDirection());
		SetActorRotation(rotation);
		CurrentState = Walk;
	}
}

void AMilkyWayPawn::Walk_Tick()
{
	GroundCheck();
	if (!OnGround)
	{
		CurrentState = Fall;
	}
	if (CurrentLeftStick.Length() == 0)
	{
		CurrentState = Stop;
	}

	//Accelerate in facing direction
	if (HSpeed < BaseMaxGroundSpeed)
	{
		HSpeed += CurrentLeftStick.Length() * GroundAcceleration * DeltaTime;
	}

	//Rotate toward camera's requested direction
	float turnScalar = GetCameraRequestedMoveDirection().Dot(GetActorRightVector());
	float turnAmount = TurnSpeed * turnScalar * DeltaTime;

	FRotator rotator = FRotator(0, turnAmount, 0);
	AddActorLocalRotation(rotator);

	Move();
}

void AMilkyWayPawn::Stop_Tick()
{
	GroundCheck();
	if (!OnGround)
	{
		CurrentState = Fall;
	}
	if (CurrentLeftStick.Length() > 0)
	{
		CurrentState = Walk;
	}
	if (HSpeed == 0)
	{
		CurrentState = Idle;
	}
	else if (HSpeed > 0)
	{
		HSpeed -= GroundDeceleration * DeltaTime;
	}
	else if (HSpeed < 0)
	{
		HSpeed += GroundDeceleration * DeltaTime;
	}
	if (abs(HSpeed) < GroundDeceleration * DeltaTime)
	{
		HSpeed = 0;
	}
	Move();
}

void AMilkyWayPawn::Jump_Tick()
{
	
}

void AMilkyWayPawn::Fall_Tick()
{
	VSpeed -= Gravity * DeltaTime;
	Move();
	GroundCheck();
	if (OnGround)
	{
		CurrentState = Idle;
		VSpeed = 0;
	}
}

void AMilkyWayPawn::Move()
{
	PreviousFrameLocation = GetActorLocation();
	FVector vVector = FVector(0, 0, 1) * VSpeed * DeltaTime;
	SetActorLocation(WallCollisionCheck());
	AddActorLocalOffset(vVector);
}

FVector AMilkyWayPawn::GetCameraRequestedMoveDirection()
{
	FVector leftStick = FVector(CurrentLeftStick.Y, CurrentLeftStick.X, 0);
	if (leftStick.Length() == 0)
	{
		return GetActorForwardVector();
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
	WallCollisionRayStartPoints[1] = UKismetMathLibrary::TransformLocation(GetActorTransform(), FVector(smallPlayerRadius, 0, StepHeight));
	//Bottom-right
	WallCollisionRayStartPoints[2] = UKismetMathLibrary::TransformLocation(GetActorTransform(), FVector(0, smallPlayerRadius, StepHeight));

	//Middle-left
	WallCollisionRayStartPoints[3] = UKismetMathLibrary::TransformLocation(GetActorTransform(), FVector(0, -smallPlayerRadius, PlayerHeight * 0.5));
	//Middle-middle
	WallCollisionRayStartPoints[4] = UKismetMathLibrary::TransformLocation(GetActorTransform(), FVector(smallPlayerRadius, 0, PlayerHeight * 0.5));
	//Middle-Right
	WallCollisionRayStartPoints[5] = UKismetMathLibrary::TransformLocation(GetActorTransform(), FVector(0, smallPlayerRadius, PlayerHeight * 0.5));

	//Top-left
	WallCollisionRayStartPoints[6] = UKismetMathLibrary::TransformLocation(GetActorTransform(), FVector(0, -smallPlayerRadius, PlayerHeight));
	//Top-middle
	WallCollisionRayStartPoints[7] = UKismetMathLibrary::TransformLocation(GetActorTransform(), FVector(smallPlayerRadius, 0, PlayerHeight));
	//Top-right
	WallCollisionRayStartPoints[8] = UKismetMathLibrary::TransformLocation(GetActorTransform(), FVector(0, smallPlayerRadius, PlayerHeight));
}

FVector AMilkyWayPawn::WallCollisionCheck()
{
	FVector lineTraceOffset = GetActorForwardVector() * HSpeed * DeltaTime;
	UpdateWallCollisionRayStartPoints();

	FVector hitNormal;
	FVector hitPoint;
	float shortestCollisionDistance = HSpeed * DeltaTime;
	for (int i = 0; i < 9; ++i)
	{
		if (DebugDrawWallCollisionChecks)
			DrawDebugDirectionalArrow(GetWorld(), WallCollisionRayStartPoints[i], WallCollisionRayStartPoints[i] + lineTraceOffset, 10, FColor::Red, false);

		FHitResult hitResult;
		if (GetWorld()->LineTraceSingleByChannel(hitResult, WallCollisionRayStartPoints[i], WallCollisionRayStartPoints[i] + lineTraceOffset, ECC_WorldStatic))
		{
			if (QuerySurfaceType(hitResult.ImpactNormal) == Wall)
			{
				if (hitResult.Distance < shortestCollisionDistance)
				{
					shortestCollisionDistance = hitResult.Distance;
					hitNormal = hitResult.ImpactNormal;
					hitPoint = hitResult.ImpactPoint;
				}
			}
		}
	}
	if (shortestCollisionDistance < HSpeed * DeltaTime)
	{
		//eject from wall
		FVector attemptedMoveLocation = GetActorLocation() + GetActorForwardVector() * HSpeed * DeltaTime;
		FVector entryVector = attemptedMoveLocation - hitPoint;
		FVector flippedNormalizedEntryVector = -entryVector;
		flippedNormalizedEntryVector.Normalize();
		float cosine = hitNormal.Dot(flippedNormalizedEntryVector);
		float ejectionDistance = cosine * entryVector.Length() + PlayerRadius;
		FVector ejectionVector = ejectionDistance * hitNormal;
		FVector newPosition = attemptedMoveLocation + ejectionVector;

		return newPosition;
	}
	else
	{
		return GetActorLocation() + GetActorForwardVector() * HSpeed * DeltaTime;
	}	
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
	if (GetWorld()->LineTraceSingleByChannel(hitResult, startLocation, endLocation, ECC_WorldStatic))
	{
		if (QuerySurfaceType(hitResult.ImpactNormal) == Ground)
		{
			SetActorLocation(hitResult.ImpactPoint);
			OnGround = true;
		}
		else
		{
			OnGround = false;
		}
	}
	else
	{
		OnGround = false;
	}
}
