#include "Config.h"
#include "Mouse.h"
#include "WallSensors.h"

AccelStepper Mouse::stepperLeft(AccelStepper::DRIVER, Pins::STEPPER_LEFT_STEP, Pins::STEPPER_LEFT_DIR);
AccelStepper Mouse::stepperRight(AccelStepper::DRIVER, Pins::STEPPER_RIGHT_STEP, Pins::STEPPER_RIGHT_DIR);

float Mouse::_cmToStepsFactor = 0;
float Mouse::_stepsToCmFactor = 0;

float Mouse::_degreesToMsFactor = 5;

int Mouse::_stepperPulseDelayUs = 5;

void Mouse::Setup()
{
	stepperLeft.setMinPulseWidth(_stepperPulseDelayUs);
	stepperLeft.setPinsInverted(true, false, true);
	//stepperLeft.setMaxSpeed(10000);
	stepperLeft.setAcceleration(100);
	stepperLeft.setSpeed(10);

	stepperRight.setMinPulseWidth(_stepperPulseDelayUs);
	//stepperRight.setMaxSpeed(10000);
	stepperRight.setAcceleration(100);
	stepperRight.setSpeed(10);
}

void Mouse::RunTest()
{
	if(WallSensors::HasWallAhead())
	{
		Stop();
		Rotate(180);
	}
	else
	{
		Move();
	}
}

void Mouse::Move()
{
	stepperLeft.runSpeed();
	stepperRight.runSpeed();
}

void Mouse::Stop()
{
	stepperLeft.stop();
	stepperRight.stop();
}

void Mouse::Rotate(int degrees)
{
	return;
	stepperLeft.setSpeed(stepperLeft.speed() * -1);

	long timeToRotate = degrees * _degreesToMsFactor;
	long startTime = millis();

	stepperLeft.runSpeed();
	stepperRight.runSpeed();
	while (millis() - startTime < timeToRotate);

	Stop();
	
	stepperLeft.setSpeed(stepperLeft.speed());
}

float Mouse::cmToSteps(float cm)
{
	if (_cmToStepsFactor == 0)
		_cmToStepsFactor = Constants::WHEEL_STEPS_PER_REVOLUTION / (2.0 * 3.14159265358979 * Constants::WHEEL_RADIUS_MM / 10.0);

	return cm * _cmToStepsFactor;
}

float Mouse::stepsToCm(float steps)
{
	if (_stepsToCmFactor == 0)
		_stepsToCmFactor = (2.0 * 3.14159265358979 * Constants::WHEEL_RADIUS_MM / 10.0) / Constants::WHEEL_STEPS_PER_REVOLUTION;

	return steps * _stepsToCmFactor;
}