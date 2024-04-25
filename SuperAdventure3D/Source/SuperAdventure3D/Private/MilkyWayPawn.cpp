// Fill out your copyright notice in the Description page of Project Settings.


#include "MilkyWayPawn.h"
#include "GameFramework/SpringArmComponent.h"
#include "Camera/CameraComponent.h"
#include "Components/ArrowComponent.h"
#include "Kismet/KismetMathLibrary.h"


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

void AMilkyWayPawn::OnLeftStickVertical(float AxisValue)
{
	CurrentLeftStick.Y = AxisValue;
	CurrentLeftStick.Normalize();
}

void AMilkyWayPawn::OnLeftStickHorizontal(float AxisValue)
{
	CurrentLeftStick.X = AxisValue;
	CurrentLeftStick.Normalize();
}

void AMilkyWayPawn::OnRightStickVertical(float AxisValue)
{
	if (GEngine)
	{
		//Print debug message
		GEngine->AddOnScreenDebugMessage(-10, 1.f, FColor::Yellow, "Hello");
	}

	CurrentRightStick.Y = AxisValue;
	CurrentRightStick.Normalize();

	float newPitch = CameraTurnSpeed * AxisValue * DeltaTime;
	FRotator rotator = FRotator(newPitch, 0, 0);

	SpringArm->AddLocalRotation(rotator);
}

void AMilkyWayPawn::OnRightStickHorizontal(float AxisValue)
{
	
	CurrentRightStick.X = AxisValue;
	CurrentRightStick.Normalize();

	float newYaw = CameraTurnSpeed * AxisValue * DeltaTime;
	FRotator rotator = FRotator(0, newYaw, 0);

	SpringArm->AddWorldRotation(rotator);
}

void AMilkyWayPawn::Idle_Tick()
{
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
	FVector cameraRequestedMoveDirection = GetCameraRequestedMoveDirection();
	float turnDirection = 1;
	if (cameraRequestedMoveDirection.Dot(GetActorRightVector()) < 0)
	{
		turnDirection = -1;
	}
	float turnAmount = TurnSpeed * turnDirection * DeltaTime;
	FRotator rotator = FRotator(0, turnAmount, 0);
	AddActorLocalRotation(rotator);

	Move();
}

void AMilkyWayPawn::Stop_Tick()
{
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

void AMilkyWayPawn::Move()
{
	FVector newLocation = GetActorLocation() + GetActorForwardVector() * HSpeed;
	SetActorLocation(newLocation);
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
