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
}

void AMilkyWayPawn::PreInitializeComponents()
{
	StateMachine->Setup(this);
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

void AMilkyWayPawn::OnJumpButtonPressed()
{
	JumpButtonPressedThisFrame = false;
	bool previousFrameJumpButton = CurrentJumpButton;
	CurrentJumpButton = true;
	if (previousFrameJumpButton != CurrentJumpButton)
	{
		//First frame of button press
		JumpButtonPressedThisFrame = true;
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

void AMilkyWayPawn::OnDiveButtonReleased()
{
	CurrentDiveButton = false;
}

void AMilkyWayPawn::Move()
{
	PreviousFrameLocation = GetActorLocation();
	SetActorLocation(WallCollisionCheck());
	CeilingCheck();
	FVector vVector = FVector(0, 0, 1) * VSpeed * DeltaTime;
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

		if (StateMachine->CurrentState == StateMachine->Jump ||
			StateMachine->CurrentState == StateMachine->Fall ||
			StateMachine->CurrentState == StateMachine->Dive)
		{
			if (HSpeed > BonkSpeedThreshold && hitNormal.Dot(GetActorForwardVector()) < BonkDotThreshold)
			{
				StateMachine->ChangeState("Bonk");
			}
		}
		if (StateMachine->CurrentState == StateMachine->Walk)
		{
			if (HSpeed > BaseMaxGroundSpeed)
			{
				StateMachine->ChangeState("Bonk");
			}
		}
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
			VSpeed = 0;
			return;
		}
	}
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