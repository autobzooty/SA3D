// Fill out your copyright notice in the Description page of Project Settings.


#include "MilkyWayPawnState.h"

MilkyWayPawnState::MilkyWayPawnState(AMilkyWayPawn* owner)
	:Owner(owner), StateMachine(owner->StateMachine)
{

}

MilkyWayPawnState::~MilkyWayPawnState()
{

}
