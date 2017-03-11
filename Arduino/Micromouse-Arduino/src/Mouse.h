#pragma once

#include <Stepper.h>

namespace Mouse
{
	extern Stepper stepperLeft;
	extern Stepper stepperRight;

	void Setup();
	void Run(int steps);
	void RunTest();
};
