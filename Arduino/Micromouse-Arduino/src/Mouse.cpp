#include "Mouse.h"
#include "Config.h"
#include "WallSensors.h"

Stepper Mouse::stepperLeft(200, Pins::STEPPER_LEFT_STEP, Pins::STEPPER_LEFT_DIR);
Stepper Mouse::stepperRight(200, Pins::STEPPER_RIGHT_STEP, Pins::STEPPER_RIGHT_DIR);

void Mouse::Setup()
{
	stepperLeft.setSpeed(9999);
	stepperRight.setSpeed(9999);
}

void Mouse::RunTest()
{
	if(WallSensors::HasWallAhead() == false)
	{
		Run(20);
	}
}

void Mouse::Run(int steps)
{
	stepperLeft.step(-steps);
	stepperRight.step(steps);
}