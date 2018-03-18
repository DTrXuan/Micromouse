#pragma once

#include <AccelStepper.h>

namespace Mouse
{
	extern AccelStepper stepperLeft;
	extern AccelStepper stepperRight;

	extern float _cmToStepsFactor;
	extern float _stepsToCmFactor;
	extern float _degreesToMsFactor;

	extern int _stepperPulseDelayUs;

	void Setup();

	void Move();
	void Stop();
	void Rotate(int degrees);

	float cmToSteps(float cm);
	float stepsToCm(float steps);

	void RunTest();
};
