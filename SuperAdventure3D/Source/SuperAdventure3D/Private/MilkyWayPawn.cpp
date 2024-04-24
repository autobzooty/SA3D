// Fill out your copyright notice in the Description page of Project Settings.


#include "MilkyWayPawn.h"

// Sets default values
AMilkyWayPawn::AMilkyWayPawn()
{
 	// Set this pawn to call Tick() every frame.  You can turn this off to improve performance if you don't need it.
	PrimaryActorTick.bCanEverTick = true;

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
	PlayerInputComponent->BindAxis(TEXT("StickForward"), this, &AMilkyWayPawn::MoveForward);
	PlayerInputComponent->BindAxis(TEXT("StickRight"), this, &AMilkyWayPawn::MoveRight);

}

void AMilkyWayPawn::MoveForward(float AxisValue)
{
	FVector newLocation = GetActorLocation() + GetActorForwardVector() * AxisValue * Speed * DeltaTime;
	SetActorLocation(newLocation);
}

void AMilkyWayPawn::MoveRight(float AxisValue)
{
	FVector newLocation = GetActorLocation() + GetActorRightVector() * AxisValue * Speed * DeltaTime;
	SetActorLocation(newLocation);
}

